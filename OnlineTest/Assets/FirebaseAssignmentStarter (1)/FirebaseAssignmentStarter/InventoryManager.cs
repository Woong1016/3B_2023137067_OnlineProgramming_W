using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://shinguwoong-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] Text MopCountText;
    [SerializeField] Text GunCountText;
    [SerializeField] Text SpannerCountText;
    [SerializeField] Text MessageText;

    string userKey;
    Dictionary<string, int> inventory = new Dictionary<string, int>();

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        userKey = PlayerPrefs.GetString("UserKey");

        if (string.IsNullOrEmpty(userKey))
        {
            MessageText.text = "로그인 정보가 일치하지 않거나 정보가 없습니다.";
            return;
        }
        //reference.Child("UserInfo").Child(userKey).KeepSynced(true);
        LoadInventory();
    }

    public void LoadInventory()
    {
        reference
            .Child("UserInfo")
            .Child(userKey)
            .Child("Inventory")
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 불러오는데 실패했습니다.";
                    });
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (snapshot.Value == null)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 내에 데이터가 없습니다.";
                    });
                    return;
                }

                string inventoryJson = snapshot.Value.ToString();
                inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    //MessageText.text = "인벤토리 불러오기 완료 되었습니다.";
                    FindObjectOfType<ShopManager>().LoadUserData();
                });
            });
    }

    void RefreshUI()
    {
        MopCountText.text = "Mop : " + GetItemCount("Mop");
        GunCountText.text = "Gun : " + GetItemCount("Gun");
        SpannerCountText.text = "Spanner : " + GetItemCount("Spanner");
    }

    int GetItemCount(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            return inventory[itemName];
        }

        return 0;
    }

    public void OnClickUseMop()
    {
        UseItem("Mop");
    }

    public void OnClickUseGun()
    {
        UseItem("Gun");
    }

    public void OnClickUseSpanner()
    {
        UseItem("Spanner");
    }

    void UseItem(string itemName)
    {
        if (!inventory.ContainsKey(itemName) || inventory[itemName] <= 0)
        {
            MessageText.text = itemName + " 개수가 부족합니다.";
            return;
        }

        inventory[itemName]--;
        SaveInventory(itemName);
    }

    void SaveInventory(string usedItemName)
    {
        string inventoryJson = JsonConvert.SerializeObject(inventory);

        reference
            .Child("UserInfo")
            .Child(userKey)
            .Child("Inventory")
            .SetValueAsync(inventoryJson)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 저장 실패하였습니다.";
                    });
                    return;
                }

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = usedItemName + " 사용했습니다.";
                });
            });
    }
}
