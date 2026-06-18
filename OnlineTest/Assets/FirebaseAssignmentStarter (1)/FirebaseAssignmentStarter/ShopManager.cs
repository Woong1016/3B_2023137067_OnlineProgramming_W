using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://shinguwoong-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] Text CoinText;
    [SerializeField] Text MessageText;

    string userKey;
    int currentCoin;
    Dictionary<string, int> inventory = new Dictionary<string, int>();

    // 1번 심화과제 
    Dictionary<string, bool> unitList = new Dictionary<string, bool>();

     

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        userKey = PlayerPrefs.GetString("UserKey");

        if (string.IsNullOrEmpty(userKey))
        {
            MessageText.text = "로그인 정보가 다르거나 존재하지 않습니다.";
            return;
        }
        //reference.Child("UserInfo").Child(userKey).KeepSynced(true);
        reference.Child("UserInfo").Child(userKey).KeepSynced(false);
        LoadUserData();
    }

    public void LoadUserData()
    {
        reference
            .Child("UserInfo")
            .Child(userKey)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "유저_정보 불러오기에 실패했습니다.";
                    });
                    return;
                }

                DataSnapshot snapshot = task.Result;

                currentCoin = int.Parse(snapshot.Child("Coin").Value.ToString());

                string inventoryJson = snapshot.Child("Inventory").Value.ToString();
                inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

                string unitListJson = snapshot.Child("UnitList").Value.ToString();
                unitList = JsonConvert.DeserializeObject<Dictionary<string, bool>>(unitListJson);

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    //MessageText.text = "유저_정보를 불러오는데 성공했습니다. "; // 이것도 굳이  필요 없어짐
                    FindObjectOfType<InventoryManager>().LoadInventory();
                });
            });
    }

    void RefreshUI()
    {
        CoinText.text = "Coin : " + currentCoin;
    }

    public void OnClickBuyMop() // 우리게임 아이템 3개로 
    {
        BuyItem("Mop", 100);
    }

    public void OnClickBuyGun()
    {
        BuyItem("Gun", 200);
    }

    public void OnClickBuySpanner()
    {
        BuyItem("Spanner", 300);
    }

    void BuyItem(string itemName, int price)
    {
        if (currentCoin < price)
        {
            MessageText.text = "돈이 부족합니다요";
            return;
        }

        currentCoin -= price;

        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName]++;
        }
        else
        {
            inventory[itemName] = 1;
        }

        SaveUserData(itemName);
    }

    void SaveUserData(string boughtItemName)
    {
        string inventoryJson = JsonConvert.SerializeObject(inventory);

        Dictionary<string, object> updateData = new Dictionary<string, object>();
        updateData["Coin"] = currentCoin;
        updateData["Inventory"] = inventoryJson;

        reference
            .Child("UserInfo")
            .Child(userKey)
            .UpdateChildrenAsync(updateData)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "구매 실패 및 저장 실패";
                    });
                    return;
                }

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = boughtItemName + " 구매가 완료되었습니다.";
                });
            });
    }
    
    public void OnClickBuyUnit2() // 2부터 4까지 1은 이미 있어
    {
        BuyUnit("Unit2", 200);  
    }

    public void OnClickBuyUnit3()
    {
        BuyUnit("Unit3", 300);  
    }
    public void OnClickBuyUnit4()
    {
        BuyUnit("Unit4", 400); 
    }

    void BuyUnit(string unitName, int price)
    {
        if (unitList.ContainsKey(unitName) && unitList[unitName] == true)
        {
            MessageText.text = "이미 보유한 유닛입니다.";
            return;
        }
        if (currentCoin < price)
        {
            MessageText.text = "유닛을 구매하기엔 돈이 부족합니다.";
            return;
        }
        currentCoin -= price;
        unitList[unitName] = true;
        SaveUnitData(unitName);
    }

    void SaveUnitData(string boughtUnitName)
    {
        string unitListJson = JsonConvert.SerializeObject(unitList);

        Dictionary<string, object> updateData = new Dictionary<string, object>();
        updateData["Coin"] = currentCoin;
        updateData["UnitList"] = unitListJson;

        reference
            .Child("UserInfo")
            .Child(userKey)
            .UpdateChildrenAsync(updateData)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "유닛 구매 및 저장 실패";
                    });
                    return;
                }

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = boughtUnitName + " 유닛을 성공적으로 영입했습니다!";
                });
            });
    }

}
