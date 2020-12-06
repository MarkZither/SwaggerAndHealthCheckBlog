using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationConsole
{
    public class TranslationOptions
    {
        public string TranslationDirectory { get; }
        public int? UsedIn { get; }
        public TranslationOptions(string translationDirectory)
        {
            TranslationDirectory = translationDirectory;
        }
        public TranslationOptions(string translationDirectory, int? usedIn)
        {
            TranslationDirectory = translationDirectory;
            UsedIn = usedIn;
        }
    }
}
