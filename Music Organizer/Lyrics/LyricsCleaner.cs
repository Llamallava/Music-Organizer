using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Music_Organizer.Lyrics
{
    public static class LyricsCleaner
    {
        public static string CleanGeniusLyrics(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string text = raw;

            text = text.Replace("\uFEFF", string.Empty);

            text = text.Replace('\u00A0', ' ');
            text = text.Replace('\u2009', ' ');
            text = text.Replace('\u202F', ' ');
            text = text.Replace('\u3000', ' ');

            text = Regex.Replace(text, @"[ \t\f\v]+", " ");

            text = Regex.Replace(
                text,
                @"^\s*\d+\s+Contributors.*?\bLyrics\b",
                string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            text = Regex.Replace(
                text,
                @"\[[^\]]+\]",
                "\n\n",
                RegexOptions.Singleline
            );

            // Add a space after punctuation if glued to the next token
            text = Regex.Replace(text, @"(?<=[\.\!\?,;:])(?=[A-Za-z])", " ");

            // Your requested rule:
            // If we find a Capital letter that is glued to the previous char (no whitespace),
            // insert a newline BEFORE it, unless the previous char is '('.
            //
            // Examples:
            // "plotBut" -> "plot\nBut"
            // "(Calm" stays "(Calm"
            // "stormConverging" -> "storm\nConverging"
            text = Regex.Replace(
                text,
                @"(?<!\s)(?<!\()(?=[A-Z])",
                "\n"
            );

            // Optional: if the newline created "word\n,word" style, normalize spaces around newlines.
            text = Regex.Replace(text, @" *\n *", "\n");

            // Reduce excessive blank lines.
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            // Trim each line.
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            text = string.Join("\n", lines).Trim();

            return text;
        }
    }
}
