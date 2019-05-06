 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase de la Torre
public class Tower : Unit
{
    //Posición de spawneo de las unidades de su equipo
    [SerializeField]
    private GameObject spawnPosition;
   
    protected override void Start(){ base.Start(); }

    protected override void Update() { base.Update(); }

    //Devuelve la posición de Spawneo de la Torre
    public Vector3 GetSpawnPosition() { return spawnPosition.transform.position; }

    //Se llama en el momento de muerte de la Torre
    protected override void Death() { base.Death(); }
}
