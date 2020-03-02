using System;
using UnityEngine;

[Serializable]
public class MapData
{
    [Header("Globals Properties")]
    [Range(1, 50)]
    public int width;

    [Range(1, 50)]
    public int height;

    public SquareData[] grid;

    public bool[] edgesHori;

    public bool[] edgesVert;

}

public enum SquareState
{
    Normal,
    Lock,
    Water,
    Grass,
    Special
}

public enum Alignment
{
    Neutral,
    IA,
    Player
}

[Serializable]
public struct SquareData
{
    public SquareState state;

    public Alignment aligment;
}