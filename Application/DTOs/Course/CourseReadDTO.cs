namespace Application.DTOs.Course
{
    public class CourseReadDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public string InstructorName { get; set; } = default!;
        public string InstructorImageUrl { get; set; } = default!;
    }
}
