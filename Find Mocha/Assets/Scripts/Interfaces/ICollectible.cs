using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectible
{
    void Collect(PlayerController script); // script is most likely to be the player
}
