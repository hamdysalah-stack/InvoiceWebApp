using AutoMapper;
using InvoiceWebApp.DTOS.InvoiceDTO;
using InvoiceWebApp.Models;
using InvoiceWebApp.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceWebApp.Services
{
    public class InvoiceService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;

        public InvoiceService(UnitOfWork unitOfWork, IMapper mapper, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public IEnumerable<Invoice> GetUserInvoices(int userId)
        {
            try
            {
                return _unitOfWork.invoiceRepository.GetAllByUserId(userId);
            }
            catch (Exception)
            {
                return Enumerable.Empty<Invoice>();
            }
        }

        public Invoice? GetInvoiceById(int id)
        {
            try
            {
                return _unitOfWork.invoiceRepository.GetById(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Invoice CreateInvoice(int userId, CreateInvoiceDTO createInvoiceDto)
        {
            try
            {
                var invoice = new Invoice
                {
                    UserId = userId,
                    Date = createInvoiceDto.Date,
                    InvoiceDetails = _mapper.Map<List<InvoiceDetail>>(createInvoiceDto.InvoiceDetails)
                };

                invoice.TotalAmount = invoice.InvoiceDetails.Sum(d => d.Total);

                var createdInvoice = _unitOfWork.invoiceRepository.Create(invoice);

                var user = _unitOfWork.userRepository.GetById(userId);
                if (user != null && user.IsEmailVerified)
                {
                    _emailService.SendInvoiceCreatedNotification(
                        user.Email,
                        user.FullName,
                        createdInvoice.Id,
                        createdInvoice.TotalAmount
                    );
                }

                return createdInvoice;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Invoice UpdateInvoice(int id, CreateInvoiceDTO updateInvoiceDto)
        {
            try
            {
                var invoice = _unitOfWork.invoiceRepository.GetById(id);

                if (invoice == null)
                    throw new InvalidOperationException("Invoice not found");

                invoice.Date = updateInvoiceDto.Date;
                invoice.InvoiceDetails.Clear();
                foreach (var detailDto in updateInvoiceDto.InvoiceDetails)
                {
                    invoice.InvoiceDetails.Add(_mapper.Map<InvoiceDetail>(detailDto));
                }
                invoice.TotalAmount = invoice.InvoiceDetails.Sum(d => d.Total);

                return _unitOfWork.invoiceRepository.Update(invoice);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteInvoice(int id)
        {
            try
            {
                _unitOfWork.invoiceRepository.Delete(id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}