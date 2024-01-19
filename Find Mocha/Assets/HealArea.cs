using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealArea : MonoBehaviour
{
    [SerializeField]
    private float hpPerTick;
    [SerializeField]
    private float startTickRate;
    [SerializeField]
    private float tickRateAcceleration;
    [SerializeField]
    private bool isContinous;


    private bool isPlayerInZone;
    private float _timer;
    private float tickRate;

    private float layer1;
    private float layer2;

    private PlayerController playerController;


    private void Start()
    {
        playerController = PlayerController.Instance;
        _timer = 0;
    }

    private void Update()
    {
        if (!isPlayerInZone)
            return;

        if (isContinous)
        {
            HealPlayer(hpPerTick * tickRate * Time.deltaTime);
        }
        else
        {
            if(_timer > 1/tickRate)
            {
                _timer = 0;
                HealPlayer(hpPerTick);
            }

            _timer += Time.deltaTime;
        }

        layer1 += tickRateAcceleration * Time.deltaTime;
        layer2 += layer1 * Time.deltaTime;
        tickRate += layer2 * Time.deltaTime;
    }

    private void HealPlayer(float heal)
    {
        playerController.Heal(heal, true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController _))
        {
            isPlayerInZone = true;
            tickRate = startTickRate;
            layer1 = 0;
            layer2 = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController _))
        {
            isPlayerInZone = false;
        }
    }
}
