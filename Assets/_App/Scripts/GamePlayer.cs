using GameBrewStudios;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GamePlayer
{
    public Photon.Realtime.Player player;
    
    public string username;

    //Used to indicate the players board piece position on the board.
    public PathNode currentNode;

    /// <summary>
    /// Each int represents the id of a card from any deck.
    /// </summary>
    public List<int> drawnCards = new List<int>();


    //This can get added to or subtracted from. If it is in the positive when the player finishes a turn, they immediately begin the next turn. If it is in the negative when they begin a turn, it skips their turn.
    public int turnBank = 0;

    /// <summary>
    /// Increments at the beginning of every turn. Still increments even if turn is "skipped".
    /// </summary>
    public int turnsTaken = 0;

    /// <summary>
    /// Incremented at the beginning of a turn that is skipped.
    /// </summary>
    public int turnsSkipped = 0;


    //Various values to calculate stats for time values
    public int backToSingleCount = 0;
    public int divorceCount = 0; //aka back to single count from the marriage ring.

    public int datingCount = 0;
    public int spacesMovedInDating = 0;

    public int relationshipCount = 0;
    public int spacesMovedInRelationship = 0;

    public int marriageCount = 0;
    public int spacesMovedInMarriage = 0;

}
