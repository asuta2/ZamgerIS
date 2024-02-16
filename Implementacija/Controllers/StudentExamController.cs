using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ooadproject.Data;
using ooadproject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ooadproject.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class StudentExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Person> _userManager;
        private readonly ExamManager _examManager;

        public StudentExamController(ApplicationDbContext context, UserManager<Person> userManager)
        {
            _context = context;
            _userManager = userManager;
            _examManager = new ExamManager(_context);
        }

        // GET: StudentExam

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.StudentExam.Include(s => s.Course).Include(s => s.Exam);
            return View(await applicationDbContext.ToListAsync());
        }
   
        //Create function that returns every type of open exam and homework as a SelectListItem on the specific course by CourseID
        public async Task<IActionResult> TeacherInput(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var students = await _context.StudentCourse
                .Include(s => s.Student)
                .Where(s => s.CourseID == id)
                .ToListAsync();
            var courses = await _context.Course.Where(c => c.TeacherID == user.Id).ToListAsync();
            var currentCourse = await _context.Course.FindAsync(id);
            var exams = await _context.Exam
                .Where(e => e.CourseID == currentCourse.ID)
                .ToListAsync();

            var examSelectListItems = exams.Select(e => new SelectListItem
            {
                Text = $"{e.Type} - {e.Time:yyyy-MM-dd}",
                Value = e.ID.ToString()
            }).ToList();

            ViewData["Exams"] = new SelectList(examSelectListItems, "Value", "Text");
            ViewData["CurrentCourse"] = currentCourse;
            ViewData["Courses"] = courses;
            ViewData["Students"] = new SelectList(GetFullNames(students), "Value", "Text");


            return View();
        }
       
        public async Task<IActionResult> SaveExamResults()
        {
            var applicationDbContext = _context.StudentExam.Include(s => s.Course).Include(s => s.Exam);
            return View(await applicationDbContext.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> SaveExamResults(int id, string link)
        {
            if (string.IsNullOrEmpty(link))
            {
                return BadRequest(new { error = "Link ne može biti prazan!" });
            }

            var exam = await _context.Exam.FindAsync(id);
            try
            {
                await _examManager.SaveExamResults(exam, link);
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            
            return Ok();
        }

        // GET: StudentExam/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.StudentExam == null)
            {
                return NotFound();
            }

            var studentExam = await _context.StudentExam
                .Include(s => s.Course)
                .Include(s => s.Exam)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (studentExam == null)
            {
                return NotFound();
            }

            return View(studentExam);
        }
        public List<SelectListItem> GetFullNames(List<StudentCourse> owo)
        {
            return owo.Select(item => new SelectListItem
            {
                Text = $"{item.Student.FirstName} {item.Student.LastName}",
                Value = item.ID.ToString()
            }).ToList();
        }

        // GET: StudentExam/Create
        public async Task<IActionResult> Create(int id)
        {
            var students = await _context.StudentCourse
                .Include(s => s.Student)
                .Where(s => s.CourseID == id)
                .ToListAsync();

            var exams = await _context.Exam
                .Include(e => e.Course)
                .Where(e => e.CourseID == id)
                .ToListAsync();

            var examSelectListItems = exams.Select(e => new SelectListItem
            {
                Text = $"{e.Type} - {e.Time:yyyy-MM-dd}",
                Value = e.ID.ToString()
            });

            ViewData["CourseID"] = new SelectList(GetFullNames(students), "Value", "Text");
            ViewData["ExamID"] = new SelectList(examSelectListItems, "Value", "Text");
            ViewData["Courses"] = await _context.Course
                .Where(c => c.TeacherID.ToString() == _userManager.GetUserId(User))
                .ToListAsync();

            return View();
        }

        // POST: StudentExam/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, [Bind("CourseID,ExamID,PointsScored")] StudentExam studentExam)
        {
            if (ModelState.IsValid)
            {
                var exam = await _context.Exam.FindAsync(studentExam.ExamID);

                // Check if the studentExam already exists for that exam
                var existingStudentExam = await _context.StudentExam.FirstOrDefaultAsync(se => se.ExamID == studentExam.ExamID);
                if (existingStudentExam != null)
                {
                    // Replace the line with a properly formatted JSON response
                    return BadRequest(new { error = "Vec su uneseni rezultati za studentov ispit." });
                }

                studentExam.IsPassed = studentExam.PointsScored >= exam.MinimumPoints;

                _context.Add(studentExam);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest(new { error = "Desila se greška" });
        }

        // GET: StudentExam/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.StudentExam == null)
            {
                return NotFound();
            }

            var studentExam = await _context.StudentExam.FindAsync(id);
            if (studentExam == null)
            {
                return NotFound();
            }
            var students = await _context.StudentCourse.Include(s => s.Student).Where(s => s.CourseID == id).ToListAsync();
            //Get list of exams that are open for the course passed by the id as a SelectList which will display the id of the exam
            var exams = await _context.Exam.Include(e => e.Course).Where(e => e.CourseID == id).ToListAsync();
            ViewData["CourseID"] = new SelectList(students, "ID", "Student.FirstName");
            ViewData["ExamID"] = new SelectList(exams, "ID", "Type");
            return View(studentExam);
        }

        // POST: StudentExam/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CourseID,ExamID,PointsScored,IsPassed")] StudentExam studentExam)
        {
            if (id != studentExam.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studentExam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExamExists(studentExam.ID))
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
            var students = await _context.StudentCourse.Include(s => s.Student).Where(s => s.CourseID == id).ToListAsync();
            //Get list of exams that are open for the course passed by the id as a SelectList which will display the id of the exam
            var exams = await _context.Exam.Include(e => e.Course).Where(e => e.CourseID == id).ToListAsync();
            ViewData["CourseID"] = new SelectList(students, "ID", "Student.FirstName");
            ViewData["ExamID"] = new SelectList(exams, "ID", "Type");
            return View(studentExam);
        }

        // GET: StudentExam/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.StudentExam == null)
            {
                return NotFound();
            }

            var studentExam = await _context.StudentExam
                .Include(s => s.Course)
                .Include(s => s.Exam)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (studentExam == null)
            {
                return NotFound();
            }

            return View(studentExam);
        }

        // POST: StudentExam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.StudentExam == null)
            {
                return Problem("Entity set 'ApplicationDbContext.StudentExam'  is null.");
            }
            var studentExam = await _context.StudentExam.FindAsync(id);
            if (studentExam != null)
            {
                _context.StudentExam.Remove(studentExam);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExamExists(int id)
        {
          return (_context.StudentExam?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
