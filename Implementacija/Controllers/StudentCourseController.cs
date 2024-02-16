using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ooadproject.Data;
using ooadproject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ooadproject.Controllers
{
    public class StudentCourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Person> _userManager;

        public StudentCourseController(ApplicationDbContext context, UserManager<Person> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StudentCourse
        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.StudentCourse.Include(s => s.Course).Include(s => s.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: StudentCourse/Details/5
        //studentcoursetstatus
        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.StudentCourse == null)
            {
                return NotFound();
            }

            var studentCourse = await _context.StudentCourse
                .Include(s => s.Course)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (studentCourse == null)
            {
                return NotFound();
            }

            return View(studentCourse);
        }

        // GET: StudentCourse/Create
        [Authorize(Roles = "StudentService")]
        public IActionResult Create()
        {
            ViewData["CourseID"] = new SelectList(_context.Course, "ID", "Name");

            List<SelectListItem> Students = new List<SelectListItem>();

            foreach (var item in _context.Student)
            {
                Students.Add(new SelectListItem() { Text = $"{item.FirstName} {item.LastName}", Value = item.Id.ToString() });
            }

            ViewData["StudentID"] = Students;
            return View();
        }

        // POST: StudentCourse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "StudentService")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CourseID,StudentID")] StudentCourse studentCourse)
        {
            studentCourse.Points = 0;
            studentCourse.Grade = 5;

            var existingStudentCourse = await _context.StudentCourse.FirstOrDefaultAsync(sc => sc.CourseID == studentCourse.CourseID && sc.StudentID == studentCourse.StudentID);
            if (existingStudentCourse != null)
            {
                return BadRequest(new { error = "Student je već upisan na ovaj kurs!" });
            }

            if (ModelState.IsValid)
            {
                _context.Add(studentCourse);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest(new { error = "Desila se greška pri dodavanju studenta na kurs." });
        }

        [Authorize(Roles = "StudentService")]
        // POST: StudentCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.StudentCourse == null)
            {
                return Problem("Entity set 'ApplicationDbContext.StudentCourse'  is null.");
            }
            var studentCourse = await _context.StudentCourse.FindAsync(id);
            if (studentCourse != null)
            {
                _context.StudentCourse.Remove(studentCourse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentCourseStatus(int? id)
        {
            var user = await _userManager.GetUserAsync(User);

            var studentCourse = await _context.StudentCourse
                .Include(sc => sc.Course)
                .Include(sc => sc.Course.Teacher)
                .FirstOrDefaultAsync(sc => sc.ID == id && sc.StudentID == user.Id);

            if (studentCourse == null)
            {
                return NotFound();
            }

            var studentExams = await _context.StudentExam
                .Include(se => se.Exam)
                .Where(se => se.CourseID == id)
                .ToListAsync();

            var studentHomeworks = await _context.StudentHomework
                .Include(sh => sh.Homework)
                .Where(sh => sh.CourseID == id)
                .ToListAsync();

            var activities = new List<IActivity>();

            double scored = 0;
            double maxPossible = 0;

            foreach (var studentExam in studentExams)
            {
                studentExam.Exam = await _context.Exam.FindAsync(studentExam.ExamID);
                activities.Add(studentExam);
                scored += studentExam.PointsScored;
                maxPossible += studentExam.Exam.TotalPoints;
            }

            foreach (var studentHomework in studentHomeworks)
            {
                studentHomework.Homework = await _context.Homework.FindAsync(studentHomework.HomeworkID);
                activities.Add(studentHomework);
                scored += studentHomework.PointsScored;
                maxPossible += studentHomework.Homework.TotalPoints;
            }

            ViewData["Courses"] = await _context.StudentCourse
                .Include(sc => sc.Course)
                .Where(sc => sc.StudentID == user.Id)
                .ToListAsync();

            ViewData["StudentCourse"] = studentCourse;
            ViewData["Activities"] = activities;
            ViewData["PointsScored"] = scored;
            ViewData["TotalPoints"] = scored;
            ViewData["MaxPossible"] = maxPossible;

            return View();
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentOverallStatus(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            var courses = await _context.StudentCourse
                .Include(sc => sc.Course)
                .Where(sc => sc.StudentID == user.Id && sc.Grade > 5)
                .ToListAsync();

            var gradedCourses = courses
                .OrderByDescending(c => c.Course.Semester)
                .ThenBy(c => c.Course.Name)
                .ToList();

            ViewData["GradedCourses"] = gradedCourses;
            ViewData["Courses"] = courses;

            double averageGrade = 0;
            if (gradedCourses.Any())
            {
                averageGrade = gradedCourses.Average(c => c.Grade);
            }

            ViewData["AverageGrade"] = averageGrade;

            return View();
        }

        private bool StudentCourseExists(int id)
        {
            return (_context.StudentCourse?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
