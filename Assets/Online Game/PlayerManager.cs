using UnityEngine;
using Photon.Pun;
public class PlayerManager : MonoBehaviourPun
{
    public GameManager GM;
    public GameObject Blocker;
    public bool myTurn;
    private bool Blocking = false;
    public int ID;
    private enum MyColor { Red, Yellow };
    MyColor myColor;

    public void Awake()
    {
        ID = photonView.ViewID;
    }


    private void Start()
    {
        SetupStart();
    }

    public void SetupStart()
    {
        if (GM == null)
        {
            GM = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        GM.PL = this;
        Blocker = GM.Blocker;

        if (photonView.ViewID == 1001)
        {
            myColor = MyColor.Red;
        }
        else
        {
            myColor = MyColor.Yellow;
        }
        //Test ID of player
        if (myColor == MyColor.Red)
        {
            Blocking = false;
            myTurn = true;
        }
        else
        {
            Blocking = true;
            myTurn = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
    }

    /* public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
     {
         if (stream.IsWriting)
         {
             if (Blocker != null)
             {
                 stream.SendNext(!Blocker.active);
                 Debug.Log("message sent");
             }
         }
         else if (stream.IsReading)
         {
             if (Blocker != null)
             {
                 Blocker.SetActive((bool)stream.ReceiveNext());
                 Debug.Log("message read");
             }
         }
     }*/
    public void SwapTurn()
    {
        if (photonView.IsMine)
        {
            myTurn = !myTurn;
            Blocking = !Blocking;
            GM.Blocker.SetActive(Blocking);
            Debug.Log(Blocking + " State");
        }
    }
}
