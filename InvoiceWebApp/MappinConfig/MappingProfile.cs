using AutoMapper;
using InvoiceWebApp.DTOS.InvoiceDTO;
using InvoiceWebApp.DTOS.Account;
using InvoiceWebApp.Models;

namespace InvoiceWebApp.MappinConfig
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<InvoiceDetailDTO, InvoiceDetail>();
            CreateMap<InvoiceDetail, InvoiceDetailDTO>();
            CreateMap<CreateInvoiceDTO, Invoice>()
                .ForMember(dest => dest.InvoiceDetails, opt => opt.MapFrom(src => src.InvoiceDetails));

            CreateMap<RegisterDTO, User>();
            
        }
    }
}
