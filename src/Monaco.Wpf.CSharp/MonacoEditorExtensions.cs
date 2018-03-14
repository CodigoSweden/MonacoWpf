using Monaco.Wpf.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Monaco.Wpf
{
    public static class MonacoEditorExtensions
    {
        public static void AddCSharpLanguageService(this MonacoEditor editor, CSharpContext cSharpCtx)
        {
            editor.RegisterCSharpServices(cSharpCtx.Id, new RoslynHandler(cSharpCtx));
        }

        public static bool Like(this string str, string wildcard)
        {
            return new Regex(
                "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }
    }
}
