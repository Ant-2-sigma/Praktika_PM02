using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp1.Models
{
    [Table("Учитель")]
    public class Teacher
    {
        public int ID { get; set; }
        public string ФИО { get; set; }
        public string Телефон { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Participation> Participations { get; set; }

        public Teacher()
        {
            Participations = new HashSet<Participation>();
        }
    }


    [Table("Класс")]
    public class Class
    {
        public int ID { get; set; }
        public string Название { get; set; }

        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Participation> ParticipationsAsExecutedClass { get; set; }

        public Class()
        {
            Students = new HashSet<Student>();
            ParticipationsAsExecutedClass = new HashSet<Participation>();
        }
    }

    [Table("Школьник")]
    public class Student
    {
        public int ID { get; set; }
        public string Фамилия { get; set; }
        public string Имя { get; set; }
        public string Отчество { get; set; }
        public string Email { get; set; }
        public string Телефон { get; set; }
        public int Класс_ID { get; set; }

        [ForeignKey("Класс_ID")]
        public virtual Class Class { get; set; }
        public virtual ICollection<Participation> Participations { get; set; }

        public Student()
        {
            Participations = new HashSet<Participation>();
        }
    }

    [Table("Олимпиада")]
    public class Olympiad
    {
        public int ID { get; set; }
        public string Название { get; set; }
        public string Предмет { get; set; }
        public DateTime Дата_проведения { get; set; }

        public virtual ICollection<Participation> Participations { get; set; }

        public Olympiad()
        {
            Participations = new HashSet<Participation>();
        }
    }

    [Table("Участие_в_олимпиаде")]
    public class Participation
    {
        public int ID { get; set; }
        public int Школьник_ID { get; set; }
        public int Олимпиада_ID { get; set; }
        public int Класс_за_который_выполнял_ID { get; set; }
        public int Учитель_ID { get; set; }
        public int? Количество_баллов { get; set; }
        public string Результат_участия { get; set; }

        [ForeignKey("Школьник_ID")]
        public virtual Student Student { get; set; }

        [ForeignKey("Олимпиада_ID")]
        public virtual Olympiad Olympiad { get; set; }

        [ForeignKey("Класс_за_который_выполнял_ID")]
        public virtual Class ExecutedClass { get; set; }

        [ForeignKey("Учитель_ID")]
        public virtual Teacher Teacher { get; set; }
    }

    public class MunicipalOlympiadsContext : DbContext
    {
        public MunicipalOlympiadsContext() : base("name=MunicipalOlympiadsConnection")
        {
            // Инициализатор базы данных
            Database.SetInitializer(new CreateDatabaseIfNotExists<MunicipalOlympiadsContext>());
        }

        public MunicipalOlympiadsContext(string connectionString) : base(connectionString) { }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Olympiad> Olympiads { get; set; }
        public DbSet<Participation> Participations { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Participation>()
                .HasRequired(p => p.Student)
                .WithMany(s => s.Participations)
                .HasForeignKey(p => p.Школьник_ID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Participation>()
                .HasRequired(p => p.Olympiad)
                .WithMany(o => o.Participations)
                .HasForeignKey(p => p.Олимпиада_ID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Participation>()
                .HasRequired(p => p.ExecutedClass)
                .WithMany(c => c.ParticipationsAsExecutedClass)
                .HasForeignKey(p => p.Класс_за_который_выполнял_ID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Participation>()
                .HasRequired(p => p.Teacher)
                .WithMany(t => t.Participations)
                .HasForeignKey(p => p.Учитель_ID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Student>()
                .HasRequired(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.Класс_ID)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}