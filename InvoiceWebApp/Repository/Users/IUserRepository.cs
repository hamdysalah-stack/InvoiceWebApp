using InvoiceWebApp.Models;

namespace InvoiceWebApp.Repository.Users
{
    public interface IUserRepository
    {
        User? GetById(int id);
        User? GetByUsername(string username);
        User? GetByEmail(string email);
        User? GetByEmailVerificationToken(string token);
        bool UsernameExists(string username);
        bool EmailExists(string email);
        User Create(User user);
        User Update(User user);
        void Delete(int id);
    }
}
