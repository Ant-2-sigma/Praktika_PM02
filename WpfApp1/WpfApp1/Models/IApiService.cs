using System.Collections.Generic;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public interface IDataService
    {
        // Учителя
        Task<List<Teacher>> GetTeachersAsync();
        Task<Teacher> AddTeacherAsync(Teacher teacher);
        Task<Teacher> UpdateTeacherAsync(Teacher teacher);
        Task<bool> DeleteTeacherAsync(int id);

        // Школьники
        Task<List<Student>> GetStudentsAsync();
        Task<Student> AddStudentAsync(Student student);
        Task<Student> UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);

        // Олимпиады
        Task<List<Olympiad>> GetOlympiadsAsync();
        Task<Olympiad> AddOlympiadAsync(Olympiad olympiad);
        Task<Olympiad> UpdateOlympiadAsync(Olympiad olympiad);
        Task<bool> DeleteOlympiadAsync(int id);

        // Участия
        Task<List<Participation>> GetParticipationsAsync();
        Task<Participation> AddParticipationAsync(Participation participation);
        Task<Participation> UpdateParticipationAsync(Participation participation);
        Task<bool> DeleteParticipationAsync(int id);

        // Классы
        Task<List<Class>> GetClassesAsync();

        // Сохранение всех изменений
        Task SaveChangesAsync();
    }
}