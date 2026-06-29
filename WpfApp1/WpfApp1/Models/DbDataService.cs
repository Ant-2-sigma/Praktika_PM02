using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class DbDataService : IDataService
    {
        private readonly MunicipalOlympiadsContext _context;

        public DbDataService()
        {
            _context = new MunicipalOlympiadsContext();
        }

        public DbDataService(MunicipalOlympiadsContext context)
        {
            _context = context;
        }

        // --- Учителя ---

        public async Task<List<Teacher>> GetTeachersAsync()
        {
            return await _context.Teachers.ToListAsync();
        }

        public async Task<Teacher> AddTeacherAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task<Teacher> UpdateTeacherAsync(Teacher teacher)
        {
            _context.Entry(teacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return false;
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Школьники ---

        public async Task<List<Student>> GetStudentsAsync()
        {
            return await _context.Students.Include(s => s.Class).ToListAsync();
        }

        public async Task<Student> AddStudentAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task<Student> UpdateStudentAsync(Student student)
        {
            _context.Entry(student).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return false;
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Олимпиады ---

        public async Task<List<Olympiad>> GetOlympiadsAsync()
        {
            return await _context.Olympiads.ToListAsync();
        }

        public async Task<Olympiad> AddOlympiadAsync(Olympiad olympiad)
        {
            _context.Olympiads.Add(olympiad);
            await _context.SaveChangesAsync();
            return olympiad;
        }

        public async Task<Olympiad> UpdateOlympiadAsync(Olympiad olympiad)
        {
            _context.Entry(olympiad).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return olympiad;
        }

        public async Task<bool> DeleteOlympiadAsync(int id)
        {
            var olympiad = await _context.Olympiads.FindAsync(id);
            if (olympiad == null) return false;
            _context.Olympiads.Remove(olympiad);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Участия ---

        public async Task<List<Participation>> GetParticipationsAsync()
        {
            return await _context.Participations
                .Include(p => p.Student)
                .Include(p => p.Olympiad)
                .Include(p => p.Teacher)
                .Include(p => p.ExecutedClass)
                .ToListAsync();
        }

        public async Task<Participation> AddParticipationAsync(Participation participation)
        {
            _context.Participations.Add(participation);
            await _context.SaveChangesAsync();
            return participation;
        }

        public async Task<Participation> UpdateParticipationAsync(Participation participation)
        {
            _context.Entry(participation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return participation;
        }

        public async Task<bool> DeleteParticipationAsync(int id)
        {
            var participation = await _context.Participations.FindAsync(id);
            if (participation == null) return false;
            _context.Participations.Remove(participation);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Классы ---

        public async Task<List<Class>> GetClassesAsync()
        {
            return await _context.Classes.ToListAsync();
        }

        // --- Сохранение ---

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}