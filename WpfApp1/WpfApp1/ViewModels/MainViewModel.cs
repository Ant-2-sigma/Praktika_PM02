using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Models;
using OfficeOpenXml; // Для работы с Excel
using Microsoft.Win32; // Для диалоговых окон
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WpfApp1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MunicipalOlympiadsContext _context;

        public ObservableCollection<Teacher> Teachers { get; set; }
        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Olympiad> Olympiads { get; set; }
        public ObservableCollection<Participation> Participations { get; set; }
        public ObservableCollection<Class> Classes { get; set; }

        private object _selectedItem;
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _currentView;
        public string CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        // Основные команды
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public ICommand TestApiCommand { get; set; }

        // Команды навигации
        public ICommand ShowTeachersCommand { get; set; }
        public ICommand ShowStudentsCommand { get; set; }
        public ICommand ShowOlympiadsCommand { get; set; }
        public ICommand ShowParticipationsCommand { get; set; }

        // Команды Excel
        public ICommand ExportExcelCommand { get; set; }
        public ICommand ImportExcelCommand { get; set; }

        public MainViewModel()
        {
            Teachers = new ObservableCollection<Teacher>();
            Students = new ObservableCollection<Student>();
            Olympiads = new ObservableCollection<Olympiad>();
            Participations = new ObservableCollection<Participation>();
            Classes = new ObservableCollection<Class>();

            try
            {
                _context = new MunicipalOlympiadsContext();

                if (!CheckDatabaseConnection()) return;

                _context.Database.CreateIfNotExists();
                SeedData();

                LoadData();

                InitializeCommands();
                CurrentView = "Teachers";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска: {ex.Message}", "Ошибка");
            }
        }

        private bool CheckDatabaseConnection()
        {
            try
            {
                using (var conn = new SqlConnection(_context.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Нет подключения к БД!\nПроверьте сервер DESKTOP-159LLU1\\KAMI0YASU\nОшибка: {ex.Message}", "Ошибка");
                return false;
            }
        }

        private void SeedData()
        {
            if (!_context.Classes.Any())
            {
                _context.Classes.Add(new Class { Название = "9А" });
                _context.SaveChanges();
            }
            if (!_context.Teachers.Any())
            {
                _context.Teachers.Add(new Teacher { ФИО = "Иванов И.И.", Телефон = "89001112233", Email = "ivanov@school.ru" });
                _context.SaveChanges();
            }
            if (!_context.Olympiads.Any())
            {
                _context.Olympiads.Add(new Olympiad { Название = "Математика", Предмет = "Математика", Дата_проведения = DateTime.Now });
                _context.SaveChanges();
            }
        }

        private void LoadData()
        {
            var teachers = _context.Teachers.ToList();
            var classes = _context.Classes.ToList();
            var olympiads = _context.Olympiads.ToList();
            var students = _context.Students.Include(s => s.Class).ToList();
            var participations = _context.Participations
                .Include(p => p.Student)
                .Include(p => p.Olympiad)
                .Include(p => p.Teacher)
                .Include(p => p.ExecutedClass)
                .ToList();

            ReplaceCollection(Teachers, teachers);
            ReplaceCollection(Classes, classes);
            ReplaceCollection(Olympiads, olympiads);
            ReplaceCollection(Students, students);
            ReplaceCollection(Participations, participations);

            NotifyUIRefresh();
        }

        private void ReplaceCollection<T>(ObservableCollection<T> target, System.Collections.Generic.List<T> source)
        {
            target.Clear();
            foreach (var item in source) target.Add(item);
        }

        private void NotifyUIRefresh()
        {
            OnPropertyChanged(nameof(Teachers));
            OnPropertyChanged(nameof(Students));
            OnPropertyChanged(nameof(Olympiads));
            OnPropertyChanged(nameof(Participations));
            OnPropertyChanged(nameof(Classes));

            // Хак для принудительного обновления UI
            if (Application.Current.MainWindow != null)
            {
                var temp = Application.Current.MainWindow.DataContext;
                Application.Current.MainWindow.DataContext = null;
                Application.Current.MainWindow.DataContext = temp;
            }
        }

        private void InitializeCommands()
        {
            TestApiCommand = new RelayCommand(async _ => await TestApiAsync());

            AddCommand = new RelayCommand(_ => AddItem());
            EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);
            DeleteCommand = new RelayCommand(_ => DeleteItem(), _ => SelectedItem != null);
            SaveCommand = new RelayCommand(_ => SaveChanges());

            ShowTeachersCommand = new RelayCommand(_ => CurrentView = "Teachers");
            ShowStudentsCommand = new RelayCommand(_ => CurrentView = "Students");
            ShowOlympiadsCommand = new RelayCommand(_ => CurrentView = "Olympiads");
            ShowParticipationsCommand = new RelayCommand(_ => CurrentView = "Participations");

            // Инициализация команд Excel
            ExportExcelCommand = new RelayCommand(_ => ExportToExcel());
            ImportExcelCommand = new RelayCommand(_ => ImportFromExcel());
        }

        private async Task TestApi()
        {
            try
            {
                var users = await LoadUsersAsync();

                var window = new ApiUsersWindow(users);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка API: " + ex.Message);
            }
        }

        private async Task TestApiAsync()
        {
            try
            {
                string apiUrl = "https://jsonplaceholder.typicode.com/users";

                using (HttpClient client = new HttpClient())
                {
                    var json = await client.GetStringAsync(apiUrl);

                    // 💥 ВОТ ЭТО СЮДА
                    var apiUsers = JsonConvert.DeserializeObject<List<ApiUserDto>>(json);

                    // дальше работа с данными
                    string preview = "";

                    foreach (var u in apiUsers)
                    {
                        preview += $"{u.id} | {u.name} | {u.email}\n";
                    }

                    var result = MessageBox.Show(
                        preview + "\n\nСохранить в БД?",
                        "API данные",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        SyncUsers(apiUsers);
                        LoadData();
                        MessageBox.Show("Сохранено!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка API: " + ex.Message);
            }
        }

        private void SyncUsers(List<ApiUserDto> users)
        {
            foreach (var u in users)
            {
                if (!_context.Teachers.Any(t => t.ФИО == u.name))
                {
                    _context.Teachers.Add(new Teacher
                    {
                        ФИО = u.name,
                        Email = u.email,
                        Телефон = "API"
                    });
                }
            }

            _context.SaveChanges();
        }

        private async Task<List<User>> LoadUsersAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync("https://jsonplaceholder.typicode.com/users");
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(json);
            }
        }


        private void ExportToExcel()
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Сохранить отчет в Excel",
                    FileName = $"Olympiads_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true) return;

                using (var package = new ExcelPackage())
                {
                    // 1. Лист Учителя
                    var wsTeachers = package.Workbook.Worksheets.Add("Учителя");
                    wsTeachers.Cells[1, 1].Value = "ID";
                    wsTeachers.Cells[1, 2].Value = "ФИО";
                    wsTeachers.Cells[1, 3].Value = "Телефон";
                    wsTeachers.Cells[1, 4].Value = "Email";

                    int row = 2;
                    foreach (var t in Teachers)
                    {
                        wsTeachers.Cells[row, 1].Value = t.ID;
                        wsTeachers.Cells[row, 2].Value = t.ФИО;
                        wsTeachers.Cells[row, 3].Value = t.Телефон;
                        wsTeachers.Cells[row, 4].Value = t.Email;
                        row++;
                    }

                    // 2. Лист Школьники
                    var wsStudents = package.Workbook.Worksheets.Add("Школьники");
                    wsStudents.Cells[1, 1].Value = "ID";
                    wsStudents.Cells[1, 2].Value = "Фамилия";
                    wsStudents.Cells[1, 3].Value = "Имя";
                    wsStudents.Cells[1, 4].Value = "Отчество";
                    wsStudents.Cells[1, 5].Value = "Класс";
                    wsStudents.Cells[1, 6].Value = "Телефон";

                    row = 2;
                    foreach (var s in Students)
                    {
                        wsStudents.Cells[row, 1].Value = s.ID;
                        wsStudents.Cells[row, 2].Value = s.Фамилия;
                        wsStudents.Cells[row, 3].Value = s.Имя;
                        wsStudents.Cells[row, 4].Value = s.Отчество;
                        wsStudents.Cells[row, 5].Value = s.Class?.Название;
                        wsStudents.Cells[row, 6].Value = s.Телефон;
                        row++;
                    }

                    // 3. Лист Олимпиады
                    var wsOlympiads = package.Workbook.Worksheets.Add("Олимпиады");
                    wsOlympiads.Cells[1, 1].Value = "ID";
                    wsOlympiads.Cells[1, 2].Value = "Название";
                    wsOlympiads.Cells[1, 3].Value = "Предмет";
                    wsOlympiads.Cells[1, 4].Value = "Дата проведения";

                    row = 2;
                    foreach (var o in Olympiads)
                    {
                        wsOlympiads.Cells[row, 1].Value = o.ID;
                        wsOlympiads.Cells[row, 2].Value = o.Название;
                        wsOlympiads.Cells[row, 3].Value = o.Предмет;
                        wsOlympiads.Cells[row, 4].Value = o.Дата_проведения.ToString("dd.MM.yyyy");
                        row++;
                    }

                    // 4. Лист Участия
                    var wsParticipations = package.Workbook.Worksheets.Add("Участия");
                    wsParticipations.Cells[1, 1].Value = "ID";
                    wsParticipations.Cells[1, 2].Value = "Школьник";
                    wsParticipations.Cells[1, 3].Value = "Олимпиада";
                    wsParticipations.Cells[1, 4].Value = "Учитель";
                    wsParticipations.Cells[1, 5].Value = "Баллы";
                    wsParticipations.Cells[1, 6].Value = "Результат";

                    row = 2;
                    foreach (var p in Participations)
                    {
                        wsParticipations.Cells[row, 1].Value = p.ID;
                        wsParticipations.Cells[row, 2].Value = $"{p.Student?.Фамилия} {p.Student?.Имя}";
                        wsParticipations.Cells[row, 3].Value = p.Olympiad?.Название;
                        wsParticipations.Cells[row, 4].Value = p.Teacher?.ФИО;
                        wsParticipations.Cells[row, 5].Value = p.Количество_баллов;
                        wsParticipations.Cells[row, 6].Value = p.Результат_участия;
                        row++;
                    }

                    // Автоподбор ширины столбцов для всех листов
                    foreach (var ws in package.Workbook.Worksheets)
                    {
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        // Делаем заголовки жирными
                        using (var range = ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }

                    package.SaveAs(new FileInfo(saveFileDialog.FileName));
                }

                MessageBox.Show("Данные успешно экспортированы в Excel!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromExcel()
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Выберите файл Excel для импорта"
                };

                if (openFileDialog.ShowDialog() != true) return;

                // Сначала загружаем актуальные справочники, чтобы правильно сопоставлять ID
                LoadData();

                using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                {
                    var workbook = package.Workbook;

                    // 1. Импорт Классов (если есть такой лист)
                    if (workbook.Worksheets.Any(w => w.Name == "Классы"))
                    {
                        ImportClasses(workbook.Worksheets["Классы"]);
                    }

                    // 2. Импорт Учителей
                    if (workbook.Worksheets.Any(w => w.Name == "Учителя"))
                    {
                        ImportTeachers(workbook.Worksheets["Учителя"]);
                    }

                    // 3. Импорт Олимпиад
                    if (workbook.Worksheets.Any(w => w.Name == "Олимпиады"))
                    {
                        ImportOlympiads(workbook.Worksheets["Олимпиады"]);
                    }

                    // Перезагружаем данные после обновления справочников, чтобы получить новые ID
                    LoadData();

                    // 4. Импорт Школьников (зависит от Классов)
                    if (workbook.Worksheets.Any(w => w.Name == "Школьники"))
                    {
                        ImportStudents(workbook.Worksheets["Школьники"]);
                    }

                    // Перезагружаем снова, чтобы получить ID школьников
                    LoadData();

                    // 5. Импорт Участий (зависит от всех остальных)
                    if (workbook.Worksheets.Any(w => w.Name == "Участия"))
                    {
                        ImportParticipations(workbook.Worksheets["Участия"]);
                    }
                }

                MessageBox.Show("Импорт успешно завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при импорте: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Вспомогательные методы импорта ---

        private void ImportClasses(ExcelWorksheet ws)
        {
            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string name = ws.Cells[row, 1].Text; // Предполагаем, что название в 1 колонке
                if (!string.IsNullOrEmpty(name) && !_context.Classes.Any(c => c.Название == name))
                {
                    _context.Classes.Add(new Class { Название = name });
                }
            }
            _context.SaveChanges();
        }

        private void ImportTeachers(ExcelWorksheet ws)
        {
            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string fio = ws.Cells[row, 2].Text; // ФИО во 2 колонке
                if (!string.IsNullOrEmpty(fio) && !_context.Teachers.Any(t => t.ФИО == fio))
                {
                    _context.Teachers.Add(new Teacher
                    {
                        ФИО = fio,
                        Телефон = ws.Cells[row, 3].Text,
                        Email = ws.Cells[row, 4].Text
                    });
                }
            }
            _context.SaveChanges();
        }

        private void ImportOlympiads(ExcelWorksheet ws)
        {
            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string name = ws.Cells[row, 2].Text; // Название во 2 колонке
                if (!string.IsNullOrEmpty(name) && !_context.Olympiads.Any(o => o.Название == name))
                {
                    _context.Olympiads.Add(new Olympiad
                    {
                        Название = name,
                        Предмет = ws.Cells[row, 3].Text,
                        Дата_проведения = DateTime.Now // Дату из текста сложно парсить надежно без формата, ставим текущую
                    });
                }
            }
            _context.SaveChanges();
        }

        private void ImportStudents(ExcelWorksheet ws)
        {
            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string surname = ws.Cells[row, 2].Text;
                string name = ws.Cells[row, 3].Text;
                string className = ws.Cells[row, 5].Text; // Название класса в 5 колонке

                if (!string.IsNullOrEmpty(surname))
                {
                    // Ищем класс по названию
                    var cls = _context.Classes.FirstOrDefault(c => c.Название == className);
                    if (cls != null)
                    {
                        // Проверяем, нет ли уже такого ученика (простая проверка по фамилии и имени)
                        if (!_context.Students.Any(s => s.Фамилия == surname && s.Имя == name && s.Класс_ID == cls.ID))
                        {
                            _context.Students.Add(new Student
                            {
                                Фамилия = surname,
                                Имя = name,
                                Отчество = ws.Cells[row, 4].Text,
                                Телефон = ws.Cells[row, 6].Text,
                                Email = "", // В примере экспорта email был, но в импорте школьника его нет в списке аргументов выше, добавим если нужно
                                Класс_ID = cls.ID
                            });
                        }
                    }
                }
            }
            _context.SaveChanges();
        }

        private void ImportParticipations(ExcelWorksheet ws)
        {
            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string studentName = ws.Cells[row, 2].Text; // "Фамилия Имя"
                string olympiadName = ws.Cells[row, 3].Text;
                string teacherName = ws.Cells[row, 4].Text;

                if (!string.IsNullOrEmpty(studentName) && !string.IsNullOrEmpty(olympiadName))
                {
                    // Парсим имя студента (разделяем по пробелу)
                    var parts = studentName.Split(' ');
                    string surname = parts.Length > 0 ? parts[0] : "";
                    string name = parts.Length > 1 ? parts[1] : "";

                    var student = _context.Students.FirstOrDefault(s => s.Фамилия == surname && s.Имя == name);
                    var olympiad = _context.Olympiads.FirstOrDefault(o => o.Название == olympiadName);
                    var teacher = _context.Teachers.FirstOrDefault(t => t.ФИО == teacherName);

                    if (student != null && olympiad != null)
                    {
                        // Проверяем дубликаты участия
                        if (!_context.Participations.Any(p => p.Школьник_ID == student.ID && p.Олимпиада_ID == olympiad.ID))
                        {
                            _context.Participations.Add(new Participation
                            {
                                Школьник_ID = student.ID,
                                Олимпиада_ID = olympiad.ID,
                                Учитель_ID = teacher?.ID ?? 0, // Если учитель не найден, ставим 0 или первого попавшегося
                                Класс_за_который_выполнял_ID = student.Класс_ID,
                                Количество_баллов = int.TryParse(ws.Cells[row, 5].Text, out int balls) ? balls : 0,
                                Результат_участия = ws.Cells[row, 6].Text
                            });
                        }
                    }
                }
            }
            _context.SaveChanges();
        }

        // --- КОНЕЦ ЛОГИКИ EXCEL ---

        private void AddItem()
        {
            object newItem = null;
            string title = "";

            try
            {
                if (!_context.Classes.Any()) _context.Classes.Add(new Class { Название = "9А" });
                if (!_context.Teachers.Any()) _context.Teachers.Add(new Teacher { ФИО = "Учитель", Телефон = "0", Email = "t@t.ru" });
                if (!_context.Olympiads.Any()) _context.Olympiads.Add(new Olympiad { Название = "Олимпиада", Предмет = "Предмет", Дата_проведения = DateTime.Now });
                _context.SaveChanges();

                switch (CurrentView)
                {
                    case "Teachers":
                        newItem = new Teacher { ФИО = "", Телефон = "", Email = "" };
                        title = "Добавление учителя";
                        break;
                    case "Students":
                        newItem = new Student { Фамилия = "", Имя = "", Отчество = "", Телефон = "", Email = "", Класс_ID = Classes.FirstOrDefault()?.ID ?? 0 };
                        title = "Добавление школьника";
                        break;
                    case "Olympiads":
                        newItem = new Olympiad { Название = "", Предмет = "", Дата_проведения = DateTime.Now };
                        title = "Добавление олимпиады";
                        break;
                    case "Participations":
                        if (!Students.Any() || !Olympiads.Any() || !Teachers.Any()) { MessageBox.Show("Нет данных!"); return; }
                        newItem = new Participation { Школьник_ID = Students.First().ID, Олимпиада_ID = Olympiads.First().ID, Учитель_ID = Teachers.First().ID, Класс_за_который_выполнял_ID = Classes.First().ID, Количество_баллов = 0, Результат_участия = "" };
                        title = "Добавление участия";
                        break;
                }

                if (newItem != null)
                {
                    var dialog = new EditDialog(newItem, "Add", title);
                    dialog.Owner = Application.Current.MainWindow;

                    if (dialog.ShowDialog() == true)
                    {
                        switch (CurrentView)
                        {
                            case "Teachers": _context.Teachers.Add((Teacher)newItem); break;
                            case "Students": _context.Students.Add((Student)newItem); break;
                            case "Olympiads": _context.Olympiads.Add((Olympiad)newItem); break;
                            case "Participations": _context.Participations.Add((Participation)newItem); break;
                        }

                        _context.SaveChanges();
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void EditItem()
        {
            if (SelectedItem == null) return;
            var dialog = new EditDialog(SelectedItem, "Edit", "Редактирование");
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                _context.SaveChanges();
                LoadData();
            }
        }

        private void DeleteItem()
        {
            if (SelectedItem == null) return;

            try
            {
                if (SelectedItem is Teacher t)
                {
                    var full = _context.Teachers
                        .Include(x => x.Participations)
                        .First(x => x.ID == t.ID);

                    _context.Participations.RemoveRange(full.Participations);
                    _context.Teachers.Remove(full);
                }
                else if (SelectedItem is Student s)
                {
                    var full = _context.Students
                        .Include(x => x.Participations)
                        .First(x => x.ID == s.ID);

                    _context.Participations.RemoveRange(full.Participations);
                    _context.Students.Remove(full);
                }
                else if (SelectedItem is Olympiad o)
                {
                    var full = _context.Olympiads
                        .Include(x => x.Participations)
                        .First(x => x.ID == o.ID);

                    _context.Participations.RemoveRange(full.Participations);
                    _context.Olympiads.Remove(full);
                }
                else
                {
                    _context.Entry(SelectedItem).State = EntityState.Deleted;
                }

                _context.SaveChanges();
                LoadData();
                SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
                LoadData();
                MessageBox.Show("Сохранено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SyncTeachersFromApi(List<Teacher> apiTeachers)
        {
            foreach (var apiTeacher in apiTeachers)
            {
                var existing = _context.Teachers
                    .FirstOrDefault(t => t.ID == apiTeacher.ID);

                if (existing == null)
                {
                    // ➕ НОВЫЙ
                    _context.Teachers.Add(new Teacher
                    {
                        ID = apiTeacher.ID,
                        ФИО = apiTeacher.ФИО,
                        Телефон = apiTeacher.Телефон,
                        Email = apiTeacher.Email
                    });
                }
                else
                {
                    // 🔄 ОБНОВЛЕНИЕ
                    existing.ФИО = apiTeacher.ФИО;
                    existing.Телефон = apiTeacher.Телефон;
                    existing.Email = apiTeacher.Email;
                }
            }

            _context.SaveChanges();
        }
    }
}