using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.EnrollmentProgress;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class EnrollmentProgressService
    {
        private readonly IEnrollmentProgressRepository _enrollmentProgressRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentProgressService(IEnrollmentProgressRepository enrollmentProgressRepository,
            IMapper mapper, IUnitOfWork unitOfWork, IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentProgressRepository = enrollmentProgressRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task CreateEnrollmentProgress(int studentId, EnrollmentProgressCreateDTO enrollmentProgressCreateDTO)
        {
            //Check if the student is really have EnrollementId
             Enrollment? currentEnrollment = await _enrollmentRepository
                .GetByIdAsync(enrollmentProgressCreateDTO.EnrollmentId);
            if (currentEnrollment == null) throw new NotFoundException($"No enrollment with Id = {enrollmentProgressCreateDTO.EnrollmentId}");
            if (studentId != currentEnrollment.StudentId) throw new ForbiddenException($"Student with Id = {studentId} don't have the right to create this enrollment progress");

            EnrollmentProgress enrollmentProgress = _mapper.Map<EnrollmentProgress>(enrollmentProgressCreateDTO);        
            await _enrollmentProgressRepository.AddAsync(enrollmentProgress);

            Enrollment? enrollment = await _enrollmentRepository.GetEnrollmentWithProgressAndCourse(enrollmentProgress.EnrollmentId);
            if (enrollment != null && enrollment.IsCourseCompleted())
            {
                enrollment.CompleteCourse();
            }
            await _unitOfWork.SaveChangesAsync();
        }


    }
}