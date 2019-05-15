using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
    internal static class Decipherer
    {
        public static string DecipherWithVersion(string cipher, string cipherVersion)
        {
            string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/player{0}.js", cipherVersion);
            string baseJs = HttpHelper.DownloadString(jsUrl);

            // Find the name of the function that handles deciphering
            var fnname = Regex.Match(baseJs, @"yt\.akamaized\.net.*encodeURIComponent\((\w+)").Groups[1].Value;
            var _argnamefnbodyresult = Regex.Match(baseJs, fnname + @"=function\((.+?)\){(.+?)}");
            var helpername = Regex.Match(_argnamefnbodyresult.Groups[2].Value, @";(.+?)\..+?\(").Groups[1].Value;
            var helperresult = Regex.Match(baseJs, "var " + helpername + "={[\\s\\S]+?};");
            var result = helperresult.Groups[0].Value;

            MatchCollection matches = Regex.Matches(result, @"[A-Za-z0-9]+:function\s*([A-z0-9]+)?\s*\((?:[^)(]+|\((?:[^)(]+|\([^)(]*\))*\))*\)\s*\{(?:[^}{]+|\{(?:[^}{]+|\{[^}{]*\})*\})*\}");

            var funcs = _argnamefnbodyresult.Groups[2].Value.Split(';');

            var sign = cipher.ToCharArray();

            foreach (string func in funcs)
            {
                foreach (Match group in matches)
                {
                    if (group.Value.Contains("reverse"))
                    {
                        var test = Regex.Match(group.Value, "^(.*?):").Groups[1].Value;

                        if (func.Contains(test))
                        {
                            sign = ReverseFunction(sign);
                        }
                    }
                    else if (group.Value.Contains("splice"))
                    {
                        var test = Regex.Match(group.Value, "^(.*?):").Groups[1].Value;

                        if (func.Contains(test))
                        {
                            sign = SpliceFunction(sign, GetOpIndex(func));
                        }
                    }
                    else
                    {
                        var test = Regex.Match(group.Value, "^(.*?):").Groups[1].Value;

                        if (func.Contains(test))
                        {
                            sign = SwapFunction(sign, GetOpIndex(func));
                        }
                    }
                }
            }
            
            return new string(sign);
        }

        private static char[] SpliceFunction(char[] a, int b)
        {
            return a.Splice(b);
        }

        private static char[] SwapFunction(char[] a, int b)
        {
            var c = a[0];
            a[0] = a[b % a.Length];
            a[b % a.Length] = c;
            return a;
        }

        private static char[] ReverseFunction(char[] a)
        {
            Array.Reverse(a);
            return a;
        }

        //private static string ApplyOperation(string cipher, string op)
        //{
        //    switch (op[0])
        //    {
        //        case 'r':
        //            return new string(cipher.ToCharArray().Reverse().ToArray());

        //        case 'w':
        //            {
        //                int index = GetOpIndex(op);
        //                return SwapFirstChar(cipher, index);
        //            }

        //        case 's':
        //            {
        //                int index = GetOpIndex(op);
        //                return cipher.Substring(index);
        //            }

        //        default:
        //            throw new NotImplementedException("Couldn't find cipher operation.");
        //    }
        //}

        //private static string DecipherWithOperations(string cipher, string operations)
        //{
        //    return operations.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
        //        .Aggregate(cipher, ApplyOperation);
        //}

        //private static string GetFunctionFromLine(string currentLine)
        //{
        //    Regex matchFunctionReg = new Regex(@"\w+\.(?<functionID>\w+)\("); //lc.ac(b,c) want the ac part.
        //    Match rgMatch = matchFunctionReg.Match(currentLine);
        //    string matchedFunction = rgMatch.Groups["functionID"].Value;
        //    return matchedFunction; //return 'ac'
        //}

        private static int GetOpIndex(string op)
        {
            string parsed = new Regex(@".(\d+)").Match(op).Result("$1");
            int index = Int32.Parse(parsed);

            return index;
        }

        //private static string SwapFirstChar(string cipher, int index)
        //{
        //    var builder = new StringBuilder(cipher);
        //    builder[0] = cipher[index];
        //    builder[index] = cipher[0];

        //    return builder.ToString();
        //}
    }
}

public static class Extensions
{
    public static T[] Splice<T>(this T[] source, int start)
    {
        var listItems = source.ToList<T>();
        var items = listItems.Skip(start).ToList<T>();
        return items.ToArray<T>();
    }
}