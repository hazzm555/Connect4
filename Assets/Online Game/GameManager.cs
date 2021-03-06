using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{

    public Transform[] DropPoints;
    private bool End = false;
    public Button[] DropButtons;
    public GameObject RedCoin, YellowCoin;
    public TextMeshProUGUI TurnText, Timer;
    private bool Turn = true; // true for Red, false for Yellow
    public int[,] boardSpots;
    private bool[] RedCoins;
    private bool[] YellowCoins;
    private int EmptyCells = 42;
    private int boardSize = 42;
    public GameObject Board, GiveUpPanel, PauseMenu, Menu, MenuP, WinP, LoseP, DrawP;
    private float Time1;
    public float DefaultTime = 30f, TurnSwappingDelay = 1.5f, EndDelay = 3f;
    private enum GameMode { Online, Offline }
    GameMode GameType { set; get; }
    public enum MyColor { Red, Yellow }
    MyColor myColor { set; get; }

    public GameObject Blocker, PlayerPre;
    public PlayerManager PL;
    //public int PL_ID;
    public Text PlayerColorText;
    public string Name, EnemyName;


    void Start()
    {
        // offline
        Time1 = DefaultTime;
        GameType = GameMode.Online;
        RedCoins = new bool[boardSize];
        YellowCoins = new bool[boardSize];
        boardSpots = new int[6, 7];
        for (int i = 0; i < boardSpots.GetLength(0); i++)
        {
            for (int j = 0; j < boardSpots.GetLength(1); j++)
            {

                boardSpots[i, j] = i * 7 + j;
            }
        }

        //online
        Online();
    }

    private void Update()
    {
        if (!End)
        {
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                StartCoroutine(Win(Turn));
            }
            Time1 -= Time.deltaTime;
            if (Time1 <= 0 && ((Turn && myColor == MyColor.Red) || (!Turn && myColor == MyColor.Yellow)))
            {
                RandomDrop();
            }

            Timer.text = "Time: " + Math.Round(Time1, 1);
        }
        
    }

    [PunRPC]
    public void DataSender(string EnemyName)
    {
        this.EnemyName = EnemyName;
    }

    public void Online()
    {
        Name = DataSaver.Name;
        photonView.RPC("DataSender", RpcTarget.Others, Name);

        PL = PhotonNetwork.Instantiate(PlayerPre.name, PlayerPre.transform.position, PlayerPre.transform.rotation).GetComponent<PlayerManager>();
        PL.GM = this;
        if (PL.photonView.ViewID == 1001)
        {
            myColor = MyColor.Red;
            PlayerColorText.text = "Red";
            PlayerColorText.color = Color.red;
            EnableArr(DropButtons);
        }
        else
        {
            myColor = MyColor.Yellow;
            PlayerColorText.text = "Yellow";
            PlayerColorText.color = Color.yellow;
            DisableArr(DropButtons);
        }
        //PL_ID = PL.ID;



    }

    // Entered whenever the player click any of the drop buttons. 
    //paused to test online method RPC ???????????????????????????????????????????????????????

    public void ButtonDropCoinClicked()
    {
        int[] DropPoint = new int[1];
        string clickedName = EventSystem.current.currentSelectedGameObject.name;
        for (int i = 0; i < DropButtons.Length; i++)
        {
            if (clickedName == DropButtons[i].name)
            {
                DropPoint[0] = i;
                StartCoroutine(DropCoin(i));
                break;
            }
        }
        photonView.RPC("ButtonDropCoinClicked2", RpcTarget.Others, DropPoint);
    }

    [PunRPC]
    public void ButtonDropCoinClicked2(int[] DropPoint)
    {
        StartCoroutine(DropCoin(DropPoint[0]));
    }
    //TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTESTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT

    private IEnumerator DropCoin(int i)
    {
        DisableArr(DropButtons);
        End = true;

        if (Turn)
        {
            Instantiate(RedCoin, DropPoints[i].transform.position, Quaternion.identity, DropPoints[i].transform);
            if (myColor == MyColor.Red)
            {
                TurnText.text = EnemyName + " turn";
            }
            else
            {
                TurnText.text = Name + " turn";
            }
        }
        else
        {
            Instantiate(YellowCoin, DropPoints[i].transform.position, Quaternion.identity, DropPoints[i].transform);
            if (myColor == MyColor.Red)
            {
                TurnText.text = Name + " turn";
            }
            else
            {
                TurnText.text = EnemyName + " turn";
            }
        }

        // Here
        for (int i1 = 0; i1 < boardSpots.GetLength(0); i1++)
        {
            if (boardSpots[i1, i] != -1)
            {
                EmptyCells--;
                if (Turn)
                {
                    RedCoins[boardSpots[i1, i]] = true;
                    if (boardSpots[i1, i] >= 35)
                    {
                        DropButtons[i].gameObject.SetActive(false);
                    }
                    boardSpots[i1, i] = -1;
                    break;
                }
                else
                {
                    YellowCoins[boardSpots[i1, i]] = true;
                    if (boardSpots[i1, i] >= 35)
                    {
                        DropButtons[i].gameObject.SetActive(false);
                    }
                    boardSpots[i1, i] = -1;
                    break;
                }
            }
        }
        yield return new WaitForSeconds(TurnSwappingDelay);
        EnableArr(DropButtons);
        Time1 = DefaultTime;
        End = false;
        CheckWin(Turn);
        //PL.SwapTurn();
        Turn = !Turn;
        SwapTurn();
    }

    public void SwapTurn()
    {
        if (Turn && myColor == MyColor.Yellow || !Turn && myColor == MyColor.Red)
        {
            DisableArr(DropButtons);
        }
        else
        {
            EnableArr(DropButtons);
        }

        //Blocker.SetActive(!Blocker.active);

    }

    private void CheckWin(bool Turn)
    {

        if (Turn)
        {
            if (RedCoins[0] && RedCoins[1] && RedCoins[2] && RedCoins[3])
                StartCoroutine(Win(Turn));
            else if (RedCoins[4] && RedCoins[1] && RedCoins[2] && RedCoins[3])
                StartCoroutine(Win(Turn));
            else if (RedCoins[4] && RedCoins[5] && RedCoins[2] && RedCoins[3])
                StartCoroutine(Win(Turn));
            else if (RedCoins[4] && RedCoins[5] && RedCoins[6] && RedCoins[3])
                StartCoroutine(Win(Turn));
            else if (RedCoins[7] && RedCoins[8] && RedCoins[9] && RedCoins[10])
                StartCoroutine(Win(Turn));
            else if (RedCoins[11] && RedCoins[8] && RedCoins[9] && RedCoins[10])
                StartCoroutine(Win(Turn));
            else if (RedCoins[11] && RedCoins[12] && RedCoins[9] && RedCoins[10])
                StartCoroutine(Win(Turn));
            else if (RedCoins[11] && RedCoins[12] && RedCoins[13] && RedCoins[10])
                StartCoroutine(Win(Turn));
            else if (RedCoins[14] && RedCoins[15] && RedCoins[16] && RedCoins[17])
                StartCoroutine(Win(Turn));
            else if (RedCoins[18] && RedCoins[15] && RedCoins[16] && RedCoins[17])
                StartCoroutine(Win(Turn));
            else if (RedCoins[18] && RedCoins[19] && RedCoins[16] && RedCoins[17])
                StartCoroutine(Win(Turn));
            else if (RedCoins[18] && RedCoins[19] && RedCoins[20] && RedCoins[17])
                StartCoroutine(Win(Turn));
            else if (RedCoins[21] && RedCoins[22] && RedCoins[23] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[25] && RedCoins[22] && RedCoins[23] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[25] && RedCoins[26] && RedCoins[23] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[25] && RedCoins[26] && RedCoins[27] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[28] && RedCoins[29] && RedCoins[30] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[32] && RedCoins[29] && RedCoins[30] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[32] && RedCoins[33] && RedCoins[30] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[32] && RedCoins[33] && RedCoins[34] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[35] && RedCoins[36] && RedCoins[37] && RedCoins[38])
                StartCoroutine(Win(Turn));
            else if (RedCoins[39] && RedCoins[36] && RedCoins[37] && RedCoins[38])
                StartCoroutine(Win(Turn));
            else if (RedCoins[39] && RedCoins[40] && RedCoins[37] && RedCoins[38])
                StartCoroutine(Win(Turn));
            else if (RedCoins[39] && RedCoins[40] && RedCoins[41] && RedCoins[38])
                StartCoroutine(Win(Turn));
            else if (RedCoins[0] && RedCoins[7] && RedCoins[14] && RedCoins[21])
                StartCoroutine(Win(Turn));
            else if (RedCoins[28] && RedCoins[7] && RedCoins[14] && RedCoins[21])
                StartCoroutine(Win(Turn));
            else if (RedCoins[28] && RedCoins[35] && RedCoins[14] && RedCoins[21])
                StartCoroutine(Win(Turn));
            else if (RedCoins[1] && RedCoins[8] && RedCoins[15] && RedCoins[22])
                StartCoroutine(Win(Turn));
            else if (RedCoins[29] && RedCoins[8] && RedCoins[15] && RedCoins[22])
                StartCoroutine(Win(Turn));
            else if (RedCoins[29] && RedCoins[36] && RedCoins[15] && RedCoins[22])
                StartCoroutine(Win(Turn));
            else if (RedCoins[2] && RedCoins[9] && RedCoins[16] && RedCoins[23])
                StartCoroutine(Win(Turn));
            else if (RedCoins[30] && RedCoins[9] && RedCoins[16] && RedCoins[23])
                StartCoroutine(Win(Turn));
            else if (RedCoins[30] && RedCoins[37] && RedCoins[16] && RedCoins[23])
                StartCoroutine(Win(Turn));
            else if (RedCoins[3] && RedCoins[10] && RedCoins[17] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[31] && RedCoins[10] && RedCoins[17] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[31] && RedCoins[38] && RedCoins[17] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[4] && RedCoins[11] && RedCoins[18] && RedCoins[25])
                StartCoroutine(Win(Turn));
            else if (RedCoins[32] && RedCoins[11] && RedCoins[18] && RedCoins[25])
                StartCoroutine(Win(Turn));
            else if (RedCoins[32] && RedCoins[39] && RedCoins[18] && RedCoins[25])
                StartCoroutine(Win(Turn));
            else if (RedCoins[5] && RedCoins[12] && RedCoins[19] && RedCoins[26])
                StartCoroutine(Win(Turn));
            else if (RedCoins[33] && RedCoins[12] && RedCoins[19] && RedCoins[26])
                StartCoroutine(Win(Turn));
            else if (RedCoins[33] && RedCoins[40] && RedCoins[19] && RedCoins[26])
                StartCoroutine(Win(Turn));
            else if (RedCoins[6] && RedCoins[13] && RedCoins[20] && RedCoins[27])
                StartCoroutine(Win(Turn));
            else if (RedCoins[34] && RedCoins[13] && RedCoins[20] && RedCoins[27])
                StartCoroutine(Win(Turn));
            else if (RedCoins[34] && RedCoins[41] && RedCoins[20] && RedCoins[27])
                StartCoroutine(Win(Turn));
            else if (RedCoins[14] && RedCoins[22] && RedCoins[30] && RedCoins[38])
                StartCoroutine(Win(Turn));
            else if (RedCoins[7] && RedCoins[15] && RedCoins[23] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[39] && RedCoins[15] && RedCoins[23] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[0] && RedCoins[8] && RedCoins[16] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[32] && RedCoins[8] && RedCoins[16] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[32] && RedCoins[40] && RedCoins[16] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[1] && RedCoins[9] && RedCoins[17] && RedCoins[25])
                StartCoroutine(Win(Turn));
            else if (RedCoins[33] && RedCoins[9] && RedCoins[17] && RedCoins[25])
                StartCoroutine(Win(Turn));
            else if (RedCoins[33] && RedCoins[41] && RedCoins[17] && RedCoins[25])
                StartCoroutine(Win(Turn));
            else if (RedCoins[2] && RedCoins[10] && RedCoins[18] && RedCoins[26])
                StartCoroutine(Win(Turn));
            else if (RedCoins[34] && RedCoins[10] && RedCoins[18] && RedCoins[26])
                StartCoroutine(Win(Turn));
            else if (RedCoins[3] && RedCoins[11] && RedCoins[19] && RedCoins[27])
                StartCoroutine(Win(Turn));
            else if (RedCoins[20] && RedCoins[26] && RedCoins[32] && RedCoins[38])
                StartCoroutine(Win(Turn));
            else if (RedCoins[13] && RedCoins[19] && RedCoins[25] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[37] && RedCoins[19] && RedCoins[25] && RedCoins[31])
                StartCoroutine(Win(Turn));
            else if (RedCoins[6] && RedCoins[12] && RedCoins[18] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[30] && RedCoins[12] && RedCoins[18] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[30] && RedCoins[36] && RedCoins[18] && RedCoins[24])
                StartCoroutine(Win(Turn));
            else if (RedCoins[5] && RedCoins[11] && RedCoins[17] && RedCoins[23])
                StartCoroutine(Win(Turn));
            else if (RedCoins[29] && RedCoins[11] && RedCoins[17] && RedCoins[23])
                StartCoroutine(Win(Turn));
            else if (RedCoins[29] && RedCoins[35] && RedCoins[17] && RedCoins[23])
                StartCoroutine(Win(Turn));
            else if (RedCoins[4] && RedCoins[10] && RedCoins[16] && RedCoins[22])
                StartCoroutine(Win(Turn));
            else if (RedCoins[28] && RedCoins[10] && RedCoins[16] && RedCoins[22])
                StartCoroutine(Win(Turn));
            else if (RedCoins[3] && RedCoins[9] && RedCoins[15] && RedCoins[21])
                StartCoroutine(Win(Turn));



        }

        else
        {
            if (YellowCoins[0] && YellowCoins[1] && YellowCoins[2] && YellowCoins[3])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[4] && YellowCoins[1] && YellowCoins[2] && YellowCoins[3])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[4] && YellowCoins[5] && YellowCoins[2] && YellowCoins[3])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[4] && YellowCoins[5] && YellowCoins[6] && YellowCoins[3])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[7] && YellowCoins[8] && YellowCoins[9] && YellowCoins[10])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[11] && YellowCoins[8] && YellowCoins[9] && YellowCoins[10])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[11] && YellowCoins[12] && YellowCoins[9] && YellowCoins[10])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[11] && YellowCoins[12] && YellowCoins[13] && YellowCoins[10])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[14] && YellowCoins[15] && YellowCoins[16] && YellowCoins[17])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[18] && YellowCoins[15] && YellowCoins[16] && YellowCoins[17])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[18] && YellowCoins[19] && YellowCoins[16] && YellowCoins[17])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[18] && YellowCoins[19] && YellowCoins[20] && YellowCoins[17])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[21] && YellowCoins[22] && YellowCoins[23] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[25] && YellowCoins[22] && YellowCoins[23] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[25] && YellowCoins[26] && YellowCoins[23] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[25] && YellowCoins[26] && YellowCoins[27] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[28] && YellowCoins[29] && YellowCoins[30] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[32] && YellowCoins[29] && YellowCoins[30] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[32] && YellowCoins[33] && YellowCoins[30] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[32] && YellowCoins[33] && YellowCoins[34] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[35] && YellowCoins[36] && YellowCoins[37] && YellowCoins[38])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[39] && YellowCoins[36] && YellowCoins[37] && YellowCoins[38])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[39] && YellowCoins[40] && YellowCoins[37] && YellowCoins[38])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[39] && YellowCoins[40] && YellowCoins[41] && YellowCoins[38])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[0] && YellowCoins[7] && YellowCoins[14] && YellowCoins[21])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[28] && YellowCoins[7] && YellowCoins[14] && YellowCoins[21])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[28] && YellowCoins[35] && YellowCoins[14] && YellowCoins[21])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[1] && YellowCoins[8] && YellowCoins[15] && YellowCoins[22])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[29] && YellowCoins[8] && YellowCoins[15] && YellowCoins[22])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[29] && YellowCoins[36] && YellowCoins[15] && YellowCoins[22])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[2] && YellowCoins[9] && YellowCoins[16] && YellowCoins[23])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[30] && YellowCoins[9] && YellowCoins[16] && YellowCoins[23])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[30] && YellowCoins[37] && YellowCoins[16] && YellowCoins[23])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[3] && YellowCoins[10] && YellowCoins[17] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[31] && YellowCoins[10] && YellowCoins[17] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[31] && YellowCoins[38] && YellowCoins[17] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[4] && YellowCoins[11] && YellowCoins[18] && YellowCoins[25])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[32] && YellowCoins[11] && YellowCoins[18] && YellowCoins[25])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[32] && YellowCoins[39] && YellowCoins[18] && YellowCoins[25])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[5] && YellowCoins[12] && YellowCoins[19] && YellowCoins[26])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[33] && YellowCoins[12] && YellowCoins[19] && YellowCoins[26])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[33] && YellowCoins[40] && YellowCoins[19] && YellowCoins[26])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[6] && YellowCoins[13] && YellowCoins[20] && YellowCoins[27])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[34] && YellowCoins[13] && YellowCoins[20] && YellowCoins[27])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[34] && YellowCoins[41] && YellowCoins[20] && YellowCoins[27])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[14] && YellowCoins[22] && YellowCoins[30] && YellowCoins[38])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[7] && YellowCoins[15] && YellowCoins[23] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[39] && YellowCoins[15] && YellowCoins[23] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[0] && YellowCoins[8] && YellowCoins[16] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[32] && YellowCoins[8] && YellowCoins[16] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[32] && YellowCoins[40] && YellowCoins[16] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[1] && YellowCoins[9] && YellowCoins[17] && YellowCoins[25])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[33] && YellowCoins[9] && YellowCoins[17] && YellowCoins[25])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[33] && YellowCoins[41] && YellowCoins[17] && YellowCoins[25])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[2] && YellowCoins[10] && YellowCoins[18] && YellowCoins[26])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[34] && YellowCoins[10] && YellowCoins[18] && YellowCoins[26])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[3] && YellowCoins[11] && YellowCoins[19] && YellowCoins[27])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[20] && YellowCoins[26] && YellowCoins[32] && YellowCoins[38])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[13] && YellowCoins[19] && YellowCoins[25] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[37] && YellowCoins[19] && YellowCoins[25] && YellowCoins[31])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[6] && YellowCoins[12] && YellowCoins[18] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[30] && YellowCoins[12] && YellowCoins[18] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[30] && YellowCoins[36] && YellowCoins[18] && YellowCoins[24])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[5] && YellowCoins[11] && YellowCoins[17] && YellowCoins[23])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[29] && YellowCoins[11] && YellowCoins[17] && YellowCoins[23])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[29] && YellowCoins[35] && YellowCoins[17] && YellowCoins[23])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[4] && YellowCoins[10] && YellowCoins[16] && YellowCoins[22])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[28] && YellowCoins[10] && YellowCoins[16] && YellowCoins[22])
                StartCoroutine(Lose(Turn));
            else if (YellowCoins[3] && YellowCoins[9] && YellowCoins[15] && YellowCoins[21])
                StartCoroutine(Lose(Turn));
        }
        if (EmptyCells == 0)
        {
            StartCoroutine(Draw());
        }



    }


    private void DisableArr(Button[] arr)
    {
        foreach (Button x in arr)
        {
            x.gameObject.SetActive(false);
        }
    }

    private void EnableArr(Button[] arr)
    {
        for (int i = 0; i < arr.GetLength(0); i++)
        {

            if (boardSpots[5, i] != -1)
            {
                arr[i].gameObject.SetActive(true);
            }

        }
    }

    private void RandomDrop()
    {
        int SpawnAt = UnityEngine.Random.Range(0, 7);
        int[] x = new int[1];
        if (DropButtons[SpawnAt].IsActive())
        {
            StartCoroutine(DropCoin(SpawnAt));
            x[0] = SpawnAt;
            Time1 = DefaultTime;
            photonView.RPC("ButtonDropCoinClicked2", RpcTarget.Others, x);
        }
        else
        {
            RandomDrop();
        }

    }

    public void OpenMenu()
    {
        MenuP.SetActive(true);
        Menu.SetActive(false);
    }

    public void CloseMenu()
    {
        MenuP.SetActive(false);
        Menu.SetActive(true);
    }

    public void SoundControlMenu()
    {
        Debug.Log("Sound Control Menu Display");
    }

    public void CheckGiveUp()
    {
        GiveUpPanel.SetActive(true);
        MenuP.SetActive(false);
    }

    public void CloseGiveUpPanel()
    {
        GiveUpPanel.SetActive(false);
        MenuP.SetActive(true);
    }

    public void GiveUp()
    {

        bool[] x = new bool[1];
        if (myColor == MyColor.Red)
        {
            x[0] = true;
        }
        else
        {
            x[0] = false;
        }
        GiveUp1(x);
        photonView.RPC("GiveUp1", RpcTarget.Others, x);
    }

    [PunRPC]
    public void GiveUp1(bool[] SurrenderColorBool)
    {
        GiveUpPanel.SetActive(false);
        MyColor SurrenderColor;
        if (SurrenderColorBool[0])
        {
            SurrenderColor = MyColor.Red;
        }
        else
        {
            SurrenderColor = MyColor.Yellow;
        }
        Debug.Log(Turn + "  " + SurrenderColor);
        if (SurrenderColor == myColor)
        {
            StartCoroutine(Lose1(Turn));
        }
        else
        {
            StartCoroutine(Win1(Turn));
        }
    }

    public void Pause()
    {

        photonView.RPC("Pause1", RpcTarget.Others);
        HideGameComponents();
        MenuP.SetActive(false);
        Timer.gameObject.SetActive(false);
        PauseMenu.SetActive(true);
        //Time.timeScale = 0;

    }

    [PunRPC]
    public void Pause1()
    {

        HideGameComponents();
        MenuP.SetActive(false);
        Timer.gameObject.SetActive(false);
        PauseMenu.SetActive(true);
        //Time.timeScale = 0;

    }

    public void Resume()
    {
        photonView.RPC("Resume1", RpcTarget.Others);
        ShowGameComponents();
        MenuP.SetActive(true);
        Timer.gameObject.SetActive(true);
        PauseMenu.SetActive(false);
        //Time.timeScale = 1;

    }

    [PunRPC]
    public void Resume1()
    {
        Time.timeScale = 1;
        Debug.Log("Resume1");
        ShowGameComponents();
        MenuP.SetActive(true);
        Timer.gameObject.SetActive(true);
        PauseMenu.SetActive(false);
        //Time.timeScale = 1;


    }

    public void ReMatch()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("MatchingUp");
    }

    public void Exit()
    {
        EndConnection();
        SceneManager.LoadScene("Entery");
    }

    private IEnumerator Draw()
    {
        End = true;
        Timer.gameObject.SetActive(false);
        DisableArr(DropButtons);
        TurnText.gameObject.SetActive(false);
        yield return new WaitForSeconds(EndDelay);
        HideGameComponents();
        DrawP.SetActive(true);
    }

    private IEnumerator Win(bool Winner)
    {
        if (Winner && myColor == MyColor.Red || !Winner && myColor == MyColor.Yellow)
        {
            End = true;
            Timer.gameObject.SetActive(false);
            DisableArr(DropButtons);
            TurnText.gameObject.SetActive(false);
            yield return new WaitForSeconds(EndDelay);
            HideGameComponents();
            WinP.SetActive(true);
            EndConnection();
        }
        else
        { StartCoroutine(Lose(Turn)); }
    }

    private IEnumerator Lose(bool Winner)
    {
        if (Winner && myColor == MyColor.Yellow || !Winner && myColor == MyColor.Red)
        {
            End = true;
        Timer.gameObject.SetActive(false);
        DisableArr(DropButtons);
        TurnText.gameObject.SetActive(false);
        yield return new WaitForSeconds(EndDelay);
        HideGameComponents();
        LoseP.SetActive(true);
        EndConnection();
        }
        else
        { StartCoroutine(Win(Turn)); }
    }

    private IEnumerator Win1(bool Winner)
    {
  
            End = true;
            Timer.gameObject.SetActive(false);
            DisableArr(DropButtons);
            TurnText.gameObject.SetActive(false);
            yield return new WaitForSeconds(EndDelay);
            HideGameComponents();
            WinP.SetActive(true);
            EndConnection();
  
    }

    private IEnumerator Lose1(bool Winner)
    {
       
            End = true;
            Timer.gameObject.SetActive(false);
            DisableArr(DropButtons);
            TurnText.gameObject.SetActive(false);
            yield return new WaitForSeconds(EndDelay);
            HideGameComponents();
            LoseP.SetActive(true);
            EndConnection();
     
    }

    public void EndConnection()
    {
        PhotonNetwork.LeaveRoom(true);
        PhotonNetwork.Disconnect();
    }

    private void HideGameComponents()
    {
        foreach (Button i in DropButtons)
        {
            i.gameObject.SetActive(false);
        }

        foreach (Transform x in DropPoints)
        {
            x.gameObject.SetActive(false);
        }
        Menu.SetActive(false);
        MenuP.SetActive(false);
        TurnText.gameObject.SetActive(false);

        Board.SetActive(false);
    }

    private void ShowGameComponents()
    {
        foreach (Button i in DropButtons)
        {
            i.gameObject.SetActive(true);
        }

        foreach (Transform x in DropPoints)
        {
            x.gameObject.SetActive(true);
        }
        Menu.SetActive(true);
        MenuP.SetActive(true);
        TurnText.gameObject.SetActive(true);
        Board.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.Exception)
        {
            StartCoroutine(Lose(Turn));

        }
    }
}
