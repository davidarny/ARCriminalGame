using UnityEngine;
using System.Collections.Generic;

public static class GameObjectExtensions
{
    public static GameObject[] FindObjectWithTag(this GameObject parent, string tag)
    {
        var actors = new List<GameObject>();
        GetChildObject(parent.transform, tag, actors);
        return actors.ToArray();
    }

    public static void GetChildObject(Transform parent, string tag, List<GameObject> actors)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == tag)
            {
                actors.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                GetChildObject(child, tag, actors);
            }
        }
    }
}
