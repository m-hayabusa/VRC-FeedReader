using UnityEngine;
using UdonXMLParser;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;

namespace nekomimiStudio.FeedReader
{
    public class FeedReaderCore : UdonXML_Callback
    {
        private UdonXML udonXml;
        [SerializeField] bool loadOnStart = false;
        public VRCUrl[] FeedURL;

        private object[] errorlog;

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

        private string[] str;
        private bool strDone = true;
        private int strLoadIttr = 0;
        private int parseIttr = 0;

        private bool active = false;

        private float parseProgressUdonXML = 0;

        public void Start()
        {
            udonXml = this.GetComponentInChildren<UdonXML>();
            if (loadOnStart) Load();
        }

        public void Load()
        {
            res = new string[FeedURL.Length][][][];
            str = new string[FeedURL.Length];
            errorlog = new object[FeedURL.Length];

            strDone = true;
            strLoadIttr = 0;
            parseIttr = 0;

            parseProgressUdonXML = 0;

            active = true;
        }

        public void Update()
        {
            if (active)
            {
                if (strDone && strLoadIttr < FeedURL.Length)
                {
                    strDone = false;
                    VRCStringDownloader.LoadUrl(FeedURL[strLoadIttr], (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
                }
                if (parseIttr < FeedURL.Length && str[parseIttr] != null && str[parseIttr] != "")
                {
                    udonXml.LoadXmlCallback(str[parseIttr], this, this.GetInstanceID() + "_" + parseIttr);
                    str[parseIttr] = "";
                }
                if (isReady()) active = false;
            }
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            str[strLoadIttr] = result.Result;
            strLoadIttr++;
            strDone = true;
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            errorlog[strLoadIttr] = result;
            strLoadIttr++;
            strDone = true;
            parseProgressUdonXML = 0;
            parseIttr++;
            Debug.Log("FeedReader: ERR: " + strLoadIttr);
        }

        public override void OnUdonXMLParseEnd(object[] data, string callbackId)
        {
            parseProgressUdonXML = 0;
            parseIttr++;
            var id = callbackId.Split('_');
            var ittr = int.Parse(id[id.Length - 1]);

            bool found = false;
            for (int i = 0; i < udonXml.GetChildNodesCount(data) && !found; i++)
            {
                var content = udonXml.GetChildNode(data, i);
                switch (udonXml.GetNodeName(content))
                {
                    case "rdf:RDF":
                        parseRSS1(ittr, content);
                        found = true;
                        break;
                    case "rss":
                        parseRSS2(ittr, content);
                        found = true;
                        break;
                    case "feed":
                        parseAtom(ittr, content);
                        found = true;
                        break;
                }
            }
            if (!found) Debug.LogWarning("RSS / Atom start tag not found");
        }

        public override void OnUdonXMLIteration(int processing, int total)
        {
            parseProgressUdonXML = processing / (float)total;
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

            res[feedNum][0][(int)FeedHeader.Title] = new string[] { GetNodeValueByName(headerRoot, "title") };
            res[feedNum][0][(int)FeedHeader.SubTitle] = new string[] { GetNodeValueByName(headerRoot, "subtitle") };
            res[feedNum][0][(int)FeedHeader.Summary] = new string[] { GetNodeValueByName(headerRoot, "description") };
            res[feedNum][0][(int)FeedHeader.Id] = new string[] { "" };
            res[feedNum][0][(int)FeedHeader.Updated] = new string[] { GetNodeValueByName(headerRoot, "dc:date") };
            res[feedNum][0][(int)FeedHeader.Rights] = new string[] { GetNodeValueByName(headerRoot, "dc:rights") };
            res[feedNum][0][(int)FeedHeader.Link] = new string[] { GetNodeValueByName(headerRoot, "link") };
            res[feedNum][0][(int)FeedHeader.AuthorName] = new string[] { "" };
            res[feedNum][0][(int)FeedHeader.AuthorUri] = new string[] { "" };

            string[][] entries = new string[udonXml.GetChildNodesCount(contentRoot)][];

            int cnt = 0;
            for (int i = 0; i < udonXml.GetChildNodesCount(contentRoot); i++)
            {
                var entry = udonXml.GetChildNode(contentRoot, i);
                if (udonXml.GetNodeName(entry) == "item")
                {
                    entries[cnt] = new string[6];
                    entries[cnt][(int)FeedEntry.Title] = GetNodeValueByName(entry, "title");
                    entries[cnt][(int)FeedEntry.SubTitle] = GetNodeValueByName(entry, "subtitle");
                    entries[cnt][(int)FeedEntry.Link] = GetNodeValueByName(entry, "link");
                    entries[cnt][(int)FeedEntry.Summary] = GetNodeValueByName(entry, "description");
                    entries[cnt][(int)FeedEntry.Id] = "";
                    entries[cnt][(int)FeedEntry.Updated] = GetNodeValueByName(entry, "dc:date");
                    cnt++;
                }
            }
            res[feedNum][1] = new string[cnt][];

            System.Array.Copy(entries, res[feedNum][1], cnt);
        }

        private void parseRSS2(int feedNum, object contentRoot)
        {
            contentRoot = udonXml.GetChildNodeByName(contentRoot, "channel");

            res[feedNum] = new string[2][][];
            res[feedNum][0] = new string[10][];

            res[feedNum][0][(int)FeedHeader.Title] = new string[] { GetNodeValueByName(contentRoot, "title") };
            res[feedNum][0][(int)FeedHeader.SubTitle] = new string[] { GetNodeValueByName(contentRoot, "subtitle") };
            res[feedNum][0][(int)FeedHeader.Summary] = new string[] { GetNodeValueByName(contentRoot, "description") };
            res[feedNum][0][(int)FeedHeader.Id] = new string[] { "" };
            res[feedNum][0][(int)FeedHeader.Updated] = new string[] { GetNodeValueByName(contentRoot, "lastBuildDate") };
            res[feedNum][0][(int)FeedHeader.Rights] = new string[] { GetNodeValueByName(contentRoot, "copyright") };
            res[feedNum][0][(int)FeedHeader.Link] = new string[] { GetNodeValueByName(contentRoot, "link") };

            res[feedNum][0][(int)FeedHeader.AuthorName] = new string[] { "" };
            res[feedNum][0][(int)FeedHeader.AuthorUri] = new string[] { "" };

            string[][] entries = new string[udonXml.GetChildNodesCount(contentRoot)][];

            int cnt = 0;
            for (int i = 0; i < udonXml.GetChildNodesCount(contentRoot); i++)
            {
                var entry = udonXml.GetChildNode(contentRoot, i);
                if (udonXml.GetNodeName(entry) == "item")
                {
                    entries[cnt] = new string[6];
                    entries[cnt][(int)FeedEntry.Title] = GetNodeValueByName(entry, "title");
                    entries[cnt][(int)FeedEntry.SubTitle] = GetNodeValueByName(entry, "subtitle");
                    entries[cnt][(int)FeedEntry.Link] = GetNodeValueByName(entry, "link");
                    entries[cnt][(int)FeedEntry.Summary] = GetNodeValueByName(entry, "description");
                    entries[cnt][(int)FeedEntry.Id] = GetNodeValueByName(entry, "guid");
                    entries[cnt][(int)FeedEntry.Updated] = GetNodeValueByName(entry, "pubDate");
                    cnt++;
                }
            }
            res[feedNum][1] = new string[cnt][];

            System.Array.Copy(entries, res[feedNum][1], cnt);
        }

        private void parseAtom(int feedNum, object contentRoot)
        {
            res[feedNum] = new string[2][][];
            res[feedNum][0] = new string[10][];

            res[feedNum][0][(int)FeedHeader.Title] = new string[] { GetNodeValueByName(contentRoot, "title") };
            res[feedNum][0][(int)FeedHeader.SubTitle] = new string[] { GetNodeValueByName(contentRoot, "subtitle") };
            res[feedNum][0][(int)FeedHeader.Summary] = new string[] { GetNodeValueByName(contentRoot, "summary") };
            res[feedNum][0][(int)FeedHeader.Id] = new string[] { GetNodeValueByName(contentRoot, "id") };
            res[feedNum][0][(int)FeedHeader.Updated] = new string[] { GetNodeValueByName(contentRoot, "updated") };
            res[feedNum][0][(int)FeedHeader.Rights] = new string[] { GetNodeValueByName(contentRoot, "rights") };
            res[feedNum][0][(int)FeedHeader.Link] = new string[] { GetNodeValueByName(contentRoot, "link") };

            var author = udonXml.GetChildNodeByName(contentRoot, "author");
            res[feedNum][0][(int)FeedHeader.AuthorName] = new string[] { GetNodeValueByName(author, "name") };
            res[feedNum][0][(int)FeedHeader.AuthorUri] = new string[] { GetNodeValueByName(author, "uri") };

            string[][] entries = new string[udonXml.GetChildNodesCount(contentRoot)][];

            int cnt = 0;
            for (int i = 0; i < udonXml.GetChildNodesCount(contentRoot); i++)
            {
                var entry = udonXml.GetChildNode(contentRoot, i);
                if (udonXml.GetNodeName(entry) == "entry")
                {
                    entries[cnt] = new string[6];
                    entries[cnt][(int)FeedEntry.Title] = GetNodeValueByName(entry, "title");
                    entries[cnt][(int)FeedEntry.SubTitle] = GetNodeValueByName(entry, "subtitle");
                    entries[cnt][(int)FeedEntry.Link] = GetNodeValueByName(entry, "link");
                    entries[cnt][(int)FeedEntry.Summary] = GetNodeValueByName(entry, "summary");
                    entries[cnt][(int)FeedEntry.Id] = GetNodeValueByName(entry, "id");
                    entries[cnt][(int)FeedEntry.Updated] = GetNodeValueByName(entry, "updated");
                    cnt++;
                }
            }
            res[feedNum][1] = new string[cnt][];

            System.Array.Copy(entries, res[feedNum][1], cnt);
        }

        public bool isLoading()
        {
            return active;
        }

        public float GetProgress()
        {
            return (parseProgressUdonXML + parseIttr) / FeedURL.Length;
        }

        public bool isReady()
        {
            return parseIttr >= FeedURL.Length;
        }

        // IVRCStringDownload[] -> TypeResolverException: Type referenced by 'VRCSDK3StringLoadingIVRCStringDownloadArray' could not be resolved. 
        public object[] errors()
        {
            return errorlog;
        }
        public IVRCStringDownload error(int feedNum = -1)
        {
            if (feedNum == -1)
            {
                for (int i = errorlog.Length - 1; i >= 0; i--)
                {
                    if (errorlog[i] != null)
                        return (IVRCStringDownload)errorlog[i];
                }
                return null;
            }

            if (feedNum < errorlog.Length && errorlog[feedNum] != null)
                return (IVRCStringDownload)errorlog[feedNum];
            return null;
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
            if (res == null || res.Length < feedNum || res[feedNum] == null) return 0;
            return res[feedNum][1].Length;
        }
        public string getFeedEntryItem(int feedNum, int item, FeedEntry entry)
        {
            if (res.Length < feedNum || res[feedNum] == null || res[feedNum][1] == null || res[feedNum][1].Length < item || res[feedNum][1].Length < item) return null;
            return res[feedNum][1][item][(int)entry];
        }

        public string getFeedHeaderItem(int feedNum, FeedHeader entry)
        {
            if (res.Length < feedNum || res[feedNum] == null || res[feedNum][0] == null) return null;
            return res[feedNum][0][(int)entry][0];
        }
    }

    public enum FeedHeader
    {
        Title, SubTitle, Summary, Id, Link, Updated, Rights, Entries, AuthorName, AuthorUri
    }
    public enum FeedEntry
    {
        Title, SubTitle, Summary, Id, Link, Updated
    }
}