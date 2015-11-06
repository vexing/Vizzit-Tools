using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    /// <summary>
    /// Class for extracting html
    /// </summary>
    public class Extractor
    {
        private HtmlDocument page;
         /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Current Webpage we are parsing</param>
        public Extractor(HtmlDocument page)
        {
            this.page = page;
        }

        /// <summary>
        /// Extract all anchor tags using HtmlAgilityPack
        /// </summary>
        /// <returns>Returns all href links in the page as a string List</returns>
        public List<string> ExtractAllAHrefTags()
        {
            List<string> hrefTags = new List<string>();

            //Iterates through all a href tags adding them to the list
            foreach (HtmlNode link in page.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];
                hrefTags.Add(att.Value);
            }

            return hrefTags;
        }

        /// <summary>
        /// Extract the title using HtmlAgilityPack
        /// </summary>
        /// <returns>Returns the title of the pageas a string</returns>
        public string GetTitle()
        {
            HtmlNode title = page.DocumentNode.SelectSingleNode("//title");

            //Returns the title if found otherwise returns Title not found
            if (title != null)
                return title.InnerText;
            else
                return "Title not found";
        }
        /// <summary>
        /// Extracts meta tags using HtmlAgilityPack
        /// </summary>
        /// <returns>Returns the title of the pageas a Dictionary</returns>
        public Dictionary<string, string> GetMeta()
        {
            HtmlNode desc = page.DocumentNode.SelectSingleNode("//meta[@name='description']");
            HtmlNode keywords = page.DocumentNode.SelectSingleNode("//meta[@name='keywords']");

            Dictionary<string, string> meta = new Dictionary<string, string>();

            //Sets Meta description if found
            if (desc != null)
                meta.Add("Description", desc.GetAttributeValue("content", ""));
            else
                meta.Add("Description", "Null");

            //Sets Meta keywords if found
            if (keywords != null)
                meta.Add("Keywords", keywords.GetAttributeValue("content", ""));
            else
                meta.Add("Keywords", "Null");

            return meta;
        }
    }
}
