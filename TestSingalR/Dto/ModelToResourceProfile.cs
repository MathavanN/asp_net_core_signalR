using AutoMapper;
using TestSingalR.Entity;

namespace TestSingalR.Dto
{
    public class ModelToResourceProfile : Profile
    {
        public ModelToResourceProfile()
        {
            CreateMap<AppUser, UserDto>();
            CreateMap<AppUser, AppUserDto>();
            CreateMap<Activity, ActivityDto>();
        }
    }
}
