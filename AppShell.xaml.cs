using System.Globalization;

namespace Lessop
{
    public partial class AppShell : Shell
    {
        public string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        public AppShell()
        {
            if (lang != "uk" && lang != "ru")
            {
                CultureInfo.CurrentCulture = new CultureInfo("en");
                CultureInfo.CurrentUICulture = new CultureInfo("en");
            }
            InitializeComponent();
            
        }
    }
}
