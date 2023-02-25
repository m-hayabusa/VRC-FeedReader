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

    void Update()
    {
        if (!done)
        {
            if (feeds.isReady())
            {
                for (int i = 0; i < feeds.getFeedLength(); i++)
                {
                    Debug.Log(feeds.getFeedHeaderItem(i, feedHeader.Title));

                    for (int j = 0; j < feeds.getFeedEntryLength(i); j++)
                    {
                        Debug.Log(feeds.getFeedEntryItem(i, j, feedEntry.Title));
                        Debug.Log(feeds.getFeedEntryItem(i, j, feedEntry.Summary));
                    }
                }
                done = true;
            }
            else if (feeds.errors().Length > 0)
            {
                var error = feeds.error();
                Debug.Log($"{error.Url.ToString()}: {error.ErrorCode.ToString()}: {error.Error}");
                if (error.Error.StartsWith("Not trusted url hit"))
                    Debug.Log("Check Settings -> Comfort & safety -> Safety -> Allow Untrusted URLs");
            }
            /*
                feeds.errors() is object[].
                cast it to VRC.SDK3.StringLoading.IVRCStringDownload

                feeds.error() is IVRCStringDownload (an latest one)
            */
            /*
                feedHeader and feedEntry are written in ./Runtime/Script/feedReader.cs
            */
        }
    }
}
```
