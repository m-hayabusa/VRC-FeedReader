using UnityEngine;
using UnityEditor;

namespace nekomimiStudio.feedReader
{
    public class CreateFeedReader
    {
        [MenuItem("GameObject/nekomimiStudio/FeedReader", false, 10)]
        public static void Create(MenuCommand menu)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/studio.nekomimi.feedreader/Runtime/FeedReader.prefab");
            GameObject res = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
            GameObjectUtility.SetParentAndAlign(res, (GameObject)menu.context);
            Undo.RegisterCreatedObjectUndo(res, "FeedReader");
            Selection.activeObject = res;
        }
    }
}