using ooadproject.Data;
using ooadproject.Sheets;
using System.Net.WebSockets;

namespace ooadproject.Models
{
    public class ExamManager
    {
        private readonly ApplicationDbContext _context;
        public ExamManager(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SaveExamResults(Exam exam, string link)
        {
            try
            {
                var results = SheetsFacade.GetExamResults(link);

                foreach (KeyValuePair<int, double> result in results)
                {
                    var student = _context.Student.FirstOrDefault(s => s.Index == result.Key);
                    if (student != null)
                    {
                        var newExam = new StudentExam();
                        newExam.Exam = exam;
                        newExam.ExamID = exam.ID;
                        var course = _context.StudentCourse.FirstOrDefault(sc => sc.StudentID == student.Id && sc.CourseID == exam.CourseID);
                        newExam.Course = course;
                        newExam.CourseID = course.ID;

                        newExam.IsPassed = result.Value >= exam.MinimumPoints;
                        newExam.PointsScored = result.Value;

                        await _context.AddAsync(newExam);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
