using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase Plane del Avión
public class Plane : Unit
{
    //Posición destino del Avión
    public Vector3 destiny;

    //Dirección del Avión
    public Vector3 direction;

    //False: no tiene destino. True: tiene un destino.
    private bool hasDestiny = false;
    
    //Establece propiedades del avión
    protected override void Start()
    {
        base.Start();
        hasDestiny = false;
        attackTrigger.radius = attackTrigger.radius * 1.5f;

    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;
        
        if(state == State.LOOKING)
            MoveToTarget();
        else
            hasDestiny = false;
        
    }

    //Envia al avión a su destino
    private void MoveToTarget()
    {
        //Si la cantidad de enemigos cercanos es superior a 0...
        if (nearEnemiesUnits.Count > 0 && nearEnemiesUnits[0])
        {
            //Envia al avión al enemigo más cercano
            destiny = nearEnemiesUnits[0].transform.position;
            direction = new Vector3(destiny.x, 0f, destiny.z)
                        - new Vector3(transform.position.x, 0f, transform.position.z);
            direction.Normalize();

            hasDestiny = false;
        }
        else
        {
            //Calcula la distancia a la torre
            if(CalculateDistance(new Vector2(transform.position.x, transform.position.z), new Vector2(destiny.x, destiny.z)) < 0.1f)
            {
                hasDestiny = false;
            }

            //Si no tiene destino...
            if (!hasDestiny)
            { 

                hasDestiny = true;
                //Pide al jugador un nuevo destino
                destiny = ownPlayer.GetNextDestiny();
                direction = new Vector3(destiny.x, 0f, destiny.z)
                            - new Vector3(transform.position.x,0f, transform.position.z);
                direction.Normalize();
            }
        }
        
        //Movimiento del avión
        transform.position = transform.position + direction * speed * Time.deltaTime;
        LookAtTarget(destiny);
    }

    //Mira hacia el lugar de destino
    private void LookAtTarget(Vector3 target)
    {
        Vector3 vector = target - transform.position;
        vector.y = 0;
        Quaternion rotation = Quaternion.LookRotation(vector);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 50f);
    }

    //Si ha muerto...
    protected override void Death(){ base.Death(); }

}
