# nS feedReader

RSS / Atom feed を読み込みます。

- 1. UnityPackage を https://vpm.nekomimi.studio/ からダウンロードしてインポート
  2. [GameObject] -> [nekomimiStudio] -> [FeedReader] を追加
  3. 追加した FeedReader の[Feed URL]を設定
  4. 下のサンプルコードが全機能です ↓
- 1. Download UnityPackage from https://vpm.nekomimi.studio/ and import
  2. Add [GameObject] -> [nekomimiStudio] -> [FeedReader]
  3. Set [Feed URL] in FeedReader that added to the scene
  4. Uses like this↓

```csharp
using UnityEngine;
using UdonSharp;
using nekomimiStudio.feedReader;

public class example : UdonSharpBehaviour
{
    [SerializeField] private feedReader feeds;
    private bool done = false;

    public override void Interact()
    {
        done = false;
        feeds.Load();
    }

    void Update()
    {
        if (!done && feeds != null)
        {
            if (feeds.isReady())
            {
                Debug.Log($"entries: {feeds.getTotalEntryCount()}");
                for (int i = 0; i < feeds.getFeedLength(); i++)
                {
                    if (feeds.errors()[i] != null)
                    {
                        var error = (VRC.SDK3.StringLoading.IVRCStringDownload)feeds.errors()[i];
                        Debug.Log($"{error.Url.ToString()}: {error.ErrorCode.ToString()}: {error.Error}");
                        if (error.Error.StartsWith("Not trusted url hit"))
                            Debug.Log("Check Settings -> Comfort & safety -> Safety -> Allow Untrusted URLs");
                    }
                    else
                    {
                        Debug.Log(feeds.getFeedHeaderItem(i, feedHeader.Title));

                        for (int j = 0; j < feeds.getFeedEntryLength(i); j++)
                        {
                            Debug.Log(feeds.getFeedEntryItem(i, j, feedEntry.Title));
                            Debug.Log(feeds.getFeedEntryItem(i, j, feedEntry.Summary));
                        }
                    }
                }
                done = true;
            }
            else if (feeds.isLoading())
            {
                Debug.Log($"loading... {feeds.GetProgress()}");
            }
            /*
                feedHeader and feedEntry are written in ./Runtime/Script/feedReader.cs (scroll to end of the file)
            */
        }
    }
}
```
