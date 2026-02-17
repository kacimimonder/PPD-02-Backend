using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Interfaces.Utilities;
using Domain.Interfaces;
using Domain.Entities;
using Application.DTOs.Enrollment;

namespace Application.Services
{
    public class EnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMapper _mapper;
        public EnrollmentService(IEnrollmentRepository enrollmentRepository, IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _mapper = mapper;
        }

        public async Task AddEnrollmentAsync(int courseId,int studentId)
        {
            Enrollment enrollment = new Enrollment();
            enrollment.CourseId = courseId;
            enrollment.StudentId = studentId;
            await _enrollmentRepository.AddAsync(enrollment);
        }

        public async Task<List<EnrollmentReadDTO>> GetEnrolledCoursesByStudentId(int studentId)
        {
            List<Enrollment> enrollments = await _enrollmentRepository
                .GetEnrolledCoursesByStudentId(studentId);

            return _mapper.Map<List<EnrollmentReadDTO>>(enrollments);
        }

        public async Task<EnrollmentReadDTO> GetEnrollmentByCourseIdAndStudentId(int courseId,int studentId)
        {
            Enrollment? enrollment = await _enrollmentRepository.GetEnrollmentByCourseIdAndStudentId(courseId,studentId);
            EnrollmentReadDTO? enrollmentRead = _mapper.Map<EnrollmentReadDTO>(enrollment);
            return enrollmentRead;
        }

    }
}
