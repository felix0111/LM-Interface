using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Html;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.IO;
using System.Xml.XPath;
using ReverseMarkdown;

namespace LMInterface
{
    public static class HttpHelper {

        public static async Task<string> GetWebsiteContent(string url, string? xPath) {
            HtmlWeb w = new () { Timeout = 30000 };

            //fetch website
            HtmlDocument doc;
            try {
                doc = await w.LoadFromWebAsync(url, Encoding.UTF8);
            } catch {
                return "Error: could not fetch content from requested URL!";
            }
            
            //remove useless nodes
            foreach (var node in doc.DocumentNode.SelectNodes("//script|//style|//nav|//header|//footer|//aside|//form|//*[contains(@class,'reflist')]|//*[contains(@class,'navbox')]|//*[contains(@class,'navbox-styles')]|//*[contains(@class,'catlinks')]") ?? Enumerable.Empty<HtmlNode>()) {
                node.Remove();
            }

            HtmlNodeCollection main;
            try {
                main = doc.DocumentNode.SelectNodes(xPath) ?? throw new Exception();
            } catch {
                // fallback
                main = doc.DocumentNode.SelectSingleNode("//body")!.ChildNodes;
            }

            Converter c = new (new Config() {CleanupUnnecessarySpaces = true , UnknownTags = Config.UnknownTagsOption.Bypass});

            var newDoc = new HtmlDocument();
            newDoc.DocumentNode.AppendChild(HtmlNode.CreateNode("<html><head></head><body></body></html>"));
            newDoc.DocumentNode.SelectSingleNode("//body")!.AppendChildren(main);

            string text = c.Convert(newDoc.DocumentNode.InnerHtml);
            return text;
        }

        public class JsonUrl {
            [JsonProperty("url")] public required string Url { get; set; }
            [JsonProperty("nodes")] public string? Nodes { get; set; } //the xpath expression used for search
        }
    }
}

public class HtmlUtilities
{
    /// <summary>
    /// Converts HTML to plain text / strips tags.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns></returns>
    public static string ConvertToPlainText(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        StringWriter sw = new StringWriter();
        ConvertTo(doc.DocumentNode, sw);
        sw.Flush();
        return sw.ToString();
    }

    private static void ConvertContentTo(HtmlNode node, TextWriter outText)
    {
        foreach (HtmlNode subnode in node.ChildNodes)
        {
            ConvertTo(subnode, outText);
        }
    }


    private static void ConvertTo(HtmlNode node, TextWriter outText)
    {
        string html;
        switch (node.NodeType)
        {
            case HtmlNodeType.Comment:
                // don't output comments
                break;

            case HtmlNodeType.Document:
                ConvertContentTo(node, outText);
                break;

            case HtmlNodeType.Text:
                // script and style must not be output
                string parentName = node.ParentNode.Name;
                if ((parentName == "script") || (parentName == "style"))
                    break;

                // get text
                html = ((HtmlTextNode)node).Text;

                // is it in fact a special closing node output as text?
                if (HtmlNode.IsOverlappedClosingElement(html))
                    break;

                // check the text is meaningful and not a bunch of whitespaces
                if (html.Trim().Length > 0)
                {
                    outText.Write(HtmlEntity.DeEntitize(html));
                }
                break;

            case HtmlNodeType.Element:
                switch (node.Name)
                {
                    case "p":
                        // treat paragraphs as crlf
                        outText.Write("\r\n");
                        break;
                    case "br":
                        outText.Write("\r\n");
                        break;
                }

                if (node.HasChildNodes)
                {
                    ConvertContentTo(node, outText);
                }
                break;
        }
    }
}