namespace ContosoUniversityRBAC.Models.SchoolViewModels
{
    public class InstructorIndexData
    {
        public PaginatedList<Instructor> Instructors { get; set; }
        public PaginatedList<Course> Courses { get; set; }
        public PaginatedList<Enrollment> Enrollments { get; set; }
    }
}
