using Application.DTOs.Course;
using Application.DTOs.CourseModule;
using Application.DTOs.Enrollment;
using Application.DTOs.EnrollmentProgress;
using Application.DTOs.ModuleContent;
using Application.DTOs.Other;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Domain.Models;

namespace Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Course
            CreateMap<Course, CourseReadDTO>()
        .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.FullName))
        .ForMember(dest => dest.InstructorImageUrl, opt => opt.MapFrom(src => src.Instructor.PhotoUrl));

            CreateMap<FilterCoursesDTO,FilterCoursesModel >();
            CreateMap<CourseModule, CourseModuleReadDTO>();

            CreateMap<Course, CourseReadFullDTO>()
        .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.FullName))
        .ForMember(dest => dest.InstructorImageUrl, opt => opt.MapFrom(src => src.Instructor.PhotoUrl))
        .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language.Name))
        .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Subject.Name))
        .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.CourseModules));

            CreateMap<CourseCreateDTO, Course > ();

            //User
            CreateMap<UserCreateDTO, User>();
            CreateMap<User, UserReadDTO>();



            //Enrollment
            CreateMap<EnrollmentCreateDTO, Enrollment>();


            //CourseModule
            CreateMap<CourseModule,CourseModuleReadDTO>()
                .ForMember(dest => dest.ModuleContents, opt => opt.MapFrom(src => src.ModuleContents));
            CreateMap<CourseModuleCreateDTO, CourseModule>();


            //ModuleContent
            CreateMap<ModuleContent, ModuleContentReadDTO>();
            CreateMap<ModuleContentCreateDTO, ModuleContent>();
            CreateMap<ModuleContentReadDTO, ModuleContent>();
            CreateMap<ModuleContentUpdateDTO, ModuleContent>();


            //Enrollment
            CreateMap<Enrollment, EnrollmentReadDTO>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.EnrollmentProgress,opt => opt.MapFrom(src => src.enrollmentProgresses));
        
        
        
            //EnrollmentProgress
            CreateMap<EnrollmentProgress,EnrollmentProgressCreateDTO>();
            CreateMap<EnrollmentProgressCreateDTO, EnrollmentProgress>();
        }


    }
}
