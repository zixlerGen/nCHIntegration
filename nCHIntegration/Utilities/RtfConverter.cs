using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using nCHIntegration.Migrations;
using RtfPipe;
using System.Text;
using System.Text.RegularExpressions;

namespace nCHIntegration.Utilities
{
    public static class RtfConverter
    {
        public static string ConvertRtfToPlainTextSimple(string rtfText)
        {
            if (string.IsNullOrWhiteSpace(rtfText))
            {
                return string.Empty;
            }

            // Remove the outer RTF braces and header
            string cleaned = rtfText.Trim();
            if (cleaned.StartsWith("{\rtf"))
            {
                // Simple removal of outer RTF wrapper if present
                int firstBrace = cleaned.IndexOf('{');
                int lastBrace = cleaned.LastIndexOf('}');
                if (firstBrace == 0 && lastBrace == cleaned.Length - 1)
                {
                    cleaned = cleaned.Substring(firstBrace + 1, lastBrace - firstBrace - 1);
                }
            }

            // Replace \par with Environment.NewLine
            cleaned = Regex.Replace(cleaned, @"\\par\s*", Environment.NewLine);

            // Remove font table and other group definitions like {\fonttbl...}, {\info...}
            cleaned = Regex.Replace(cleaned, @"\{\\.*?\}", "");

            // Remove most common RTF control words (\rtf, \ansi, \deff0, \pard, \f0, \fs23, \viewkind, \uc, etc.)
            // This regex tries to match words starting with '\' followed by letters and optional digits, ending with a space or group end
            cleaned = Regex.Replace(cleaned, @"\\\w+\d*\s*", "");

            // Remove character escapes like \'xx (e.g., \'92 for apostrophe)
            cleaned = Regex.Replace(cleaned, @"\'[0-9a-fA-F]{2}", "");

            // Remove any remaining braces if they weren't part of a control group
            cleaned = cleaned.Replace("{", "").Replace("}", "");

            //if(cleaned.StartsWith("{tf"))
            //{
            //    // Simple removal of outer RTF wrapper if present
            //    int firstBrace = cleaned.IndexOf('{');
            //    int lastBrace = cleaned.LastIndexOf('}');
            //    if (firstBrace == 0 && lastBrace == cleaned.Length - 1)
            //    {
            //        cleaned = cleaned.Substring(firstBrace + 1, lastBrace - firstBrace - 1);
            //    }
            //}

            int firstLabelAs = cleaned.IndexOf("abeled as ");
            if (firstLabelAs >= 0)
            {
                cleaned = cleaned.Substring(firstLabelAs + 11, cleaned.Length - firstLabelAs - 11);
            }

            int firstPurpose = cleaned.IndexOf("[Purpose]");
            if (firstPurpose >= 0)
            {
                cleaned = cleaned.Substring(firstPurpose, cleaned.Length - firstPurpose);
            }

            int firstlang1054 = cleaned.IndexOf("lang1054");
            if (firstlang1054 >= 0)
            {
                cleaned = cleaned.Substring(0, firstlang1054);
            }

            // Replace \par with Environment.NewLine
            cleaned = cleaned.Replace("pard", "");
            cleaned = cleaned.Replace("\"", "");
            cleaned = cleaned.Replace("par", "");
            //cleaned = Regex.Replace(cleaned, "par", "\n");


            // Clean up multiple newlines and trim each line
            string[] lines = cleaned.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                    .Select(line => line.Trim())
                                    .ToArray();

            // Join lines, removing purely empty ones after trimming, and consolidating multiple newlines
            return string.Join("\n", lines.Where(l => !string.IsNullOrEmpty(l)))
                         .Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine)
                         .Trim(); // Final trim
        }

        public static string FormatMedicalReport(string plainText)
        {
            StringBuilder formattedBuilder = new StringBuilder();
            // Split by Environment.NewLine for consistency with how RTF \par maps
            string[] lines = plainText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            bool inFindings = false;
            bool inImpression = false;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue; // Skip lines that are empty after trim
                }

                if (trimmedLine.Equals("[Purpose]", StringComparison.OrdinalIgnoreCase))
                {
                    formattedBuilder.AppendLine("--- Purpose ---");
                    inFindings = false;
                    inImpression = false;
                }
                else if (trimmedLine.Equals("[Findings]", StringComparison.OrdinalIgnoreCase))
                {
                    // Add an extra line for separation if it's not the very first thing
                    if (formattedBuilder.Length > 0 && !formattedBuilder.ToString().EndsWith(Environment.NewLine + Environment.NewLine))
                    {
                        formattedBuilder.AppendLine();
                    }
                    formattedBuilder.AppendLine("--- Findings ---");
                    inFindings = true;
                    inImpression = false;
                }
                else if (trimmedLine.Equals("[Impression]", StringComparison.OrdinalIgnoreCase))
                {
                    // Add an extra line for separation
                    if (formattedBuilder.Length > 0 && !formattedBuilder.ToString().EndsWith(Environment.NewLine + Environment.NewLine))
                    {
                        formattedBuilder.AppendLine();
                    }
                    formattedBuilder.AppendLine("--- Impression ---");
                    inFindings = false;
                    inImpression = true;
                }
                else if (inFindings || inImpression)
                {
                    // Indent specific lines within Findings/Impression sections
                    if (trimmedLine.StartsWith("MUGA (Tc-99m RBC)") ||
                        trimmedLine.StartsWith("The study reveals") ||
                        trimmedLine.StartsWith("Calculated LVEF") ||
                        trimmedLine.StartsWith("Previous LVEF") ||
                        trimmedLine.StartsWith("LVEF "))
                    {
                        formattedBuilder.AppendLine($"- {trimmedLine}");
                    }
                    else
                    {
                        formattedBuilder.AppendLine(trimmedLine);
                    }
                }
                else
                {
                    // For other general content, just append
                    formattedBuilder.AppendLine(trimmedLine);
                }
            }

            // Final cleanup of excessive newlines after custom formatting
            string finalOutput = Regex.Replace(formattedBuilder.ToString(), @"(\r\n|\r|\n){3,}", Environment.NewLine + Environment.NewLine);

            return finalOutput.Trim(); // Final trim to remove any leading/trailing whitespace
        }

        public static string RtfToPlainText(string rtfText)
        {
            string plaintext = "";

            plaintext = "";

            return plaintext;
        }
        public static string ConvertRtfToPlain(string rtf)
        {
            //var plainText = Rtf.ToHtml(rtf);

            var plainText = rtf.Replace("par", "");
            rtf = rtf.Replace("\\", "");
            String [] rtfList = rtf.Split("\n");
            plainText = "";
            foreach (var item in rtfList)
            {
                bool status = false;
                string raw = item.Replace("par", "");

                if (raw.IndexOf('{') == -1)
                    status = true;
                else status = false;

                if (raw.IndexOf('}') == -1)
                    status = true;
                else status = false;

                if (raw.IndexOf("viewkind4uc") >= 0)
                { 
                    int indexFirstSpace = raw.IndexOf(' ');
                    if (indexFirstSpace < 0)
                        raw = "";
                    else
                        raw = raw.Substring(indexFirstSpace, raw.Length - indexFirstSpace);
                }

                //Regex.Replace(raw, @"lang\d+\s*", "");
                raw = raw.Replace("lang1054f1fs20", "");
                raw = raw.Replace("lang1054f0", "");
                raw = raw.Replace("lang1054", "");
                raw = raw.Replace("lang1033f1", "");
                raw = raw.Replace("lang1033f0", "");
                raw = raw.Replace("lang1033f1", "");
                raw = raw.Replace("lang1054f0", "");
                raw = raw.Replace("ab ", "");
                raw = raw.Replace("\b ", "");
                raw = raw.Replace("\b0", "");
                raw = raw.Replace("cf0fs20", "");
                raw = raw.Replace("dltr", "");
                raw = raw.Replace("cf0f1fs20", "");
                raw = raw.Replace("f1fs20", "");

                if (status)
                    plainText += raw + "\n";
            }

            //plainText = HtmlToPlainText(plainText);
            return (plainText);
        }
        public static string HtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText).Trim();
        }
    }
}
