﻿using System;
//using System.box.Collections;
using System.Collections.Generic;


namespace tsuro
{
    interface IAdmin
    {
        // takes in a SPlayer, a Board, and a Tile t that will be played
        // returns true if the tile that is going to be played is in player's hand
        // AND if placing the tile is not a move that will eliminate the player
        bool legalPlay(SPlayer p, Board b, Tile t);

        // takes in a List of Tiles of the drawpile, a list of SPlayers for the inGamePlayers
        // a list of SPlayers that are eliminated, a Board, and a Tile t that will be placed during that turn
        // for the first SPlayer in inGamePlayers
        TurnResult playATurn(List<Tile> pile, List<SPlayer> inGamePlayers,List<SPlayer> eliminatedPlayers,
            Board b, Tile t);
    }
    public class Admin:IAdmin
    {
        //private List<Tile> drawPile = new List<Tile>();

        

        public bool tileInHand(SPlayer p, Tile t)
        {
            List<Tile> hand = p.returnHand();
            
            // check if Tile t is in the hand of the player
            if (hand == null) //if there are no tiles in the players hand
            {
                return false;
            }
            else // if there are tiles in the players hand
            {
                // check all of the tiles in the players hand against t
                foreach (Tile hTile in hand)
                {
                    if (hTile.isEqual(t))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool legalPlay(SPlayer p, Board b, Tile t)
        {
            return (tileInHand(p, t) && b.checkPlaceTile(p,t));
        }

        

        public TurnResult playATurn(List<Tile> pile, List<SPlayer> inGamePlayers, List<SPlayer> eliminatedPlayers,
            Board b, Tile t)
        {
            // make the drawpile be the pile passed in from playATurn
            b.drawPile = pile;
            //if there are no players in the game
            if(inGamePlayers.Count == 0)
            {
                // return TurnResult with the same drawpile, same list of Players in game,
                // same list of Players eliminated, same board, and null for list of players who are winners
                TurnResult tr = new TurnResult(pile, inGamePlayers, eliminatedPlayers, b, null);
                return tr;
            }

            // set the temp player to be the first player in inGamePlayers
            SPlayer tempPlayer = inGamePlayers[0];
            
            // check if placing tile t by tempPlayer is a valid move
            // (does not lead player back to an edge)
            bool playWasLegal = b.checkPlaceTile(tempPlayer, t);

            // if placing tile is a valid move
            if (playWasLegal)
            {
                // return a new player that has placed the tile and moved to the new grid location and tile posn
                SPlayer currentPlayer = b.placeTile(tempPlayer, t);

      

                //remove old player from the list of inGamePlayers 
                inGamePlayers.Remove(tempPlayer);

                // loop through rest of inGamePlayers, move players to new locations if tile placed effects them
                // if they go to edge, eliminate them
                int numPlayers = inGamePlayers.Count;
                List<SPlayer> toBeEliminated = new List<SPlayer>();

                for (int i = 0; i < numPlayers; i++)
                {
                    inGamePlayers[i] = b.movePlayer(inGamePlayers[i]);
                    if (b.onEdge(inGamePlayers[i]))
                    {
                        toBeEliminated.Add(inGamePlayers[i]);
                        //b.eliminatePlayer(inGamePlayers[i]);
                        //eliminatedPlayers.Add(inGamePlayers[i]);
                        //inGamePlayers.Remove(inGamePlayers[i]);
                    }
                }
                foreach (SPlayer p in toBeEliminated)
                {
                    b.eliminatePlayer(p);
                    //eliminatedPlayers.Add(p);
                    //inGamePlayers.Remove(p);
                }

                //add the player who played their turn at the end of the list of inGamePlayers
                inGamePlayers.Add(currentPlayer);

                // draw the first tile from the drawpile
                //Tile drawnTile = b.drawATile();

                // if the drawpile was not empty, add this tile to players hand
                if (b.drawPile.Count != 0)
                {
                    if(b.returnDragonTileHolder() == null)//there was no dragontile holder and the pile was not empty
                    {
                        currentPlayer.addTileToHand(b.drawATile());
                    }
                    else
                    {
                        if(b.returnDragonTileHolder().returnHand().Count < 3)
                        {
                            SPlayer dragonTileHolder = b.returnDragonTileHolder();
                            int dragonTileHolderIndex = inGamePlayers.FindIndex(x => 
                                x.returnColor() == dragonTileHolder.returnColor());
                            for (int i = dragonTileHolderIndex; i < dragonTileHolderIndex + inGamePlayers.Count; i++)
                            {
                                int correctedIndex = (i + inGamePlayers.Count) % inGamePlayers.Count;

                                if ((b.drawPile.Count != 0) && (inGamePlayers[correctedIndex].returnHand().Count <3))
                                {
                                    inGamePlayers[correctedIndex].addTileToHand(b.drawATile());
                                }
                            }
                        }
                        //dragon tile holder needs to "put" tile aside
                        b.setDragonTileHolder(null);
                    }
                }
                else
                {
                    if (b.returnDragonTileHolder() == null)//means there is no dragontile holder
                    {
                        b.setDragonTileHolder(currentPlayer);
                    }
                }

                TurnResult tr = new TurnResult(pile, inGamePlayers, eliminatedPlayers, b, null);
                return tr;
            }
            else // if placing tile was not a valid move for the player
            {
                // if the player does not have any other tiles in their hand, place the tile that will 
                // eliminate them
                if (tempPlayer.returnHand().Count == 0)
                {
                    SPlayer currentPlayer = b.placeTile(tempPlayer, t);
                    inGamePlayers.Remove(tempPlayer); // remove player from inGamePlayers
                    eliminatedPlayers.Add(currentPlayer); //add player to eliminatedPlayers
                    // return TurnResult with new list for inGamePlayers and eliminatedPlayers
                    if ((b.returnDragonTileHolder() != null) && (b.returnDragonTileHolder().returnColor() == currentPlayer.returnColor()))
                    {
                        b.setDragonTileHolder(null);
                    }
                    TurnResult tr = new TurnResult(pile, inGamePlayers, eliminatedPlayers, b, null);
                    return tr;
                }
            }
            return null;
        }
    }
}
