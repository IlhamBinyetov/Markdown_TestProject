using System.Text.RegularExpressions;

namespace Markdown_TestProject
{
    public static class Markdown

    {
            private static string Wrap(string text, string tag) => $"<{tag}>{text}</{tag}>";

            private static bool IsTag(string text, string tag) => text.StartsWith($"<{tag}>");


            private static string Parse(string markdown, string delimiter, string tag)
            {
                var pattern = $"{delimiter}(.+?){delimiter}";
                var replacement = $"<{tag}>$1</{tag}>";
                return Regex.Replace(markdown, pattern, replacement);
            }
            private static string ParseBold(string markdown) => Parse(markdown, "__", "strong");

            private static string ParseItalic(string markdown) => Parse(markdown, "_", "em");


            private static string ParseText(string markdown, bool inList)
            {
                var parsedText = ParseItalic(ParseBold(markdown));
                return inList == true ? parsedText : Wrap(parsedText, "p");
            }
            private static string ParseHeader(string markdown, bool inList, out bool inListAfter)
            {
                int headerLevel = GetHeaderLevel(markdown);
                if (headerLevel > 0)
                {
                    inListAfter = false;
                    string headerContent = markdown.Substring(headerLevel).Trim();
                    return inList
                        ? $"</ul><h{headerLevel}>{headerContent}</h{headerLevel}>"
                        : $"<h{headerLevel}>{headerContent}</h{headerLevel}>";
                }
                inListAfter = inList;
                return null;
            }

            private static string ParseLineItem(string markdown, bool inList, out bool inListAfter)
            {
                if (markdown.TrimStart().StartsWith("*"))
                {
                    var itemContent = ParseText(markdown.Substring(1).Trim(), true);
                    var result = Wrap(itemContent, "li");
                    inListAfter = true;
                    return inList ? result : $"<ul>{result}";
                }
                
                if (inList)
                {
                    inListAfter = false;
                    return "</ul>" + ParseText(markdown, false);
                }

                inListAfter = false;
                return ParseText(markdown, false);
        }


            private static string ParseParagraph(string markdown, bool list, out bool inListAfter)
            {
                if (list)
                {
                    inListAfter = false;
                    return "</ul>" + ParseText(markdown, false);
                }
                else
                {
                    inListAfter = false;
                    return ParseText(markdown, false);
                }
            }


             private static string ParseLine(string markdown, bool list, out bool inListAfter)
             {
                string result = "";
                result = ParseHeader(markdown, list, out inListAfter);
                if (result != null) return result;

                result = ParseLineItem(markdown, list, out inListAfter);
                if (result != null) return result;

                result = ParseParagraph(markdown, list, out inListAfter);
                if (result != null) return result;

                throw new ArgumentException("Invalid markdown");

             }
            public static string Parse(string markdown)
            {
                var lines = markdown.Split('\n');
                var result = "";
                var inList = false;

                foreach (var line in lines)
                {
                    var lineResult = ParseLine(line, inList, out inList);
                    result += lineResult;
                }

                if (inList) result += "</ul>";
                return result;
            }
            private static int GetHeaderLevel(string markdown)
            {
                int level = 0;
                while (level < markdown.Length && markdown[level] == '#') level++;
                return level;
            }

    }
}