using UnityEngine;
using Photon.Pun;
public class MultiPlayerManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Number of players " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
