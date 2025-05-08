using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
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

        public static bool ValidateUrl(string url) {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}