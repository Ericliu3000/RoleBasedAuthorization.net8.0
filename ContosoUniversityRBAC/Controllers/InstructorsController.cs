using ContosoUniversityRBAC.Data;
using ContosoUniversityRBAC.Models;
using ContosoUniversityRBAC.Models.SchoolViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sang.AspNetCore.RoleBasedAuthorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversityRBAC.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly ILogger<InstructorsController> _logger;
 

        public InstructorsController(MyDbContext context, ILogger<InstructorsController> logger )
        {
            _logger = logger;
            _context = context;
            
            
        }

        // GET: Instructors
        [Resource("InstructorsRead")]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber , int? id, int? courseID)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["FirstMinNameSortParm"] = sortOrder == "FirstMinName" ? "FirstMinName_desc" : "FirstMinName";

            var viewModel = new InstructorIndexData();

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            
            var instructors = from s in _context.Instructors
                              select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                instructors = instructors.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstMidName.Contains(searchString));

            }
            switch (sortOrder)
            {
                case "FirstMinName":
                    instructors = instructors.OrderBy(s => s.FirstMidName);
                    break;
                case "FirstMinName_desc":
                    instructors = instructors.OrderByDescending(s => s.FirstMidName);
                    break;
                case "name_desc":
                    instructors = instructors.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    instructors = instructors.OrderBy(s => s.HireDate);
                    break;
                case "date_desc":
                    instructors = instructors.OrderByDescending(s => s.HireDate);
                    break;
                default:
                    instructors = instructors.OrderBy(s => s.LastName);
                    break;
            }
           instructors = instructors
                                .Include(i => i.OfficeAssignment)
                                .Include(i => i.CourseAssignments)
                                .ThenInclude(i => i.Course)
                                //    .ThenInclude(i => i.Enrollments)
                                //        .ThenInclude(i => i.Student)
                                //.Include(i => i.CourseAssignments)
                                //.ThenInclude(i => i.Course)
                                 //   .ThenInclude(i => i.Department)
                                .AsNoTracking();
            int pageSize = 3;

            viewModel.Instructors = await PaginatedList<Instructor>.CreateAsync(instructors, pageNumber ?? 1, pageSize);
                        
                  
            if (id != null)
            {
                ViewData["InstructorID"] = id.Value;
                /*
                Instructor instructorCources = instructors.Where(i => i.ID == id.Value)
                                             .Include(i => i.CourseAssignments)
                                                .ThenInclude(i => i.Course)
                                                    .ThenInclude(i => i.Department)
                                                                .AsNoTracking()
                                                    .Single();
               // var courseQuery = instructors.Where(i => i.ID == id.Value).Single().CourseAssignments.Select(s => s.Course);
                var courseQuery = instructorCources.CourseAssignments.Select(s => s.Course);
                */
                // 直接构造数据库查询（IQueryable<Course> 类型）
                var courseQuery = _context.CourseAssignments
                    .Where(ca => ca.InstructorID == id.Value) // 筛选该教师的课程分配记录
                        .Include( i => i.Course)
                            .ThenInclude( i=> i.Department)
                    .Select(ca => ca.Course);  // 投影到 Course 对象（IQueryable<Course> 类型）


                viewModel.Courses = await PaginatedList<Course>.CreateAsync(courseQuery, 1, 1000);
            }

            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;


                var enrollmentQuery = _context.Enrollments.Where(i => i.CourseID == courseID.Value)
                                .Include(i => i.Student);
                                
                viewModel.Enrollments = await PaginatedList<Enrollment>.CreateAsync( enrollmentQuery, 1, 1000);
            }
          
 
            //return View(await PaginatedList<Instructor>.CreateAsync((IQueryable<Instructor>)viewModel.Instructors, pageNumber ?? 1, pageSize));
            return View(viewModel);
        }

        // GET: Instructors/Details/5
        [Resource("InstructorsRead")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Instructors/Create
        [Resource("InstructorsDelete")]
        public IActionResult Create()
        {
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Resource("InstructorsDelete")]
        public async Task<IActionResult> Create([Bind("HireDate,ID,LastName,FirstMidName,OfficeAssignment")] Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new CourseAssignment { InstructorID = instructor.ID, CourseID = int.Parse(course) };
                    instructor.CourseAssignments.Add(courseToAdd);
                }
            }
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(instructor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        [Resource("InstructorsWrite")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var instructor = await _context.Instructors.FindAsync(id);
            var instructor = await _context.Instructors
            .Include(i => i.OfficeAssignment)
            .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }
        /*
        // POST: Instructors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Resource("InstructorsWrite")]
        public async Task<IActionResult> Edit(int id, [Bind("HireDate,ID,LastName,FirstMidName")] Instructor instructor)
        {
            if (id != instructor.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                    if (!InstructorExists(instructor.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(instructor);
        }
        */
        /*
         * 与 Bind 特性对比：
            [Bind(Include)] 是在模型绑定时过滤字段，而 TryUpdateModel 是在已有实体上选择性更新。
            TryUpdateModel 更灵活，可根据运行时条件动态调整允许更新的字段。
         */
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Resource("InstructorsWrite")]
        public async Task<IActionResult> EditPost(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return NotFound();
            }
           // var instructorToUpdate = await _context.Instructors.FirstOrDefaultAsync(s => s.ID == id);
            var instructorToUpdate = await _context.Instructors
               .Include(i => i.OfficeAssignment)
               .Include(i => i.CourseAssignments)
                     .ThenInclude(i => i.Course)
               .FirstOrDefaultAsync(s => s.ID == id);

            if (await TryUpdateModelAsync<Instructor>(
                instructorToUpdate,
                "",
                s => s.FirstMidName, s => s.LastName, s => s.HireDate,s=>s.OfficeAssignment))
            {
                if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                {
                    instructorToUpdate.OfficeAssignment = null;
                }
                UpdateInstructorCourses(selectedCourses, instructorToUpdate);
                try
                {
                    await _context.SaveChangesAsync();
                   
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }
            UpdateInstructorCourses(selectedCourses, instructorToUpdate);
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }
        // GET: Instructors/Delete/5
        [Resource("InstructorsDelete")]
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false )
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }
            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Resource("InstructorsDelete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //var instructor = await _context.Instructors.FindAsync(id);
            Instructor instructor = await _context.Instructors
                .Include(i => i.CourseAssignments)   
                .SingleAsync(i => i.ID == id);
            var departments = await _context.Departments
                       .Where(d => d.InstructorID == id)
                       .ToListAsync();
            departments.ForEach(d => d.InstructorID = null);
            if (instructor == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                    _context.Instructors.Remove(instructor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                 
            }

            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.ID == id);
        }
        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Courses;
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewData["Courses"] = viewModel;
        }
        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (instructorToUpdate.CourseAssignments.Select(c => c.Course.CourseID));
            foreach (var course in _context.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment { InstructorID = instructorToUpdate.ID, CourseID = course.CourseID });
                    }
                }
                else
                {

                    if (instructorCourses.Contains(course.CourseID))
                    {
                        CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments.FirstOrDefault(i => i.CourseID == course.CourseID);
                        _context.Remove(courseToRemove);
                    }
                }
            }
        }
    }
}
