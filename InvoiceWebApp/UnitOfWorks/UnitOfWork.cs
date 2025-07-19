using InvoiceWebApp.Data;
using InvoiceWebApp.Repository.Users;
using InvoiceWebApp.Repository.Invoice;

namespace InvoiceWebApp.UnitOfWorks
{
    public class UnitOfWork
    {
        public ApplicationDbContext context { get; }
        private UserRepository? _userRepository;
        private InvoiceRepository? _invoiceRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
        }

        public UserRepository userRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(context);
                }
                return _userRepository;
            }
        }

        public InvoiceRepository invoiceRepository
        {
            get
            {
                if (_invoiceRepository == null)
                {
                    _invoiceRepository = new InvoiceRepository(context);
                }
                return _invoiceRepository;
            }
        }

        public int SaveChanges()
        {
            return context.SaveChanges();
        }

  
    }
}