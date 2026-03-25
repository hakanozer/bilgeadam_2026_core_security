using AutoMapper;
using RestApi.Dtos;
using RestApi.Models;
using RestApi.Models.Dto;
namespace RestApi.Utils
{
    
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Profile
            CreateMap<UserRegisterDto, User>();
            CreateMap<User, UserRegisterDtoResponse>();
            CreateMap<UserLoginDto, User>();

        }
    }

}