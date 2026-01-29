#if !DISABLESTEAMWORKS && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Workshop Item Search")]
    public class SteamWorkshopItemSearch : MonoBehaviour
    {
        [Header("Elements")]
        public TMPro.TMP_InputField searchText;
        public TMPro.TextMeshProUGUI currentPageLabel;
        public TMPro.TextMeshProUGUI pageCountLabel;
        public SteamWorkshopItemDetailData template;
        public Transform content;

        public UgcQuery activeQuery;
        public int CurrentFrom
        {
            get
            {
                if (activeQuery != null)
                {
                    var maxItemIndex = (int)(activeQuery.Page * 50);
                    if (maxItemIndex < activeQuery.matchedRecordCount)
                    {
                        return maxItemIndex - 49;
                    }
                    else
                    {
                        var remainder = (int)(activeQuery.matchedRecordCount % 50);
                        maxItemIndex = maxItemIndex - 50 + remainder;
                        return maxItemIndex - remainder + 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        public int CurrentTo
        {
            get
            {
                if (activeQuery != null)
                {
                    var maxItemIndex = (int)(activeQuery.Page * 50);
                    if (maxItemIndex < activeQuery.matchedRecordCount)
                    {
                        return maxItemIndex;
                    }
                    else
                    {
                        var remainder = (int)(activeQuery.matchedRecordCount % 50);
                        maxItemIndex = maxItemIndex - 50 + remainder;
                        return maxItemIndex;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        public int TotalCount => activeQuery != null ? (int)activeQuery.matchedRecordCount : 0;
        public int CurrentPage => activeQuery != null ? (int)activeQuery.Page : 0;

        [Header("Events")]
        public UnityEvent<UgcQuery> onResultsReady;
        public UnityEvent<UgcQuery> onQueryUpdated;

        private readonly List<SteamWorkshopItemDetailData> currentRecords = new();
        private string lastSearchString = "";

        private string GetSearchString()
        {
            if (searchText != null)
                return searchText.text;
            else
                return string.Empty;
        }

        public void SearchMyPublished()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.GetMyPublished();            
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }

            if (activeQuery.handle != UGCQueryHandle_t.Invalid)
                activeQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchAll()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.Get(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }

            if (activeQuery.handle != UGCQueryHandle_t.Invalid)
                activeQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchSubscribed()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.GetSubscribed();
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }

            if (activeQuery.handle != UGCQueryHandle_t.Invalid)
                activeQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void PrepareSearchAll()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.Get(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }

            if (activeQuery.handle != UGCQueryHandle_t.Invalid)
                onQueryUpdated.Invoke(activeQuery);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchFavorites()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Favorited, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }

            if (activeQuery.handle != UGCQueryHandle_t.Invalid)
                activeQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void PrepareSearchFavorites()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Favorited, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }

            if (activeQuery.handle != UGCQueryHandle_t.Invalid)
                onQueryUpdated.Invoke(activeQuery);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void SearchFollowed()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Followed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }

            if (activeQuery.handle != UGCQueryHandle_t.Invalid)
                activeQuery.Execute(HandleResults);
            else
                Debug.LogError("Steam was unable to create a query handle for this argument. Check your App ID and the App ID in the query manager.");
        }

        public void PrepareSearchFollowed()
        {
            var filter = GetSearchString();
            lastSearchString = filter;
            activeQuery = UgcQuery.Get(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Followed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, new(0), AppData.Me);
            if (!string.IsNullOrEmpty(filter))
            {
                API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, filter);
            }
            onQueryUpdated.Invoke(activeQuery);
        }

        public void ExecuteSearch()
        {
            if (activeQuery != null && activeQuery.handle != UGCQueryHandle_t.Invalid)
                activeQuery.Execute(HandleResults);
            else
                Debug.LogError("Attempted to execute a query with an invalid query handle.");
        }

        public void SetNextSearchPage()
        {
            if (activeQuery != null)
            {
                activeQuery.SetNextPage();

                if (!string.IsNullOrEmpty(lastSearchString))
                    API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, lastSearchString);

                activeQuery.Execute(HandleResults);
            }
            else
            {
                Debug.LogWarning("No active query or the query handle is invalid, you must call a Search or Prepare Search method before iterating over the pages");
            }
        }

        public void SetPreviousSearchPage()
        {
            if (activeQuery != null)
            {
                activeQuery.SetPreviousPage();

                if (!string.IsNullOrEmpty(lastSearchString))
                    API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, lastSearchString);

                activeQuery.Execute(HandleResults);
            }
            else
            {
                Debug.LogWarning("No active query or the query handle is invalid, you must call a Search or Prepare Search method before iterating over the pages");
            }
        }

        public void SetSearchPage(uint page)
        {
            if(activeQuery != null)
            {
                activeQuery.SetPage(page);

                if (!string.IsNullOrEmpty(lastSearchString))
                    API.UserGeneratedContent.Client.SetSearchText(activeQuery.handle, lastSearchString);

                activeQuery.Execute(HandleResults);
            }
            else
            {
                Debug.LogWarning("No active query or the query handle is invalid, you must call a Search or Prepare Search method before iterating over the pages");
            }
        }

        private void HandleResults(UgcQuery query)
        {
            if(currentPageLabel != null)
            currentPageLabel.text = activeQuery.Page.ToString();
            if(pageCountLabel != null)
            pageCountLabel.text = activeQuery.pageCount.ToString();

            if (template != null
                && content != null)
            {
                foreach (var comp in currentRecords)
                    Destroy(comp.gameObject);

                currentRecords.Clear();

                foreach (var result in query.ResultsList)
                {
                    var comp = Instantiate(template, content);
                    currentRecords.Add(comp);

                    comp.Data = result;
                    comp.LoadPreview();
                }
            }
            onResultsReady?.Invoke(activeQuery);
        }
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamWorkshopItemSearch), true)]
    public class SteamWorkshopItemSearchEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif