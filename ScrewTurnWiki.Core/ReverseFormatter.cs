using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.IO;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki
{

    /// <summary>
    /// Implements reverse formatting methods (HTML-&gt;WikiMarkup).
    /// </summary>
    public class ReverseFormatter
    {

        private string _wiki;

        private void ProcessList(XmlNodeList nodes, string marker, StringBuilder sb)
        {
            string ul = "*";
            string ol = "#";
            foreach (XmlNode node in nodes)
            {
                string text = "";
                if (node.Name == "li")
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.Name == "br")
                        {
                            text += "\n";
                        }
                        else if (child.Name != "ol" && child.Name != "ul")
                        {
                            TextReader reader = new StringReader(child.OuterXml);
                            XmlDocument n = FromHTML(reader);
                            StringBuilder tempSb = new StringBuilder();
                            ProcessChild(n.ChildNodes, tempSb);
                            text += tempSb.ToString();
                        }
                    }
                    XmlAttribute styleAttribute = node.Attributes["style"];
                    if (styleAttribute != null)
                    {
                        if (styleAttribute.Value.Contains("bold"))
                        {
                            text = "'''" + text + "'''";
                        }
                        if (styleAttribute.Value.Contains("italic"))
                        {
                            text = "''" + text + "''";
                        }
                        if (styleAttribute.Value.Contains("underline"))
                        {
                            text = "__" + text + "__";
                        }
                    }
                    sb.Append(marker + " " + text);
                    if (!sb.ToString().EndsWith("\n")) sb.Append("\n");
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.Name == "ol") ProcessList(child.ChildNodes, marker + ol, sb);
                        if (child.Name == "ul") ProcessList(child.ChildNodes, marker + ul, sb);
                    }
                }
            }
        }

        private string ProcessImage(XmlNode node)
        {
            string result = "";
            if (node.Attributes.Count != 0)
            {
                foreach (XmlAttribute attName in node.Attributes)
                {
                    if (attName.Name == "src")
                    {
                        string[] path = attName.Value.Split('=');
                        if (path.Length > 2) result += "{" + "UP(" + path[1].Split('&')[0].Replace("%20", " ") + ")}" + path[2].Replace("%20", " ");
                        else result += "{UP}" + path[path.Length - 1].Replace("%20", " ");
                    }
                }
            }
            result = HttpUtility.UrlDecode(result);
            // Cut width from end of filename
            int index = result.IndexOf("|");
            if (index > -1)
                result = result.Substring(0, index);

            return result;
        }

        private string ProcessLink(string link)
        {
            link = link.Replace("%20", " ");
            string subLink = "";
            if (link.ToLowerInvariant().StartsWith("getfile")) //"getfile.aspx"
            {
                string[] urlParameters = link.Remove(0, 13).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                string pageName = urlParameters.FirstOrDefault(p => p.ToLowerInvariant().StartsWith("page"));
                if (!string.IsNullOrEmpty(pageName)) pageName = Uri.UnescapeDataString(pageName.Split(new char[] { '=' })[1]);
                string fileName = urlParameters.FirstOrDefault(p => p.ToLowerInvariant().StartsWith("file"));
                fileName = Uri.UnescapeDataString(fileName.Split(new char[] { '=' })[1]);
                if (string.IsNullOrEmpty(pageName))
                {
                    subLink = "{UP}" + fileName;
                }
                else
                {
                    subLink = "{UP(" + pageName + ")}" + fileName;
                }
                link = subLink;
            }
            return link;
        }

        //private void ProcessChildImage(XmlNodeList nodes, StringBuilder sb) {
        //    string image = "";
        //    string p = "";
        //    string url = "";
        //    bool hasDescription = false;
        //    foreach(XmlNode node in nodes) {
        //        if(node.Name.ToLowerInvariant() == "img") image += ProcessImage(node);
        //        else if(node.Name.ToLowerInvariant() == "p") {
        //            hasDescription = true;
        //            StringBuilder tempSb = new StringBuilder();
        //            ProcessChild(node.ChildNodes, tempSb);
        //            p += "|" + tempSb.ToString() + "|";
        //        }
        //        else if(node.Name.ToLowerInvariant() == "a") {
        //            string link = "";
        //            string target = "";
        //            if(node.Attributes.Count != 0) {
        //                XmlAttributeCollection attribute = node.Attributes;
        //                foreach(XmlAttribute attName in attribute) {
        //                    if(attName.Value == "_blank") target += "^";
        //                    if(attName.Name == "href") link += attName.Value;
        //                }
        //            }
        //            link = ProcessLink(link);
        //            image += ProcessImage(node.LastChild);
        //            url = "|" + target + link;
        //        }
        //    }
        //    if(!hasDescription) p = "||";
        //    sb.Append(p + image + url);
        //}

        //private void ProcessChildImageNew(XmlNode mainNode,  StringBuilder sb)
        //{
        //    string image = "";
        //    //string p = "";
        //    string url = "";
        //    //bool hasDescription = false;

        //    string align = "image";
        //    switch (mainNode.Attributes["class"].Value)
        //    {
        //        case "imageleft":
        //            align = "left";
        //            break;
        //        case "imageright":
        //            align = "right";
        //            break;
        //        case "imageauto":
        //            align = "auto";
        //            break;
        //    }

        //    string width = String.Empty;
        //    var thumbinnerNode = mainNode.ChildNodes[0];
        //    if (thumbinnerNode.Attributes != null && thumbinnerNode.Attributes["style"] != null && thumbinnerNode.Attributes["style"].Value.Contains("width:"))
        //    {
        //        string s = thumbinnerNode.Attributes["style"].Value;
        //        var regex = new Regex(@"\d+");
        //        int w;
        //        if (Int32.TryParse(regex.Match(s).Groups[0].Value, out w))
        //        {
        //            switch (w)
        //            {
        //                case 100:
        //                    width = "small";
        //                    break;
        //                case 250:
        //                    width = "medium";
        //                    break;
        //                default:
        //                    width = w.ToString();
        //                    break;
        //            }
        //        }
        //    }

        //    string description = String.Empty;
        //    foreach (XmlNode node in thumbinnerNode.ChildNodes)
        //    {
        //        if (node.Name.ToLowerInvariant() == "img") image += ProcessImage(node);
        //        else if (node.Attributes != null && node.Attributes["class"] != null && node.Attributes["class"].Value == "imagedescription")
        //        {
        //            description = node.InnerText;
        //            ////hasDescription = true;
        //            //StringBuilder tempSb = new StringBuilder();
        //            //ProcessChild(node.ChildNodes, tempSb);
        //            //description = tempSb.ToString();
        //            ////p += "|" + tempSb.ToString() + "|";
        //        }
        //        else if (node.Name.ToLowerInvariant() == "a")
        //        {
        //            string link = "";
        //            string target = "";
        //            if (node.Attributes.Count != 0)
        //            {
        //                XmlAttributeCollection attribute = node.Attributes;
        //                foreach (XmlAttribute attName in attribute)
        //                {
        //                    if (attName.Value == "_blank") target += "^";
        //                    if (attName.Name == "href") link += attName.Value;
        //                }
        //            }
        //            link = ProcessLink(link);
        //            image += ProcessImage(node.LastChild);
        //            url = target + link;
        //        }
        //    }
        //    //if (!hasDescription) p = "||";
        //    //sb.Append(p + image + url);
        //    sb.Append(String.Format("[image:{0}|{1}|{2}|{3}]", url, align, width, description));
        //}

        private void ProcessTableImage(XmlNodeList nodes, StringBuilder sb)
        {
            foreach (XmlNode node in nodes)
            {
                switch (node.Name.ToLowerInvariant())
                {
                    case "tbody":
                        ProcessTableImage(node.ChildNodes, sb);
                        break;
                    case "tr":
                        ProcessTableImage(node.ChildNodes, sb);
                        break;
                    case "td":
                        string image = "";
                        string aref = "";
                        string p = "";
                        bool hasLink = false;
                        if (node.FirstChild.Name.ToLowerInvariant() == "img")
                        {
                            StringBuilder tempSb = new StringBuilder();
                            ProcessTableImage(node.ChildNodes, tempSb);
                            image += tempSb.ToString();
                        }
                        if (node.FirstChild.Name.ToLowerInvariant() == "a")
                        {
                            hasLink = true;
                            StringBuilder tempSb = new StringBuilder();
                            ProcessTableImage(node.ChildNodes, tempSb);
                            aref += tempSb.ToString();
                        }
                        if (node.LastChild.Name.ToLowerInvariant() == "p") p += node.LastChild.InnerText;
                        if (!hasLink) sb.Append(p + image);
                        else sb.Append(p + aref);
                        break;
                    case "img":
                        sb.Append("|" + ProcessImage(node));
                        break;
                    case "a":
                        string link = "";
                        string target = "";
                        string title = "";
                        if (node.Attributes.Count != 0)
                        {
                            XmlAttributeCollection attribute = node.Attributes;
                            foreach (XmlAttribute attName in attribute)
                            {
                                if (attName.Name != "id".ToLowerInvariant())
                                {
                                    if (attName.Value == "_blank") target += "^";
                                    if (attName.Name == "href") link += attName.Value;
                                    if (attName.Name == "title") title += attName.Value;
                                }
                                link = ProcessLink(link);
                            }
                            ProcessTableImage(node.ChildNodes, sb);
                            sb.Append("|" + target + link);
                        }
                        break;
                }
            }
        }

        private void ProcessTable(XmlNodeList nodes, StringBuilder sb)
        {
            foreach (XmlNode node in nodes)
            {
                switch (node.Name.ToLowerInvariant())
                {
                    case "thead":
                        ProcessTable(node.ChildNodes, sb);
                        break;
                    case "th":
                        sb.Append("! ");
                        ProcessChild(node.ChildNodes, sb);
                        sb.Append("\n");
                        break;
                    case "caption":
                        sb.Append("|+ ");
                        ProcessChild(node.ChildNodes, sb);
                        sb.Append("\n");
                        break;
                    case "tbody":
                        ProcessTable(node.ChildNodes, sb);
                        break;
                    case "tr":
                        string style = "";
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            style += " " + attr.Name + "=\"" + attr.Value + "\" ";
                            //if(attr.Name.ToLowerInvariant() == "style") style += "style=\"" + attr.Value + "\" ";
                        }
                        sb.Append("|- " + style + "\n");
                        ProcessTable(node.ChildNodes, sb);
                        break;
                    case "td":
                        string styleTd = "";
                        if (node.Attributes.Count != 0)
                        {
                            foreach (XmlAttribute attr in node.Attributes)
                            {
                                styleTd += " " + attr.Name + "=\"" + attr.Value + "\" ";
                            }
                            sb.Append("| " + styleTd + " | ");
                            ProcessChild(node.ChildNodes, sb);
                            sb.Append("\n");
                        }
                        else
                        {
                            sb.Append("| ");
                            ProcessChild(node.ChildNodes, sb);
                            sb.Append("\n");
                        }
                        break;
                }
            }
        }

        private void ProcessGallery(XmlNode node, StringBuilder sb)
        {
            var meta = node.Attributes != null && node.Attributes["meta"] != null ? node.Attributes["meta"].Value : "";
            var perrow = "";
            var size = "";
            foreach (var s in meta.Trim().Split(';'))
            {
                if (s.StartsWith("perrow"))
                    perrow = s;
                if (s.StartsWith("size"))
                    size = s;
            }
            sb.AppendFormat("<gallery {0}{1}>\n", perrow, size);
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name.ToLowerInvariant() == "img")
                {
                    var altText = childNode.Attributes != null && childNode.Attributes["alt"] != null ? childNode.Attributes["alt"].Value : "";
                    sb.AppendFormat("image:{0}|{1}\n", ProcessImage(childNode), altText);
                }
            }
            sb.AppendLine("</gallery>");
            for (int i = 0; i < node.ChildNodes.Count; i++)
                node.RemoveChild(node.ChildNodes[0]);
        }

        private void ProcessChild(XmlNodeList nodes, StringBuilder sb)
        {
            foreach (XmlNode node in nodes)
            {
                bool anchor = false;
                if (node.NodeType == XmlNodeType.Text) sb.Append(node.Value.TrimStart('\r').TrimStart('\n'));
                else if (node.NodeType != XmlNodeType.Whitespace)
                {
                    if (node.Attributes["pluginid"] != null)
                    {
                        ProcessPlugin(node, sb);
                        continue;
                    }

                    switch (node.Name.ToLowerInvariant())
                    {
                        case "html":
                            ProcessChild(node.ChildNodes, sb);
                            break;
                        case "b":
                        case "strong":
                            if (node.HasChildNodes)
                            {
                                sb.Append("'''");
                                ProcessChild(node.ChildNodes, sb);
                                sb.Append("'''");
                            }
                            break;
                        case "strike":
                        case "s":
                            if (node.HasChildNodes)
                            {
                                sb.Append("--");
                                ProcessChild(node.ChildNodes, sb);
                                sb.Append("--");
                            }
                            break;
                        case "em":
                        case "i":
                            if (node.HasChildNodes)
                            {
                                sb.Append("''");
                                ProcessChild(node.ChildNodes, sb);
                                sb.Append("''");
                            }
                            break;
                        case "u":
                            if (node.HasChildNodes)
                            {
                                sb.Append("__");
                                ProcessChild(node.ChildNodes, sb);
                                sb.Append("__");
                            }
                            break;
                        case "h1":
                            if (node.HasChildNodes)
                            {
                                //if(node.FirstChild.NodeType == XmlNodeType.Whitespace) {
                                if (String.IsNullOrWhiteSpace(node.FirstChild.InnerText))
                                {
                                    sb.Append("----\n");
                                    ProcessChild(node.ChildNodes, sb);
                                }
                                else
                                {
                                    if (!(sb.Length == 0 || sb.ToString().EndsWith("\n"))) sb.Append("\n");
                                    sb.Append("==");
                                    ProcessChild(node.ChildNodes, sb);
                                    sb.Append("==\n");
                                }
                            }
                            else sb.Append("----\n");
                            break;
                        case "h2":
                            if (!(sb.Length == 0 || sb.ToString().EndsWith("\n"))) sb.Append("\n");
                            sb.Append("===");
                            ProcessChild(node.ChildNodes, sb);
                            sb.Append("===\n");
                            break;
                        case "h3":
                            if (!(sb.Length == 0 || sb.ToString().EndsWith("\n"))) sb.Append("\n");
                            sb.Append("====");
                            ProcessChild(node.ChildNodes, sb);
                            sb.Append("====\n");
                            break;
                        case "h4":
                            if (!(sb.Length == 0 || sb.ToString().EndsWith("\n"))) sb.Append("\n");
                            sb.Append("=====");
                            ProcessChild(node.ChildNodes, sb);
                            sb.Append("=====\n");
                            break;
                        case "pre":
                            if (node.HasChildNodes) sb.Append("@@" + node.InnerText + "@@");
                            break;
                        case "code":
                            if (node.HasChildNodes)
                            {
                                sb.Append("{{");
                                ProcessChild(node.ChildNodes, sb);
                                sb.Append("}}");
                            }
                            break;
                        case "hr":
                        case "hr /":
                            sb.Append("\n== ==\n");
                            ProcessChild(node.ChildNodes, sb);
                            break;
                        case "span":
                            if (node.Attributes["style"] != null && node.Attributes["style"].Value.Replace(" ", "").ToLowerInvariant().Contains("font-weight:normal"))
                            {
                                ProcessChild(node.ChildNodes, sb);
                            }
                            else if (node.Attributes["style"] != null && node.Attributes["style"].Value.Replace(" ", "").ToLowerInvariant().Contains("white-space:pre"))
                            {
                                sb.Append(": ");
                            }
                            else
                            {
                                sb.Append(node.OuterXml);
                            }
                            break;
                        case "br":
                            ////sb.Append("{br}");
                            //sb.Append("\n");
                            if (node.PreviousSibling != null && node.PreviousSibling.Name == "br")
                            {
                                sb.Append("\n");
                            }
                            else
                            {
                                ////sb.Append("\n"); //
                                if (Settings.GetProcessSingleLineBreaks(_wiki)) sb.Append("\n");
                                else sb.Append("\n\n");
                            }
                            break;
                        case "table":
                            string tableStyle = "";

                            if (node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("imageauto"))
                            {
                                sb.Append("[imageauto|");
                                ProcessTableImage(node.ChildNodes, sb);
                                sb.Append("]");
                            }
                            else
                            {
                                foreach (XmlAttribute attName in node.Attributes)
                                {
                                    tableStyle += attName.Name + "=\"" + attName.Value + "\" ";
                                }
                                sb.Append("{| " + tableStyle + "\n");
                                ProcessTable(node.ChildNodes, sb);
                                sb.Append("|}\n");
                            }
                            break;
                        case "ol":
                            /*After tags h1,h2,h3,h4,h5,h6 set the symbol /n, then them ignored*/
                            if (node.PreviousSibling != null && node.PreviousSibling.Name != "br" && !node.PreviousSibling.Name.StartsWith("h"))
                            {
                                sb.Append("\n");
                            }
                            if (node.ParentNode != null)
                            {
                                // Experemental
                                //if (node.ParentNode.Name != "td") ProcessList(node.ChildNodes, "#", sb);
                                //else sb.Append(node.OuterXml);
                                ProcessList(node.ChildNodes, "#", sb);
                            }
                            else ProcessList(node.ChildNodes, "#", sb);
                            break;
                        case "ul":
                            /*After tags h1,h2,h3,h4,h5,h6 set the symbol /n, then them ignored*/
                            if (node.PreviousSibling != null && node.PreviousSibling.Name != "br" && !node.PreviousSibling.Name.StartsWith("h"))
                            {
                                sb.Append("\n");
                            }
                            if (node.ParentNode != null)
                            {
                                // Experemental
                                //if (node.ParentNode.Name != "td") ProcessList(node.ChildNodes, "*", sb);
                                //else sb.Append(node.OuterXml);
                                ProcessList(node.ChildNodes, "*", sb);
                            }
                            else ProcessList(node.ChildNodes, "*", sb);
                            break;
                        case "sup":
                            sb.Append("<sup>");
                            ProcessChild(node.ChildNodes, sb);
                            sb.Append("</sup>");
                            break;
                        case "sub":
                            sb.Append("<sub>");
                            ProcessChild(node.ChildNodes, sb);
                            sb.Append("</sub>");
                            break;
                        case "p":
                            if (node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("imagedescription"))
                            {
                                continue;
                            }
                            else
                            {
                                ProcessChild(node.ChildNodes, sb);
                                sb.Append("\n");
                                //if(!Settings.GetProcessSingleLineBreaks(_wiki)) sb.Append("\n");
                            }
                            break;
                        case "div":
                            if (node.Attributes["wikiid"] != null && node.Attributes["wikiid"].Value.Contains("gallery"))
                                ProcessGallery(node, sb);
                            else if (node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("box"))
                            {
                                if (node.HasChildNodes)
                                {
                                    sb.Append("(((");
                                    ProcessChild(node.ChildNodes, sb);
                                    sb.Append(")))");
                                }
                            }
                            //else if (node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("image"))
                            //{
                            //    if (node.ChildNodes.Count > 0 && node.ChildNodes[0].Attributes["class"] != null && node.ChildNodes[0].Attributes["class"].Value.Contains("imagethumbinner"))
                            //        ProcessChildImageNew(node, sb);
                            //    else
                            //    {
                            //        if (node.Attributes["class"].Value.Contains("imageleft"))
                            //            sb.Append("[imageleft");
                            //        else if (node.Attributes["class"].Value.Contains("imageright"))
                            //            sb.Append("[imageright");
                            //        else if (node.Attributes["class"].Value.Contains("image"))
                            //            sb.Append("[image");
                            //        ProcessChildImage(node.ChildNodes, sb);
                            //        sb.Append("]");                                    
                            //    }

                            //}
                            //else if(node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("imageleft")) {
                            //    sb.Append("[imageleft");
                            //    // was: ProcessChildImage(node.ChildNodes, sb);
                            //    // mc
                            //    ProcessChild(node.ChildNodes, sb);
                            //    // mc
                            //    sb.Append("]");
                            //}
                            //else if(node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("imageright")) {
                            //    sb.Append("[imageright");
                            //    // was: ProcessChildImage(node.ChildNodes, sb);
                            //    // mc
                            //    ProcessChild(node.ChildNodes, sb);
                            //    // mc
                            //    sb.Append("]");
                            //}
                            //else if(node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("image")) {
                            //    sb.Append("[image");
                            //    // was: ProcessChildImage(node.ChildNodes, sb);
                            //    // mc
                            //    ProcessChild(node.ChildNodes, sb);
                            //    // mc
                            //    sb.Append("]");
                            //}
                            else if (node.Attributes["class"] != null && node.Attributes["class"].Value.Contains("indent"))
                            {
                                sb.Append(": ");
                                ProcessChild(node.ChildNodes, sb);
                                sb.Append("\n");
                            }
                            else if (node.Attributes.Count > 0)
                            {
                                sb.Append(node.OuterXml);
                            }
                            else
                            {
                                sb.Append("\n");
                                //if(node.PreviousSibling != null && node.PreviousSibling.Name != "div") {
                                //    if(!Settings.GetProcessSingleLineBreaks(_wiki)) sb.Append("\n");
                                //}
                                if (node.FirstChild != null && node.FirstChild.Name == "br")
                                {
                                    node.RemoveChild(node.FirstChild);
                                }
                                if (node.HasChildNodes)
                                {
                                    ProcessChild(node.ChildNodes, sb);
                                    sb.Append("\n"); //
                                    ////if(Settings.GetProcessSingleLineBreaks(_wiki)) sb.Append("\n");
                                    ////else sb.Append("\n\n");
                                }
                            }
                            break;
                        case "img":
                            string description = String.Empty;
                            string bbsize = null;
                            string bbalign = null;
                            string bbframe = null;
                            string urlType = "wiki";
                            string src = String.Empty;
                            string ilink = null;
                            string alt = null;
                            int sizeW = 0;
                            int width = 0;
                            int height = 0;
                            if (node.Attributes.Count != 0)
                            {
                                foreach (XmlAttribute attrib in node.Attributes)
                                {
                                    switch (attrib.Name)
                                    {
                                        case "data-wiki-type-size":
                                            bbsize = attrib.Value;
                                            break;
                                        case "data-wiki-align":
                                            bbalign = attrib.Value;
                                            break;
                                        case "data-wiki-border":
                                            bbframe = attrib.Value;
                                            break;
                                        case "data-wiki-size-width":
                                            int number;
                                            if (Int32.TryParse(attrib.Value, out number))
                                                sizeW = number;
                                            break;
                                        case "data-wiki-url-type":
                                            urlType = attrib.Value;
                                            break;
                                        case "data-wiki-desc":
                                            description = attrib.Value;
                                            break;
                                        case "alt":
                                            alt = attrib.Value;
                                            break;
                                        case "data-wiki-link":
                                            ilink = attrib.Value;
                                            break;
                                        case "src":
                                            src = attrib.Value;
                                            break;
                                        case "style":
                                            var reg = new Regex(@"(\d{1,4})");
                                            foreach (var s in attrib.Value.Split(';'))
                                            {
                                                if (s.Trim().StartsWith("width", true, CultureInfo.InvariantCulture))
                                                {
                                                    var match = reg.Match(s);
                                                    if (!String.IsNullOrEmpty(match.Groups[0].Value))
                                                        width = Convert.ToInt32(match.Groups[0].Value);
                                                }
                                                if (s.Trim().StartsWith("height", true, CultureInfo.InvariantCulture))
                                                {
                                                    var match = reg.Match(s);
                                                    if (!String.IsNullOrEmpty(match.Groups[0].Value))
                                                        height = Convert.ToInt32(match.Groups[0].Value);
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            sb.Append("[image:" + (urlType != "url" ? ProcessImage(node) : HttpUtility.UrlDecode(src)));
                            if (!string.IsNullOrEmpty(ilink)) sb.Append("|link=" + ilink);
                            if (bbsize == "small" || bbsize == "medium") sb.Append("|" + bbsize);
                            if (bbalign != null && bbalign != "auto") sb.Append("|" + bbalign);
                            if (bbframe != null && bbframe != "frame") sb.Append("|" + bbframe);
                            if (bbsize == "customMax" && sizeW > 0 && urlType != "url") sb.Append("|max" + sizeW.ToString());
                            if (bbsize == "customWidth" && sizeW > 0 && urlType != "url") sb.Append("|" + sizeW.ToString());
                            if (width > 0 && height > 0 && urlType == "url") sb.AppendFormat("|{0}x{1}", width, height);
                            if (!string.IsNullOrEmpty(alt)) sb.AppendFormat("|alt={0}", alt);
                            if (!string.IsNullOrEmpty(description)) sb.Append("|" + description);
                            sb.Append("]");
                            break;
                        //string description = "";
                        //bool hasClass = false;
                        //bool isLink = false;
                        //if(node.ParentNode != null && node.ParentNode.Name.ToLowerInvariant() == "a") isLink = true;
                        //if(node.Attributes.Count != 0) {
                        //    foreach(XmlAttribute attName in node.Attributes) {
                        //        if(attName.Name == "alt") description = attName.Value;
                        //        if(attName.Name == "class") hasClass = true;
                        //    }
                        //}
                        //if(!hasClass && !isLink) sb.Append("[image|" + description + "|" + ProcessImage(node) + "]\n");
                        //else if(!hasClass && isLink) sb.Append("[image|" + description + "|" + ProcessImage(node));
                        //else sb.Append(description + "|" + ProcessImage(node));
                        //break;
                        case "a":
                            string link = "";
                            string title = "";
                            if (node.Attributes.Count != 0 && node.Attributes["wikiid"] != null && node.Attributes["wikiid"].Value.ToLowerInvariant() == "file")
                            {
                                XmlAttributeCollection attributes = node.Attributes;
                                foreach (XmlAttribute attName in attributes)
                                {
                                    if (attName.Name != "id".ToLowerInvariant())
                                    {
                                        if (attName.Name.ToLowerInvariant() == "href")
                                            link += attName.Value.Replace("%20", " ");
                                        if (attName.Name.ToLowerInvariant() == "title") title += attName.Value;
                                    }
                                }
                                sb.Append("[file:" + ProcessLink(link));
                                if (!string.IsNullOrEmpty(title)) sb.Append("|" + title);
                                sb.Append("]");
                                break;
                            }
                            bool isTable = false;
                            string target = "";
                            string formattedLink = "";
                            bool isSystemLink = false;
                            bool childImg = false;
                            bool pageLink = false;
                            if (node.FirstChild != null && node.FirstChild.Name == "img") childImg = true;
                            if (node.ParentNode.Name == "td") isTable = true;
                            if (node.Attributes.Count != 0)
                            {
                                XmlAttributeCollection attribute = node.Attributes;
                                foreach (XmlAttribute attName in attribute)
                                {
                                    if (attName.Name != "id".ToLowerInvariant())
                                    {
                                        if (attName.Value.ToLowerInvariant() == "_blank") target += "^";
                                        if (attName.Name.ToLowerInvariant() == "href") link += attName.Value.Replace("%20", " ");
                                        if (attName.Name.ToLowerInvariant() == "title") title += attName.Value;
                                        if (attName.Name.ToLowerInvariant() == "class" && attName.Value.ToLowerInvariant() == "systemlink") isSystemLink = true;
                                        if (attName.Name.ToLowerInvariant() == "class" && (attName.Value.ToLowerInvariant() == "unknownlink" || attName.Value.ToLowerInvariant() == "pagelink")) pageLink = true;
                                    }
                                    else
                                    {
                                        anchor = true;
                                        sb.Append("[anchor|#" + attName.Value + "]");
                                        ProcessChild(node.ChildNodes, sb);
                                        break;
                                    }
                                }
                                if (isSystemLink)
                                {
                                    string[] splittedLink = link.Split('=');
                                    if (splittedLink.Length == 2) formattedLink = "c:" + splittedLink[1];
                                    else formattedLink = link.LastIndexOf('/') > 0 ? link.Substring(link.LastIndexOf('/') + 1) : link;
                                }
                                else if (pageLink)
                                {
                                    formattedLink = link.LastIndexOf('/') > 0 ? link.Substring(link.LastIndexOf('/') + 1) : link;
                                    if (GlobalSettings.PageExtension.Length > 0)
                                    {
                                        var index = formattedLink.IndexOf(GlobalSettings.PageExtension);
                                        if (index >= 0) formattedLink = formattedLink.Remove(formattedLink.IndexOf(GlobalSettings.PageExtension));
                                    }
                                    formattedLink = Uri.UnescapeDataString(formattedLink);
                                }
                                else
                                {
                                    formattedLink = ProcessLink(link);
                                }
                                if (!anchor && !isTable && !childImg)
                                {
                                    if (HttpUtility.HtmlDecode(title) != HttpUtility.HtmlDecode(link))
                                    {
                                        sb.Append("[" + target + formattedLink + "|");
                                        ProcessChild(node.ChildNodes, sb);
                                        sb.Append("]");
                                    }
                                    else
                                    {
                                        sb.Append("[" + target + formattedLink + "]");
                                    }
                                }
                                if (!anchor && !childImg && isTable)
                                {
                                    sb.Append("[" + target + formattedLink + "|");
                                    ProcessChild(node.ChildNodes, sb);
                                    sb.Append("]");
                                }
                                if (!anchor && childImg && !isTable)
                                {
                                    ProcessChild(node.ChildNodes, sb);
                                    sb.Append("|" + target + formattedLink + "]");
                                }
                            }
                            break;
                        default:
                            sb.Append(node.OuterXml);
                            break;
                    }
                }
            }
        }

        private void ProcessPlugin(XmlNode node, StringBuilder sb)
        {
            IList<IFormatterProviderV50> providers = FormattingPipeline.GetSortedFormatters(_wiki);
            foreach (IFormatterProviderV50 provider in providers)
                if (provider.EnablePluginsEditor)
                {
                    provider.WysiwygReverseFormat(node, sb);
                    break;
                }

        }

        private XmlDocument FromHTML(TextReader reader)
        {
            // setup SgmlReader
            Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader();
            sgmlReader.DocType = "HTML";
            sgmlReader.WhitespaceHandling = WhitespaceHandling.None;

            sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;
            sgmlReader.InputStream = reader;

            // create document
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.XmlResolver = null;
            doc.Load(sgmlReader);
            return doc;
        }

        /// <summary>
        /// Reverse formats HTML content into WikiMarkup.
        /// </summary>
        /// <param name="wiki">The wiki.</param>
        /// <param name="html">The input HTML.</param>
        /// <returns>The corresponding WikiMarkup.</returns>
        public string ReverseFormat(string wiki, string html)
        {
            /*Fix Ckeditors's bug*/
            html = html.Replace("&nbsp;", "<br />");

            _wiki = wiki;
            StringReader strReader = new StringReader(html.Trim('\n', '\r'));
            XmlDocument x = FromHTML((TextReader)strReader);
            if (x != null && x.HasChildNodes && x.FirstChild.HasChildNodes)
            {
                StringBuilder sb = new StringBuilder();
                ProcessChild(x.FirstChild.ChildNodes, sb);
                return sb.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}