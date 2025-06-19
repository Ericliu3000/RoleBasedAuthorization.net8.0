using ContosoUniversityRBAC.Models;
using ContosoUniversityRBAC.Models.UserViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversityRBAC.Data
{
    public class MyDbContext : IdentityDbContext<MyUser,MyRole,string>
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<ContosoUniversityRBAC.Models.Course> Courses { get; set; } = default!;

        public DbSet<ContosoUniversityRBAC.Models.Department> Departments { get; set; } = default!;

        public DbSet<ContosoUniversityRBAC.Models.Student> Students { get; set; } = default!;

        public DbSet<ContosoUniversityRBAC.Models.Instructor> Instructors { get; set; } = default!;
        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<Person> Persons { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Enrollment>().ToTable("Enrollment");
            modelBuilder.Entity<Person>().ToTable("Person");
            modelBuilder.Entity<Student>().ToTable("Person");
            modelBuilder.Entity<Department>().ToTable("Department");
            modelBuilder.Entity<Instructor>().ToTable("Person");
            modelBuilder.Entity<OfficeAssignment>().ToTable("OfficeAssignment");
            modelBuilder.Entity<CourseAssignment>().ToTable("CourseAssignment");


            modelBuilder.Entity<CourseAssignment>()
                .HasKey(c => new { c.CourseID, c.InstructorID });
        }
        public DbSet<ContosoUniversityRBAC.Models.UserViewModels.UserCreateInputModel> UserCreateInputModel { get; set; } = default!;
        public DbSet<ContosoUniversityRBAC.Models.UserViewModels.RoleInputModel> RoleInputModel { get; set; }
    }
}

