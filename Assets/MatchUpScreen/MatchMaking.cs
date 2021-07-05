using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchMaking : MonoBehaviourPunCallbacks
{
    string Room_name;
    float waitTime;
    int roomCountsAtStart;
    public Text Name;
    private void Start()
    {   
        //Name>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        Name.text = DataSaver.Name;
        if (Name.text == "")
        {
            Name.text = "Player" + Random.Range(100,999);
            DataSaver.Name = Name.text;
        }
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

        waitTime = Random.Range(1f,5f);
        //Room_name = "Room " + Random.Range(0, 99999);
        roomCountsAtStart = PhotonNetwork.CountOfRooms;
        Debug.Log("Rooms at start : " + roomCountsAtStart);
        PhotonNetwork.JoinRandomRoom();
    }

    public void SearchForRoom()
    {
        Debug.Log("Searching");
        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 1;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LeaveRoom(true);

    }

    public override void OnConnectedToMaster()
    {   
        Debug.Log("Left room and connected to server");
        Invoke("joinRoom",waitTime);
    }

    public void joinRoom()
    {
        if(!PhotonNetwork.InRoom)
            PhotonNetwork.JoinRandomRoom();
        else {
            Invoke("joinRoom", waitTime);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joining");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("FirstDay 1");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed join Rooms count = " + PhotonNetwork.CountOfRooms);
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
        Invoke("SearchForRoom", waitTime);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Players number = " + PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("FirstDay 1");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnect");
        SceneManager.LoadScene("Entery");
    }

    public void ReturnButtonClicked()
    {
        Debug.Log("Return button has been clicked");
        PhotonNetwork.Disconnect();
    }
    
}
