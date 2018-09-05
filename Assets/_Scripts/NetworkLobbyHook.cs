using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class NetworkLobbyHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        //Default code below:
        //base.OnLobbyServerSceneLoadedForPlayer(manager, lobbyPlayer, gamePlayer);

        //My override code
        LobbyPlayer thisLobbyPlayer = lobbyPlayer.GetComponent<LobbyPlayer>();
        Player thisPlayer = gamePlayer.GetComponent<Player>();

        thisPlayer.MyName = thisLobbyPlayer.playerName;
    }

}
