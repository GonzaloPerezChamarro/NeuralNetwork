using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Clase del Warrior
public class Warrior : Unit
{
    //Animator del Warrior
    [SerializeField]
    private Animator animator;

    //Agente para el NavMesh
    private NavMeshAgent agent;

    //Destino al que se dirige el warrior
    public Vector3 destiny;

    //False: no tiene destino. True: tiene un destino.
    bool hasDestiny = false;
    
    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        
        agent.speed = speed;
        nearEnemiesUnits = new List<Unit>();
    }
    
    
    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        if(state == State.LOOKING)
        {
            agent.isStopped = false;

            animator.SetBool("Attack", false);
            
            //Ataca a la unidad mas cercana
            if(nearEnemiesUnits.Count > 0 && nearEnemiesUnits[0])
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(nearEnemiesUnits[0].gameObject.transform.position, out hit, 1, -1))
                {
                    agent.SetDestination(hit.position);
                }

                hasDestiny = false;

            }
            else
			{
                if (!hasDestiny)
                {
                    //Samplea si es posicion valida
                    destiny = ownPlayer.GetNextDestiny();
                    hasDestiny = true;
                }
                agent.SetDestination (destiny);
            }

        }
        else if(state == State.ATTACKING)
        {
            //Ataca a la unidad mas cercana
            animator.SetBool("Attack", true);
            agent.isStopped = true;
            if(nearEnemiesUnits.Count > 0 && nearEnemiesUnits[0])
                transform.LookAt(nearEnemiesUnits[0].gameObject.transform);
            else
                transform.LookAt(destiny);
            
            hasDestiny = false;
        }
        
    }

    //Devuelve el Agent
    public NavMeshAgent GetAgent(){ return agent; }

    //Se llama en el momento de muerte del Warrior
    protected override void Death()
    {
        agent.isStopped = true;
        base.Death();
    }

}
