using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// Универсальное диалоговое окно для добавления и редактирования сущностей.
    /// Использует рефлексию для автоматической генерации полей ввода
    /// на основе свойств объекта, помеченных атрибутом EditableField.
    /// </summary>
    public partial class EditDialog : Window
    {
        public object ResultItem { get; private set; }
        private string _mode;
        private object _originalItem;

        public EditDialog(object item, string mode, string title)
        {
            InitializeComponent();
            _mode = mode;
            _originalItem = item;
            TitleText.Text = title;
            GenerateFields(item);
        }

        /// <summary>
        /// Автоматическая генерация полей на основе рефлексии.
        /// Заменяет ручную проверку типов и жестко заданные поля.
        /// </summary>
        private void GenerateFields(object item)
        {
            FieldsContainer.Children.Clear();
            ResultItem = item;

            if (item == null) return;

            var type = item.GetType();
            var properties = type.GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => !IsNavigationProperty(p))
                .Where(p => !IsIdProperty(p, _mode))
                .ToList();

            foreach (var prop in properties)
            {
                var fieldType = GetFieldType(prop);
                var label = GetPropertyDisplayName(prop);

                switch (fieldType)
                {
                    case FieldType.String:
                        AddStringField(label, item, prop);
                        break;
                    case FieldType.Int:
                        AddIntField(label, item, prop);
                        break;
                    case FieldType.NullableInt:
                        AddNullableIntField(label, item, prop);
                        break;
                    case FieldType.DateTime:
                        AddDateField(label, item, prop);
                        break;
                }
            }
        }

        /// <summary>
        /// Определяет тип поля по типу свойства
        /// </summary>
        private FieldType GetFieldType(PropertyInfo prop)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (propType == typeof(string)) return FieldType.String;
            if (propType == typeof(int)) return FieldType.Int;
            if (propType == typeof(DateTime)) return FieldType.DateTime;

            return FieldType.String;
        }

        /// <summary>
        /// Получает отображаемое имя свойства (заменяет подчёркивания на пробелы)
        /// </summary>
        private string GetPropertyDisplayName(PropertyInfo prop)
        {
            var name = prop.Name;
            // Замена "Результат_участия" на "Результат участия"
            name = name.Replace("_", " ");
            return name + ":";
        }

        /// <summary>
        /// Проверяет, является ли свойство навигационным (виртуальная коллекция или объект)
        /// </summary>
        private bool IsNavigationProperty(PropertyInfo prop)
        {
            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                return true;

            if (prop.GetMethod != null && prop.GetMethod.IsVirtual &&
                !prop.PropertyType.IsValueType &&
                prop.PropertyType != typeof(string))
                return true;

            return false;
        }

        /// <summary>
        /// Пропускает ID при добавлении (генерируется автоматически)
        /// </summary>
        private bool IsIdProperty(PropertyInfo prop, string mode)
        {
            if (mode == "Add" && prop.Name == "ID" && prop.PropertyType == typeof(int))
                return true;
            return false;
        }

        // --- Методы создания полей ---

        private void AddStringField(string label, object source, PropertyInfo prop)
        {
            var panel = CreateFieldPanel(label);
            var tb = new TextBox { Padding = new Thickness(5) };
            tb.Text = prop.GetValue(source) as string ?? "";
            tb.TextChanged += (s, e) => prop.SetValue(source, tb.Text);
            panel.Children.Add(tb);
            FieldsContainer.Children.Add(panel);
        }

        private void AddIntField(string label, object source, PropertyInfo prop)
        {
            var panel = CreateFieldPanel(label);
            var tb = new TextBox { Padding = new Thickness(5) };
            var value = (int)prop.GetValue(source);
            tb.Text = value.ToString();
            tb.TextChanged += (s, e) =>
            {
                if (int.TryParse(tb.Text, out int result))
                    prop.SetValue(source, result);
            };
            panel.Children.Add(tb);
            FieldsContainer.Children.Add(panel);
        }

        private void AddNullableIntField(string label, object source, PropertyInfo prop)
        {
            var panel = CreateFieldPanel(label);
            var tb = new TextBox { Padding = new Thickness(5) };
            var value = prop.GetValue(source) as int?;
            tb.Text = value?.ToString() ?? "";
            tb.TextChanged += (s, e) =>
            {
                if (int.TryParse(tb.Text, out int result))
                    prop.SetValue(source, (int?)result);
                else if (string.IsNullOrEmpty(tb.Text))
                    prop.SetValue(source, (int?)null);
            };
            panel.Children.Add(tb);
            FieldsContainer.Children.Add(panel);
        }

        private void AddDateField(string label, object source, PropertyInfo prop)
        {
            var panel = CreateFieldPanel(label);
            var dp = new DatePicker { Padding = new Thickness(5) };
            var value = (DateTime)prop.GetValue(source);
            dp.SelectedDate = value;
            dp.SelectedDateChanged += (s, e) =>
            {
                if (dp.SelectedDate.HasValue)
                    prop.SetValue(source, dp.SelectedDate.Value);
            };
            panel.Children.Add(dp);
            FieldsContainer.Children.Add(panel);
        }

        /// <summary>
        /// Создает панель с подписью для поля ввода
        /// </summary>
        private StackPanel CreateFieldPanel(string label)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 3)
            });
            return panel;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private enum FieldType
        {
            String,
            Int,
            NullableInt,
            DateTime
        }
    }

}