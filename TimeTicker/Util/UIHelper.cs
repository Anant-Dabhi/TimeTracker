using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TimeTicker
{
   public class UIHelper
    {

        private static double windowMinHeight = 300;
        private static double windowMaxHeight = 493;
        private static double windowHeight = windowMinHeight;
        private static double windowWidth = 350;
        private static double offRight = 3;
        private static double offBottom = 3;

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static double WindowMinHeight
        {
            get
            {
                return windowMinHeight;
            }
        }

        public static double WindowMaxHeight
        {
            get
            {
                return windowMaxHeight;
            }
        }

        public static double WindowHeight
        {
            get
            {
                return windowHeight;
            }
        }

        public static double WindowWidth
        {
            get
            {
                return windowWidth;
            }
        }

        public static double WindowMinimizeTop
        {
            get
            {
                return SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height - WindowMinHeight - offBottom;
            }
        }

        public static double WindowMaximizeTop
        {
            get
            {
                return SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height - WindowMaxHeight - offBottom;
            }
        }


        public static Thickness WindowMinimizedMargin
        {
            get
            {
                return new Thickness(0, WindowMaxHeight - WindowMinHeight, 0, 0); ;
            }
        }

        public static double WindowLeft
        {
            get
            {
                return SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - WindowWidth - offRight;
            }
        }
    }

    public static class Extension
    {
        public static Color ToColor(this string color)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(color);
            }
            catch
            {
                return Colors.Transparent;
            }
        }
    }
}
