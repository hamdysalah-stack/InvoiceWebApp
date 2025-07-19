using BCrypt.Net;
using System;
using InvoiceWebApp.DTOS.Account;
using InvoiceWebApp.Models;
using InvoiceWebApp.UnitOfWorks;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;

namespace InvoiceWebApp.Services
{
    public class UserService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly EmailService _emailService;
        private readonly IMapper _mapper;


        public UserService(UnitOfWork unitOfWork, IMapper mapper, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public User? Authenticate(string username, string password)
        {
            try
            {
                var user = _unitOfWork.userRepository.GetByUsername(username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return null;

                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Register(RegisterDTO model)
        {
            if (_unitOfWork.userRepository.GetByUsername(model.Username) != null)
                throw new InvalidOperationException("Username already exists.");

            if (_unitOfWork.userRepository.GetByEmail(model.Email) != null)
                throw new InvalidOperationException("Email already exists.");

            var verificationToken = Guid.NewGuid().ToString();

            var user = _mapper.Map<User>(model);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.EmailVerificationToken = verificationToken;
            user.IsEmailVerified = false;
            user.CreatedAt = DateTime.UtcNow;

            _unitOfWork.userRepository.Create(user);
            _unitOfWork.SaveChanges();

            try
            {
                Console.WriteLine($"Attempting to send verification email to: {user.Email}");
                var emailSent = _emailService.SendVerificationEmail(user.Email, user.FullName, verificationToken);
                if (emailSent)
                {
                    Console.WriteLine($"Verification email sent successfully to {user.Email}");
                }
                else
                {
                    Console.WriteLine($"Failed to send verification email to {user.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email: {ex.Message}");
            }
        }

        public bool VerifyEmail(string token)
        {
            try
            {
                var user = _unitOfWork.userRepository.GetByEmailVerificationToken(token);

                if (user == null)
                    return false;

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;

                _unitOfWork.userRepository.Update(user);

                _emailService.SendWelcomeEmail(user.Email, user.FullName);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsUsernameAvailable(string username)
        {
            return !_unitOfWork.userRepository.UsernameExists(username);
        }

        public bool IsEmailAvailable(string email)
        {
            return !_unitOfWork.userRepository.EmailExists(email);
        }
    }
}