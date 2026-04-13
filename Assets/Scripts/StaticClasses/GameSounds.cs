using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSounds : MonoBehaviour
{
    public static GameSounds Instance;

    public AudioClip GibsExplosionSound;
    public AudioClip BulletImpactSound;
    public AudioClip DecapitationSound;
    public AudioClip BladeImpactSound;
    public AudioClip BluntImpactSound;

    void Awake()
    {
        Instance = this;
    }
}