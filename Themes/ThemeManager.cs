namespace WPF.Themes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Diagnostics;

    public static class ThemeManager
    {
        public static ResourceDictionary GetThemeResourceDictionary(string theme, string file)
        {
            if (theme != null)
            {
                //Assembly assembly = Assembly.LoadFrom("WPF.Themes.dll");
                string packUri = String.Format("Themes/{0}/{1}", theme, file);
                return Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            }
            return null;
        }

        public static string[] GetThemes()
        {
            string[] themes = new string[] 
            { 
                "Metro",
                "MetroDark",
            };
            return themes;
        }

        public static void ApplyTheme(this Application app, string theme)
        {
            ResourceDictionary dictionary = ThemeManager.GetThemeResourceDictionary(theme, "Theme.xaml");

            if (dictionary != null)
            {
                //適用済みのテーマを削除する
                for (int i = 0; i < app.Resources.MergedDictionaries.Count; ++i)
                {
                    ResourceDictionary item = app.Resources.MergedDictionaries[i];
                    if (item.Source == null) continue;
                    Debug.WriteLine("ApplyTheme(app) LocalPath=" + item.Source.OriginalString);
                    if (item.Source.OriginalString.Contains("Themes") == true)
                    {
                        app.Resources.MergedDictionaries.RemoveAt(i);
                        --i;
                    }
                }

                ResourceDictionary dic_common = ThemeManager.GetThemeResourceDictionary(theme, "Common.xaml");
                app.Resources.MergedDictionaries.Add(dic_common);
                /*
                ResourceDictionary dic_button = ThemeManager.GetThemeResourceDictionary(theme, "ButtonStyle.xaml");
                app.Resources.MergedDictionaries.Add(dic_button);*/
                app.Resources.MergedDictionaries.Add(dictionary);
            }
        }

        public static void ApplyTheme(this ContentControl control, string theme)
        {
            ResourceDictionary dictionary = ThemeManager.GetThemeResourceDictionary(theme, "Theme.xaml");

            if (dictionary != null)
            {
                //適用済みのテーマを削除する
                for(int i = 0; i < control.Resources.MergedDictionaries.Count;++i)
                {
                    ResourceDictionary item = control.Resources.MergedDictionaries[i];
                    if (item.Source == null) continue;
                    Debug.WriteLine("ApplyTheme(control) LocalPath=" + item.Source.OriginalString);
                    if(item.Source.OriginalString.Contains("Themes") == true)
                    {
                        control.Resources.MergedDictionaries.RemoveAt(i);
                        --i;
                    }
                }

                //新しいテーマを適用する
                ResourceDictionary dic_common = ThemeManager.GetThemeResourceDictionary(theme, "Common.xaml");

                control.Resources.MergedDictionaries.Add(dic_common);
                control.Resources.MergedDictionaries.Add(dictionary);
            }
        }

        #region Theme

        /// <summary>
        /// Theme Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.RegisterAttached("Theme", typeof(string), typeof(ThemeManager),
                new FrameworkPropertyMetadata((string)string.Empty,
                    new PropertyChangedCallback(OnThemeChanged)));

        /// <summary>
        /// Gets the Theme property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static string GetTheme(DependencyObject d)
        {
            return (string)d.GetValue(ThemeProperty);
        }

        /// <summary>
        /// Sets the Theme property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetTheme(DependencyObject d, string value)
        {
            d.SetValue(ThemeProperty, value);
        }

        /// <summary>
        /// Handles changes to the Theme property.
        /// </summary>
        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string theme = e.NewValue as string;
            if (theme == string.Empty)
                return;

            ContentControl control = d as ContentControl;
            if (control != null)
            {
                control.ApplyTheme(theme);
            }
        }

        #endregion



    }
}
