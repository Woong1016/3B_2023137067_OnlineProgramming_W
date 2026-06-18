using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketManager : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://shinguwoong-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] Text MarketMessageText;

    string userKey;

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();
        userKey = PlayerPrefs.GetString("UserKey");
    }

    public void OnClickSellMop()
    {
        string sellItemName = "Mop";
        int sellPrice = 150;

        reference.Child("UserInfo").Child(userKey).GetValueAsync().ContinueWith(task =>
        {
            // 방어 코드
            if (task.IsFaulted || !task.Result.Exists)
            {
                dispatcher.Enqueue(() => MarketMessageText.text = "유저 정보를 불러오지 못했습니다.");
                return;
            }

            DataSnapshot snapshot = task.Result;

            string inventoryJson = snapshot.Child("Inventory").Value.ToString();
            Dictionary<string, int> inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

            // 인벤토리에 아이템이 있는지 확인
            if (!inventory.ContainsKey(sellItemName) || inventory[sellItemName] <= 0)
            {
                dispatcher.Enqueue(() => MarketMessageText.text = "판매할 " + sellItemName + "이(가) 부족합니다.");
                return;
            }
            inventory[sellItemName]--;
            reference.Child("UserInfo").Child(userKey).Child("Inventory").SetValueAsync(JsonConvert.SerializeObject(inventory));

            // 마켓에 물건 등록 (판매자 키, 아이템, 가격)
            DatabaseReference newMarketRef = reference.Child("Market").Push();
            Dictionary<string, object> marketData = new Dictionary<string, object>
            {
                { "SellerKey", userKey },
                { "ItemName", sellItemName },
                { "Price", sellPrice }
            };

            newMarketRef.SetValueAsync(marketData).ContinueWith(sellTask =>
            {
                // 방어 코드
                if (sellTask.IsFaulted) return;
                dispatcher.Enqueue(() => MarketMessageText.text = sellItemName + " 판매 등록 완료!");
            });
        });
    }

    public void OnClickBuyFromMarket()
    {
        reference.Child("Market").LimitToFirst(1).GetValueAsync().ContinueWith(task =>
        {
            // 방어 코드
            if (task.IsFaulted || !task.Result.HasChildren)
            {
                dispatcher.Enqueue(() => MarketMessageText.text = "경매장에 등록된 물건이 없습니다.");
                return;
            }

            DataSnapshot marketSnapshot = null;
            foreach (var child in task.Result.Children) marketSnapshot = child;

            string marketKey = marketSnapshot.Key;
            string sellerKey = marketSnapshot.Child("SellerKey").Value.ToString();
            string itemName = marketSnapshot.Child("ItemName").Value.ToString();
            int price = int.Parse(marketSnapshot.Child("Price").Value.ToString());

            if (sellerKey == userKey)
            {
                dispatcher.Enqueue(() => MarketMessageText.text = "자신이 등록한 물건은 살 수 없습니다.");
                return;
            }

            // 구매자 데이터 확인 및 갱신
            reference.Child("UserInfo").Child(userKey).GetValueAsync().ContinueWith(buyerTask =>
            {
                // 방어 코드
                if (buyerTask.IsFaulted || !buyerTask.Result.Exists) return;

                DataSnapshot buyerSnap = buyerTask.Result;
                int myCoin = int.Parse(buyerSnap.Child("Coin").Value.ToString());

                if (myCoin < price)
                {
                    dispatcher.Enqueue(() => MarketMessageText.text = "코인이 부족합니다요");
                    return;
                }

                // 구매자 돈 차감이랑 인벤토리 증가
                myCoin -= price;
                Dictionary<string, int> myInv = JsonConvert.DeserializeObject<Dictionary<string, int>>(buyerSnap.Child("Inventory").Value.ToString());
                if (myInv.ContainsKey(itemName)) myInv[itemName]++;
                else myInv[itemName] = 1;

                reference.Child("UserInfo").Child(userKey).Child("Coin").SetValueAsync(myCoin);
                reference.Child("UserInfo").Child(userKey).Child("Inventory").SetValueAsync(JsonConvert.SerializeObject(myInv));

                reference.Child("UserInfo").Child(sellerKey).Child("Coin").GetValueAsync().ContinueWith(sellerTask =>
                {
                    // 방어 코드
                    if (sellerTask.IsFaulted || !sellerTask.Result.Exists) return;

                    int sellerCoin = int.Parse(sellerTask.Result.Value.ToString());
                    reference.Child("UserInfo").Child(sellerKey).Child("Coin").SetValueAsync(sellerCoin + price);
                });
                reference.Child("Market").Child(marketKey).RemoveValueAsync().ContinueWith(removeTask =>
                {
                    // 방어 코드
                    if (removeTask.IsFaulted) return;
                    dispatcher.Enqueue(() => MarketMessageText.text = "경매장에서 " + itemName + " 구매 완료!");
                });
            });
        });
    }
}