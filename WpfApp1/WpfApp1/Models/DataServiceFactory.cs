namespace WpfApp1.Models
{
    public static class DataServiceFactory
    {
        public enum DataSourceMode
        {
            Database,
            Api
        }

        private static DataSourceMode _currentMode = DataSourceMode.Database;
        private static IDataService _service;

        public static DataSourceMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                _service = null; // Сброс кэша при смене режима
            }
        }

        public static IDataService GetService()
        {
            if (_service == null)
            {
                switch (_currentMode)
                {
                    case DataSourceMode.Api:
                        _service = new ApiDataService();
                        break;
                    default:
                        _service = new DbDataService();
                        break;
                }
            }
            return _service;
        }
    }
}