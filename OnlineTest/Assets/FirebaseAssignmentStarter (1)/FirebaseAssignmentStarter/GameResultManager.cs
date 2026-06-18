using Firebase.Database;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResultManager : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://shinguwoong-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] Text MessageText; // 결과를 띄워줄 공용 메시지 텍스트

    string userKey;

     
    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        userKey = PlayerPrefs.GetString("UserKey");
    }
    public void OnClickGameClear() // 이걸로 체크
    {
        if (string.IsNullOrEmpty(userKey)) return;
        int newScore = 1500;
        int rewardCoin = 300;

        SaveGameResult(newScore, rewardCoin);
    }

    public void OnClickGamePerfectClear() // 이걸로 체크
    {
        if (string.IsNullOrEmpty(userKey)) return;
        int newScore = 3000;
        int rewardCoin = 1000;

        SaveGameResult(newScore, rewardCoin);
    }

    void SaveGameResult(int newScore, int rewardCoin)
    {
        //데이터 불러오고 스코어 체크랑 돈 체크
        reference.Child("UserInfo").Child(userKey).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                dispatcher.Enqueue(() => MessageText.text = "데이터 불러오기 실패");
                return;
            }

            DataSnapshot snapshot = task.Result;
            if (!snapshot.Exists) return;

            int currentScore = int.Parse(snapshot.Child("Score").Value.ToString());
            int currentCoin = int.Parse(snapshot.Child("Coin").Value.ToString());

            Dictionary<string, object> updateData = new Dictionary<string, object>();

            // 현재 코인 + 보상코인
            updateData["Coin"] = currentCoin + rewardCoin;

            //최고 점수 갱신하게끔
            bool isNewRecord = false;
            if (newScore > currentScore)
            {
                updateData["Score"] = newScore;
                isNewRecord = true;
            }

            // Firebase로 보내고
            reference.Child("UserInfo").Child(userKey).UpdateChildrenAsync(updateData).ContinueWith(updateTask =>
            {
                if (updateTask.IsFaulted) return;

                dispatcher.Enqueue(() =>
                {
                    string resultMsg = "청소 완료! " + rewardCoin + " Coin 획득!";
                    if (isNewRecord) resultMsg += "최고 점수 갱신 : " + newScore + "점";
                    MessageText.text = resultMsg;
                    ShopManager shop = FindObjectOfType<ShopManager>();
                    if (shop != null) shop.LoadUserData();
                });
            });
        });
    }
}