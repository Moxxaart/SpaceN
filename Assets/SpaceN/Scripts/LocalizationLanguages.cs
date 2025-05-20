namespace SpaceN.Scripts
{
    public static class LocalizationLanguages
    {
        public static string[] GetLanguageNames()
        {
            return System.Enum.GetNames(typeof(Language));
        }
    
        // Можно также добавить свойство для текущего языка, если удобно:
        public static Language CurrentLanguage { get; set; } = Language.en;
    
        public static string CurrentLanguageName => CurrentLanguage.ToString();
    }
}