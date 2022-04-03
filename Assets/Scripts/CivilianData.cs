using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "ScriptableObjects/CivilianData", order = 1)]
public class CivilianData : ScriptableObject
{
    public enum BodyType
    {
        guy1,
        guy2,
        guy3
    }

    [Header("Basic info")]
    public string civilianName;

    [Header("Characteristics")]
    public Sprite baseSprite; // TEMPORARY
    public Sprite shirt;
    public Sprite hat;
    public Sprite pants;
}
