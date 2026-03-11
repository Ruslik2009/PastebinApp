using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

    
namespace PastebinApp
{

    public class ReactionColorConverter : IValueConverter
    {
        
        private static readonly IBrush DefaultBrush = SolidColorBrush.Parse("#65676B");
        private static readonly IBrush ActiveBrush = Brushes.White; 

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
           
            if (value == null || parameter == null)
                return DefaultBrush;

            try 
            {
                
                int userReaction = System.Convert.ToInt32(value);
                int expected = System.Convert.ToInt32(parameter);

                
                return (userReaction == expected) ? ActiveBrush : DefaultBrush;
            }
            catch 
            {
                return DefaultBrush; 
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

}