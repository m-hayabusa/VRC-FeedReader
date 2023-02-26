using UnityEngine;
using UdonXMLParser;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using UdonSharp;

namespace nekomimiStudio.feedReader
{
    public class feedReader : UdonXML_Callback
    {
        private UdonXML udonXml;
        public VRCUrl[] FeedURL;

        private object[] errorlog = new object[0];

        private string[][][][] res;
        /*
            Result res = Feed[];
            Feed = {
                Header,
                Entry[]
            }

            Header = {
                Title[1],
                SubTitle[1],
                Summary[1],
                Id[1],
                Link[1],
                Updated[1],
                Rights[1],
                AuthorName[1],
                AuthorUri[1]
            }

            Entry = {
                Title,
                SubTitle,
                Summary,
                Id,
                Link,
                Updated,
            }
        */

        private int loadingIttr = 0;
        private bool done = true;

        public void Start()
        {
            res = new string[FeedURL.Length][][][];
            udonXml = this.GetComponentInChildren<UdonXML>();
        }

        public void Update()
        {
            if (done && loadingIttr < FeedURL.Length)
            {
                done = false;
                VRCStringDownloader.LoadUrl(FeedURL[loadingIttr], (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
            }
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Debug.Log(result.Result);
            udonXml.LoadXmlCallback(result.Result, this, this.GetInstanceID() + "_" + loadingIttr);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            var err = new object[errorlog.Length + 1];
            System.Array.Copy(errorlog, err, errorlog.Length);
            err[err.Length - 1] = result;
            errorlog = err;
        }

        public override void OnUdonXMLParseEnd(object[] data, string callbackId)
        {
            Debug.Log(callbackId);
            var id = callbackId.Split('_');
            var ittr = int.Parse(id[id.Length - 1]);

            var content = udonXml.GetChildNode(data, 1);
            switch (udonXml.GetNodeName(content))
            {
                case "rdf:RDF":
                    parseRSS1(ittr, content);
                    break;
                case "rss":
                    parseRSS2(ittr, content);
                    break;
                case "feed":
                    parseAtom(ittr, content);
                    break;
            }
        }

        private string GetNodeValueByName(object data, string nodeName)
        {
            if (!udonXml.HasChildNodes(data)) return "";
            var node = udonXml.GetChildNodeByName(data, nodeName);
            if (node == null) return "";
            return udonXml.GetNodeValue(node);
        }
        private void parseRSS1(int feedNum, object contentRoot)
        {
            var headerRoot = udonXml.GetChildNodeByName(contentRoot, "channel");

            res[feedNum] = new string[2][][];
            res[feedNum][0] = new string[10][];

            res[feedNum][0][(int)feedHeader.Title] = new string[] { GetNodeValueByName(headerRoot, "title") };
            res[feedNum][0][(int)feedHeader.SubTitle] = new string[] { GetNodeValueByName(headerRoot, "subtitle") };
            res[feedNum][0][(int)feedHeader.Summary] = new string[] { GetNodeValueByName(headerRoot, "description") };
            res[feedNum][0][(int)feedHeader.Updated] = new string[] { GetNodeValueByName(headerRoot, "dc:date") };
            res[feedNum][0][(int)feedHeader.Link] = new string[] { GetNodeValueByName(headerRoot, "link") };
            res[feedNum][0][(int)feedHeader.AuthorName] = new string[] { "" };
            res[feedNum][0][(int)feedHeader.AuthorUri] = new string[] { "" };

            string[][] entries = new string[udonXml.GetChildNodesCount(contentRoot)][];

            int cnt = 0;
            for (int i = 0; i < udonXml.GetChildNodesCount(contentRoot); i++)
            {
                var entry = udonXml.GetChildNode(contentRoot, i);
                if (udonXml.GetNodeName(entry) == "item")
                {
                    entries[cnt] = new string[6];
                    entries[cnt][(int)feedEntry.Title] = GetNodeValueByName(entry, "title");
                    entries[cnt][(int)feedEntry.SubTitle] = GetNodeValueByName(entry, "subtitle");
                    entries[cnt][(int)feedEntry.Link] = GetNodeValueByName(entry, "link");
                    entries[cnt][(int)feedEntry.Summary] = GetNodeValueByName(entry, "description");
                    entries[cnt][(int)feedEntry.Updated] = GetNodeValueByName(entry, "dc:date");
                    cnt++;
                }
            }
            res[feedNum][1] = new string[cnt][];

            System.Array.Copy(entries, res[feedNum][1], cnt);

            done = true;
            loadingIttr++;
        }

        private void parseRSS2(int feedNum, object contentRoot)
        {
            contentRoot = udonXml.GetChildNodeByName(contentRoot, "channel");

            res[feedNum] = new string[2][][];
            res[feedNum][0] = new string[10][];

            res[feedNum][0][(int)feedHeader.Title] = new string[] { GetNodeValueByName(contentRoot, "title") };
            res[feedNum][0][(int)feedHeader.SubTitle] = new string[] { GetNodeValueByName(contentRoot, "subtitle") };
            res[feedNum][0][(int)feedHeader.Summary] = new string[] { GetNodeValueByName(contentRoot, "description") };
            res[feedNum][0][(int)feedHeader.Updated] = new string[] { GetNodeValueByName(contentRoot, "lastBuildDate") };
            res[feedNum][0][(int)feedHeader.Link] = new string[] { GetNodeValueByName(contentRoot, "link") };

            res[feedNum][0][(int)feedHeader.AuthorName] = new string[] { "" };
            res[feedNum][0][(int)feedHeader.AuthorUri] = new string[] { "" };

            string[][] entries = new string[udonXml.GetChildNodesCount(contentRoot)][];

            int cnt = 0;
            for (int i = 0; i < udonXml.GetChildNodesCount(contentRoot); i++)
            {
                var entry = udonXml.GetChildNode(contentRoot, i);
                if (udonXml.GetNodeName(entry) == "item")
                {
                    entries[cnt] = new string[6];
                    entries[cnt][(int)feedEntry.Title] = GetNodeValueByName(entry, "title");
                    entries[cnt][(int)feedEntry.SubTitle] = GetNodeValueByName(entry, "subtitle");
                    entries[cnt][(int)feedEntry.Link] = GetNodeValueByName(entry, "link");
                    entries[cnt][(int)feedEntry.Summary] = GetNodeValueByName(entry, "description");
                    entries[cnt][(int)feedEntry.Updated] = GetNodeValueByName(entry, "pubDate");
                    cnt++;
                }
            }
            res[feedNum][1] = new string[cnt][];

            System.Array.Copy(entries, res[feedNum][1], cnt);

            done = true;
            loadingIttr++;
        }

        private void parseAtom(int feedNum, object contentRoot)
        {
            res[feedNum] = new string[2][][];
            res[feedNum][0] = new string[10][];

            res[feedNum][0][(int)feedHeader.Title] = new string[] { GetNodeValueByName(contentRoot, "title") };
            res[feedNum][0][(int)feedHeader.SubTitle] = new string[] { GetNodeValueByName(contentRoot, "subtitle") };
            res[feedNum][0][(int)feedHeader.Summary] = new string[] { GetNodeValueByName(contentRoot, "summary") };
            res[feedNum][0][(int)feedHeader.Updated] = new string[] { GetNodeValueByName(contentRoot, "updated") };
            res[feedNum][0][(int)feedHeader.Link] = new string[] { GetNodeValueByName(contentRoot, "link") };

            var author = udonXml.GetChildNodeByName(contentRoot, "author");
            res[feedNum][0][(int)feedHeader.AuthorName] = new string[] { GetNodeValueByName(author, "name") };
            res[feedNum][0][(int)feedHeader.AuthorUri] = new string[] { GetNodeValueByName(author, "name") };

            string[][] entries = new string[udonXml.GetChildNodesCount(contentRoot)][];

            int cnt = 0;
            for (int i = 0; i < udonXml.GetChildNodesCount(contentRoot); i++)
            {
                var entry = udonXml.GetChildNode(contentRoot, i);
                if (udonXml.GetNodeName(entry) == "entry")
                {
                    entries[cnt] = new string[6];
                    entries[cnt][(int)feedEntry.Title] = GetNodeValueByName(entry, "title");
                    entries[cnt][(int)feedEntry.SubTitle] = GetNodeValueByName(entry, "subtitle");
                    entries[cnt][(int)feedEntry.Link] = GetNodeValueByName(entry, "link");
                    entries[cnt][(int)feedEntry.Summary] = GetNodeValueByName(entry, "summary");
                    entries[cnt][(int)feedEntry.Updated] = GetNodeValueByName(entry, "updated");
                    cnt++;
                }
            }
            res[feedNum][1] = new string[cnt][];

            System.Array.Copy(entries, res[feedNum][1], cnt);

            done = true;
            loadingIttr++;
        }

        public bool isReady()
        {
            return done && loadingIttr >= FeedURL.Length;
        }

        // IVRCStringDownload[] -> TypeResolverException: Type referenced by 'VRCSDK3StringLoadingIVRCStringDownloadArray' could not be resolved. 
        public object[] errors()
        {
            return errorlog;
        }
        public IVRCStringDownload error()
        {
            if (errorlog.Length == 0) return null;
            return (IVRCStringDownload)errorlog[errorlog.Length - 1];
        }
        public int getFeedLength()
        {
            if (res == null) return 0;
            return res.Length;
        }

        public int getTotalEntryCount()
        {
            int length = 0;
            for (int i = 0; i < getFeedLength(); i++)
            {
                length += getFeedEntryLength(i);
            }
            return length;
        }

        public int getFeedEntryLength(int feedNum)
        {
            if (res == null || res.Length <= feedNum || res[feedNum] == null) return 0;
            return res[feedNum][1].Length;
        }
        public string getFeedEntryItem(int feedNum, int item, feedEntry entry)
        {
            return res[feedNum][1][item][(int)entry];
        }

        public string getFeedHeaderItem(int feedNum, feedHeader entry)
        {
            return res[feedNum][0][(int)entry][0];
        }
    }

    public enum feedHeader
    {
        Title, SubTitle, Summary, Id, Link, Updated, Rights, Entries, AuthorName, AuthorUri
    }
    public enum feedEntry
    {
        Title, SubTitle, Summary, Id, Link, Updated
    }
}