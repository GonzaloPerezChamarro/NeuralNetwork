using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase de Torreta
public class Turret : Unit
{
    //Parte superior de la Torreta
    [SerializeField]
    private GameObject top;

    //Posición de spawneo de la Bala
    [SerializeField]
    private Transform spawnBullet;

    //Prefab de la Bala
    [SerializeField]
    private GameObject bullet;

    //Base a la que pertenece la Torreta
    [SerializeField]
    private BaseTurret baseTurret;

    //Establecimiento de las propiedades básicas de la torreta
    protected override void Start()
    {
        base.Start();
        nearEnemiesUnits = new List<Unit>();
        attackTrigger.radius = attackTrigger.radius * 1.5f;
    }


    protected override void Update()
    {
        base.Update();
        
        if(state == State.LOOKING)
        {
            //nothing
        }

        else if(state == State.ATTACKING)
        {
            //Si hay un enemigo en su zona, se gira hacia él
            if(nearEnemiesUnits.Count > 0 && nearEnemiesUnits[0])
            {
                Vector3 dir = nearEnemiesUnits[0].GetPosition() - transform.position;
                dir.y = 0;
                top.transform.rotation = Quaternion.LookRotation(dir);
            }

        }
    }

    //Dispara hacia la unidad que recibe
    protected override void Attack(Unit unit)
    {
        GameObject tempBullet = (GameObject)Instantiate(bullet, spawnBullet.position, Quaternion.identity);
        tempBullet.GetComponent<Bullet>().team = team;
        Vector3 dir = ((unit.GetPosition() + new Vector3(0, 1f, 0)) - spawnBullet.position).normalized;
        tempBullet.GetComponent<Bullet>().AddForce(dir);
        tempBullet.GetComponent<Bullet>().SetDamage(damage);
        tempBullet.GetComponent<Bullet>().SetTurret(this);
    }

    //Elimina a la unidad de sus enemigos cercanos
    public void DeleteUnit(Unit unit)
    {
        nearEnemiesUnits.Remove(unit);
        --nearEnemiesCount;
    }

    //Establece la base a la que pertenece
    public void SetBase(BaseTurret _baseTurret){ baseTurret = _baseTurret; }

    //Se llama cuando la Torreta muere
    protected override void Death()
    {
        baseTurret.SetTaken(false);
        isDead = true;
        base.Death();
    }

}
