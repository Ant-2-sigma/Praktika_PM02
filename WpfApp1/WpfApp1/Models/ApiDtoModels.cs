using Newtonsoft.Json;
using System;

namespace WpfApp1.Models
{
    public class ParticipationDto
    {
        public int ID { get; set; }
        public int Школьник_ID { get; set; }
        public int Олимпиада_ID { get; set; }
        public int Класс_за_который_выполнял_ID { get; set; }
        public int Учитель_ID { get; set; }
        public int? Количество_баллов { get; set; }
        public string Результат_участия { get; set; }

        // Расширенные поля для отображения
        [JsonProperty(PropertyName = "школьник_фио")]
        public string ШкольникФИО { get; set; }

        [JsonProperty(PropertyName = "олимпиада_название")]
        public string ОлимпиадаНазвание { get; set; }

        [JsonProperty(PropertyName = "учитель_фио")]
        public string УчительФИО { get; set; }

        [JsonProperty(PropertyName = "класс_название")]
        public string КлассНазвание { get; set; }
    }

    public class ApiResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}