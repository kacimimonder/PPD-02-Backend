namespace Application.DTOs.Course
{
    public class CourseCreateDTO
    {
        //public int? Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public int? SubjectID { get; set; }
        public int LanguageID { get; set; } = default!;
        public string Level { get; set; } = default!;
        public int InstructorID { get; set; } = default!;
    }
}
