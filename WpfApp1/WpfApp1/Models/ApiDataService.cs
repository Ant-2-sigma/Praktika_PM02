using System.Collections.Generic;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class ApiDataService : IDataService
    {
        // --- Учителя ---
        public async Task<List<Teacher>> GetTeachersAsync()
            => await ApiClient.GetAsync<Teacher>("teachers");

        public async Task<Teacher> AddTeacherAsync(Teacher teacher)
            => await ApiClient.PostAsync("teachers", teacher);

        public async Task<Teacher> UpdateTeacherAsync(Teacher teacher)
            => await ApiClient.PutAsync("teachers", teacher.ID, teacher);

        public async Task<bool> DeleteTeacherAsync(int id)
            => await ApiClient.DeleteAsync("teachers", id);

        // --- Школьники ---
        public async Task<List<Student>> GetStudentsAsync()
            => await ApiClient.GetAsync<Student>("students");

        public async Task<Student> AddStudentAsync(Student student)
            => await ApiClient.PostAsync("students", student);

        public async Task<Student> UpdateStudentAsync(Student student)
            => await ApiClient.PutAsync("students", student.ID, student);

        public async Task<bool> DeleteStudentAsync(int id)
            => await ApiClient.DeleteAsync("students", id);

        // --- Олимпиады ---
        public async Task<List<Olympiad>> GetOlympiadsAsync()
            => await ApiClient.GetAsync<Olympiad>("olympiads");

        public async Task<Olympiad> AddOlympiadAsync(Olympiad olympiad)
            => await ApiClient.PostAsync("olympiads", olympiad);

        public async Task<Olympiad> UpdateOlympiadAsync(Olympiad olympiad)
            => await ApiClient.PutAsync("olympiads", olympiad.ID, olympiad);

        public async Task<bool> DeleteOlympiadAsync(int id)
            => await ApiClient.DeleteAsync("olympiads", id);

        // --- Участия ---
        public async Task<List<Participation>> GetParticipationsAsync()
            => await ApiClient.GetAsync<Participation>("participations");

        public async Task<Participation> AddParticipationAsync(Participation participation)
            => await ApiClient.PostAsync("participations", participation);

        public async Task<Participation> UpdateParticipationAsync(Participation participation)
            => await ApiClient.PutAsync("participations", participation.ID, participation);

        public async Task<bool> DeleteParticipationAsync(int id)
            => await ApiClient.DeleteAsync("participations", id);

        // --- Классы ---
        public async Task<List<Class>> GetClassesAsync()
            => await ApiClient.GetAsync<Class>("classes");

        // --- Сохранение (в API-режиме не требуется) ---
        public async Task SaveChangesAsync()
        {
            await Task.CompletedTask;
        }
    }
}