using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase de la base de la Torreta
public class BaseTurret : MonoBehaviour
{
    //True: Base tomada. False: Base no tomada
    private bool taken;

    //Get Taken
    public bool GetTaken(){ return taken; }

    //Establece si se ha tomado o no
    public void SetTaken(bool _taken){ taken = _taken; }
}
