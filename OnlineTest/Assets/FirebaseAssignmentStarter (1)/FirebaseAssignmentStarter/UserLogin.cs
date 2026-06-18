using Firebase.Database;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserLogin : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://shinguwoong-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] InputField NickNameInput;
    [SerializeField] Text CheckText;

    [Header("Scene")]
    [SerializeField] string NextSceneName = "MainScene";
    [SerializeField] bool LoadNextSceneAfterLogin = false;

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();
    }

    // 로그인 버튼에 연결
    public void OnClickLogin()
    {
        string nickName = NickNameInput.text.Trim();

        if (string.IsNullOrEmpty(nickName))
        {
            CheckText.text = "로그인 창에 닉네임을 입력해주세요";
            return;
        }

        Login(nickName);
    }

    void Login(string nickName)
    {
        reference
            .Child("UserInfo")
            .OrderByChild("NickName")
            .EqualTo(nickName)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        CheckText.text = "Firebase에서 읽기오류가 발생했어요";
                    });
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.HasChildren)
                {
                    dispatcher.Enqueue(() =>
                    {
                        CheckText.text = "입력하신 닉네임이 존재하지 않습니다";
                    });
                    return;
                }

                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    string userKey = userSnapshot.Key;

                    dispatcher.Enqueue(() =>
                    {
                        PlayerPrefs.SetString("UserKey", userKey);
                        PlayerPrefs.SetString("UserNickName", nickName);
                        PlayerPrefs.Save();

                        CheckText.text = "로그인 성공하였습니다";

                        if (LoadNextSceneAfterLogin)
                        {
                            SceneManager.LoadScene(NextSceneName);
                        }
                    });

                    break;
                }
            });
    }
}
