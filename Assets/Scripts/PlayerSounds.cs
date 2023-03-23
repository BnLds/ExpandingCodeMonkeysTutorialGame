using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private PlayerController player;
    private float footStepTimer;
    private float footStepTimerMax = .1f;


    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        player.OnPickedSomething += PlayerController_OnPickedSomething;
    }

    private void Update()
    {
        footStepTimer -= Time.deltaTime;
        if(footStepTimer < 0)
        {
            footStepTimer = footStepTimerMax;

            if(player.IsWalking())
            {
                float volume = 1f;
                SoundManager.Instance.PlayFoottepsSound(player.transform.position, volume);
            }
        }
    }

    private void PlayerController_OnPickedSomething(object sender, EventArgs e)
    {
        float volume = 1f;
        SoundManager.Instance.PlayPickedSomethingSound(player.transform.position, volume);
    }
}
