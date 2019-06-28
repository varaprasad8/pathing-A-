using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilebutons : MonoBehaviour
{
    [SerializeField]
    private TileType tileType;

    public TileType MyTileType { get => tileType; }
}
