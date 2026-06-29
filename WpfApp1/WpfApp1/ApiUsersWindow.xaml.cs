using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public partial class ApiUsersWindow : Window
    {
        public ApiUsersWindow(List<User> users)
        {
            InitializeComponent();
            DataContext = users;
        }
    }
}