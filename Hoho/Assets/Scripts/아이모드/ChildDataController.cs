using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  //This template can be customized at C:\Program Files\Unity\Hub\Editor\2021.3.8f1\Editor\Data\Resources\ScriptTemplates\81-C# Script-NewBehaviourScript.cs.txt
using System;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;


/// <summary>
/// 아이모드의 데이터 송수신을 책임짐. 
/// </summary>
public class ChildDataController : MonoBehaviour
{
    [FirestoreData]
    public class GameResult
    {
        [FirestoreProperty]
        public string 시작날짜 { get; set; } = "";

        [FirestoreProperty]
        public string 시작시간 { get; set; } = "";

        [FirestoreProperty]
        public int 레벨 { get; set; } = 1;

        [FirestoreProperty]
        public int 별개수 { get; set; } = 0;

        [FirestoreProperty]
        public int 플레이시간 { get; set; } = 0;

        [FirestoreProperty]
        public int 훈련시간 { get; set; } = 0;

        [FirestoreProperty]
        public int 완성률 { get; set; } = 0;

        [FirestoreProperty]
        public Dictionary<string, float> 호흡기록 { get; set; } = ChildDataController.BreatheResult;

        [FirestoreProperty]
        public Dictionary<string, float> 예상호흡기록 { get; set; } = ChildDataController.ExpectedBreatheResult;
    };


    [FirestoreData]
    public class PointInformation
    {
        [FirestoreProperty]
        public int 현재포인트 { get; set; } = 0;

        [FirestoreProperty]
        public int 목표점수 { get; set; } = 1000;

        [FirestoreProperty]
        public int 레벨 { get; set; } = 1;

        [FirestoreProperty]
        public string 보상제목 { get; set; } = "놀이공원";
    }


    public delegate void updateDelegate();

    static public Dictionary<string, int> RLresult = new Dictionary<string, int>();
    static public Dictionary<string, int> CPresult = new Dictionary<string, int>();

    static FirebaseFirestore db;

    static bool isReceived = false; 
    static bool canSend = false;

    /// <summary>
    /// 전체에서 쓰이는 point
    /// </summary>
    static int point=0;

    /// <summary>
    /// 현재 보상을 얻기 위한 목표 점수.
    /// </summary>
    static int goalPoint=1000;

    /// <summary>
    /// 현재 진행 보상 단계. PointShop에서 검은색 점의 개수.
    /// </summary>
    static int level=1;
    /// <summary>
    /// 원 안에 있는 텍스트. 가령, "놀이공원".  
    /// </summary>
    static string rewardTitle="놀이공원";

    /// <summary>
    /// 레벨에 따른 보상 내용들.
    /// </summary>
    public static List<string> rewardTitleList = new List<string>();

    /// <summary>
    /// 현재 점수/목표 점수
    /// </summary>
    static float progressRatio=point/(float) goalPoint;


    /// <summary>
    /// 유저 ID.
    /// </summary>
    static string childID = "001";

    /// <summary>
    /// 부모 ID.
    /// </summary>
    static string parentID = "001";

    /// <summary>
    ///        시작날짜 = "", 시작시간 = "", 레벨 = 0, 별개수 = starNum, 플레이시간 = 0, 호흡기록, 예상호흡기록
    /// </summary>
    public static GameResult fishGameResult = new GameResult();


    /// <summary>
    /// 호흡 결과는 여기에 기록해서 SendGameResult로 보낼 것. timestamp와 0~1 사이의 넣으면 됨. 
    /// </summary>
    public static Dictionary<string, float> BreatheResult = new Dictionary<string, float> { { "0", 0 }, };

    /// <summary>
    /// correctHookPos는 여기에 기록해서 SendGameResult로 보낼 것. timestamp와 0~1 사이의 넣으면 됨. 
    /// </summary>
    public static Dictionary<string, float> ExpectedBreatheResult = new Dictionary<string, float> { { "0", 0 }, };

    /// <summary>
    /// canSend(bool), point (int), level(int), rewardTitle(string), goalPoint(int), progressRatio(float), childID(string)를 담은 Dictionary 반환.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, object> getValues()
    {

        Dictionary<string, object> A = new Dictionary<string, object>();
        A.Add("isReceived", isReceived);
        A.Add("canSend", canSend);
        A.Add("point", point);
        A.Add("level", level);
        A.Add("goalPoint", goalPoint);
        A.Add("rewardTitle", rewardTitle);
        A.Add("rewardTitleList", rewardTitleList);
        A.Add("progressRatio", progressRatio);
        A.Add("childID", childID);
        A.Add("parentID", parentID);

        return A;
    }

    /// <summary>
    /// true로 설정해야 보낼 수 있음. 
    /// </summary>
    /// <param name="canSend"></param>
    public static void setCanSend(bool canSend)
    {
       ChildDataController.canSend=canSend;
    }

    /// <summary>
    /// 전체에서 쓰이는 point
    /// </summary>
    public static void setPoint(int pt)
    {
        point = pt;
    }

    /// <summary>
    /// 아이 모드의 point를 더함. 레벨업, 보상 내용까지 알아서 수정해줌. 보상 내용은 포인트샵을 가야 초기화됨. 
    /// </summary>
    /// <param name="pt"></param>
    public static void addPoint(int pt)
    {
        point = point+pt;
        if (point > goalPoint && level-1<rewardTitleList.Count)
        {
            level = level + 1;
            rewardTitle = rewardTitleList[level - 1];
        }
    }


    /// <summary>
    /// 현재 보상을 얻기 위한 목표 점수.
    /// </summary>
    public static void setGoalPoint(int pt)
    {
        goalPoint = pt;
    }


    /// <summary>
    /// 현재 진행 보상 단계. PointShop에서 검은색 점의 개수.
    /// </summary>
    public static void setLevel(int lv)
    {
        level = lv;
    }

    public static void setRewardTitle(string title)
    {
        rewardTitle = title;
    }

    public static void setRewardTitleList(List<string> titleList)
    {
        rewardTitleList.Clear();
        foreach(string title in titleList)
        {
            rewardTitleList.Add(title);
        }
    }

    /// <summary>
    /// 현재 점수/목표 점수
    /// </summary>
    public static void setProgressRatio(float ratio)
    {
        progressRatio = ratio;
    }


    /// <summary>
    /// 유저 ID.
    /// </summary>
    public static void setChildID(string id)
    {
        childID = id;
    }

    public static void setParentID(string id)
    {
        parentID = id;
    }


    /// <summary>
    /// 서버에 포인트, level, rewardTitle, pointString, 진행률, childID를 보냄. 
    /// </summary>
    public static void SendPoint()
    {
        if (!canSend)
        {
            Debug.Log("Not yet prepared.");
        }

        Debug.Log("Send point.");
        DocumentReference docRef = db.Collection("ChildrenUsers").Document(childID).Collection("Point").Document("CurrentPoint");
        Dictionary<string, object> user = new Dictionary<string, object>
        {
                { "현재포인트", point},
                { "레벨", level },
                { "보상제목", rewardTitle },
                { "목표점수", goalPoint },
        };

        docRef.SetAsync(user).ContinueWithOnMainThread(task => {
            
            if (task.IsCompleted)
            {
                Debug.Log("Added data to the document in the users collection.");
            }
            else { Debug.Log("Failed"); }
        });
    }

    public static void SendGameResult()
    {
        fishGameResult.호흡기록 = BreatheResult;
        fishGameResult.예상호흡기록 = ExpectedBreatheResult;

        Debug.Log( "Keys : "+fishGameResult.호흡기록.Keys.ToString());

        if (!canSend)
        {
            Debug.Log("Not yet prepared.");
        }

        Debug.Log("Send point for GameResult");

        string today = fishGameResult.시작날짜;       
        Debug.Log(today);
        Query todayQuery = db.Collection("ChildrenUsers").Document(childID).Collection("Point").Document("FishPoint").Collection("Results").WhereEqualTo("시작날짜", today);        

        todayQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {     
            
            QuerySnapshot todayQuerySnapshot = task.Result;
            string documentName = today + "_" + (todayQuerySnapshot.Count + 1);            

            DocumentReference docRef = db.Collection("ChildrenUsers").Document(childID).Collection("Point").Document("FishPoint").Collection("Results").Document(documentName);

            //Debug.Log(documentName+"\n시작날짜 : " + fishGameResult.시작날짜 + "\n" + "시작시간 : " + fishGameResult.시작시간 + "\n" + "레벨 : "+ fishGameResult.레벨 + "\n별개수 : " + fishGameResult.별개수 + "\n플레이시간 : " + fishGameResult.플레이시간+"\n호흡기록 : "+fishGameResult.호흡기록.Keys.Count);

            docRef.SetAsync(fishGameResult).ContinueWithOnMainThread(task => {

                if (task.IsCompleted)
                {
                    Debug.Log("Added data to the document "+documentName+" in the users collection.");
                }
                else { Debug.Log("Failed"); }
            });
        });
    }

    public static void ReceivePoint(updateDelegate updateCircle)
    {
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        DocumentReference pointDoc = db.Collection("ChildrenUsers").Document(childID).Collection("Point").Document("CurrentPoint");

        pointDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            { // document가 없으면 false
              //snapshot.ID
                Debug.Log("ChildDataController.receivePoint");
                PointInformation pointInfo = snapshot.ConvertTo<PointInformation>();
                level = pointInfo.레벨;
                goalPoint=pointInfo.목표점수;
                rewardTitle=pointInfo.보상제목;
                point=pointInfo.현재포인트;

                isReceived = true;
            }
            else
            {
                Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
            }
          
        });

        updateCircle();

    }


    static public void receiveTimeCustom()
    {
        var result = new Dictionary<string, float>();

        /*
        int Exhale = new Dictionary<string, int>();
        var ExhaleStop = new Dictionary<string, int>();
        var Inhale = new Dictionary<string, int>();
        var InhalStop = new Dictionary<string, int>();
        var TotalTime = new Dictionary<string, int>(); 
        */
            
        Query TSQuery = db.Collection("ParentUsers").Document(parentID).Collection("TimeCustom");
        TSQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot TSQuerySnapshot = task.Result;
            foreach(DocumentSnapshot doc in TSQuerySnapshot.Documents)
            {
                var docDictionary = doc.ToDictionary();

                string log = "read TimeCustom data : ";

                foreach (KeyValuePair<string, object> pair in docDictionary) 
                {
                    result.Add(pair.Key, float.Parse(pair.Value.ToString()));
                    log += pair.Key + " : " + pair.Value + "\n";
                }
                Debug.Log(log);
            }
            FishGenerator.upTime = result["Inhale"];
            FishGenerator.upWaitTime = result["InhaleStop"];
            FishGenerator.downTime = result["Exhale"];
            FishGenerator.downWaitTime = result["ExhaleStop"];
        });          
    }

    static public void receiveRewardList(updateDelegate updateReward)
    {
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }
        Query RLquery = db.Collection("ParentUsers").Document(parentID).Collection("Point").WhereEqualTo("type", "list");
        RLquery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot RLQuerySnapshot = task.Result;
            //Debug.Log(" : "+RLQuerySnapshot.Count);
            foreach (DocumentSnapshot doc in RLQuerySnapshot.Documents)
            {
                //Debug.Log("123");
                Dictionary<string, object> RewardLists = doc.ToDictionary();
                //Debug.Log("123");


                foreach (KeyValuePair<string, object> pair in RewardLists)
                {
                    //Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                    //Debug.Log(pair.Key == "레벨");
                    //Debug.Log("디버깅 : "+RewardLists[pair.Key]);                 
                }

                //Debug.Log("레벨 : "+RewardLists["레벨"].ToString());

                int level = System.Int32.Parse(RewardLists["레벨"].ToString());

                //Debug.Log("level is " + level);
                //Debug.Log(level);
                int point = System.Int32.Parse(RewardLists["포인트"].ToString());
                //Debug.Log("123");
                //Debug.Log("level : "+level + ", point : " + point);

                ChildDataController.RLresult.Add("포인트_"+ level.ToString(), point);

            }
            updateReward();
        });


    }

    static public void receiveCompPoint(updateDelegate updateReward)
    {
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }
        Query CPquery = db.Collection("ParentUsers").Document(parentID).Collection("Point").WhereEqualTo("type", "card");
        CPquery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            //Debug.Log("receiving CompPoint");
            QuerySnapshot CPQuerySnapshot = task.Result;
            Debug.Log("receiving CompPoint : " + CPQuerySnapshot.Count);
            int idx = 1;
            foreach (DocumentSnapshot doc in CPQuerySnapshot.Documents)
            {
                Dictionary<string, object> CompPoint = doc.ToDictionary();

                foreach (KeyValuePair<string, object> pair in CompPoint)
                {
                    //Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                    Debug.Log("디버깅 : " + CompPoint[pair.Key]);
                }

                int point = System.Int32.Parse(CompPoint["포인트"].ToString());
                Debug.Log("포인트 파싱");
                //Debug.Log("level : "+level + ", point : " + point);

                ChildDataController.CPresult.Add("포인트_" + idx, point);
                idx++;

            }
            updateReward();
        });


    }



    public void UpdateData()
    {
        DocumentReference docRef = db.Collection("users").Document("aturing");
        Dictionary<string, object> user = new Dictionary<string, object>
        {
                { "First", "Alan" },
                { "Middle", "Mathison" },
                { "Last", "Turing" },
                { "Born", 1912 }
        };
        docRef.SetAsync(user).ContinueWithOnMainThread(task => {
            Debug.Log("Added data to the aturing document in the users collection.");
        });
    }

    public void ReadData()
    {
        Debug.Log("SSSS Firebase Read Data.");
        CollectionReference usersRef = db.Collection("users");
        usersRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Debug.Log(String.Format("User: {0}", document.Id));
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                Debug.Log(String.Format("First: {0}", documentDictionary["First"]));
                if (documentDictionary.ContainsKey("Middle"))
                {
                    Debug.Log(String.Format("Middle: {0}", documentDictionary["Middle"]));
                }

                Debug.Log(String.Format("Last: {0}", documentDictionary["Last"]));
                Debug.Log(String.Format("Born: {0}", documentDictionary["Born"]));
            }

            Debug.Log("Read all data from the users collection.");
        });
    }

    // Start is called before the first frame update
    void Start()
    {        
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }
        DontDestroyOnLoad(gameObject);  
    }
}
