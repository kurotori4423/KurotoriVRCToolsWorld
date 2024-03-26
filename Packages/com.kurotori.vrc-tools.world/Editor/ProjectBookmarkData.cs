using System.Collections.Generic;
using UnityEngine;

namespace KurotoriTools
{

    [CreateAssetMenu(fileName = "BookmarkData", menuName = "ScriptableObjects/BookmarkData", order = 1)]
    public class ProjectBookmarkData : ScriptableObject
    {
        public List<Object> bookmarks;
    }
}