using System.Globalization;
using System.Resources;
using Lessop.Resources;
using Lessop.Resources.Strings;

namespace Lessop.Converters
{
    public class ResourceConverter : IValueConverter
    {
        private static readonly ResourceManager ResourceManager = new ResourceManager(typeof(AppResources));

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string resourceKey)
            {
                return ResourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture) ?? resourceKey;
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
