using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private void Awake()
    {
        instance = this;
    }
    public AudioSource AudioSource;
    public AudioClip ShootAudioClip;
    public AudioClip HitAudioClip;

    public float PlayHitAudioINterval = 0.2f;//时间间隔
    private float lastPlayerHitAudioTime;
    public float PlayHitAudioValidTime = 0.1f;//0.1秒内有重复的吞掉
    public void PlayShootAudio()
    {
        AudioSource.PlayOneShot(ShootAudioClip);
    }
    public void PlayHitAudio()
    {
        AudioSource.PlayOneShot(HitAudioClip);
    }
    private void Update()
    {
        if(SharedData.GameSharedData.Data.PlayHitAudio && Time.time-lastPlayerHitAudioTime>PlayHitAudioINterval && Time.time-SharedData.GameSharedData.Data.PlayHitAudioTime<Time.deltaTime)
        {
            lastPlayerHitAudioTime = Time.time;
            PlayHitAudio();
            SharedData.GameSharedData.Data.PlayHitAudio = false;
        }
    }
}
