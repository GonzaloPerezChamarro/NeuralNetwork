using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase de la bala de las torretas
public class Bullet : MonoBehaviour
{
    //Rigidbody
    private Rigidbody rb;

    //Daño que hace la bala
    private int damage;

    //Torreta que ha spawneado la bala
    private Turret turret;

    //Equipo al que pertenece
    public int team;

    private void Awake(){ rb = this.GetComponent<Rigidbody>(); }


    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        //Si triggea con una unidad del equipo contrario...
        if (other.gameObject.GetComponent<Unit>() && other.GetComponent<Unit>().team != team)
        {
            //Hace daño a dicha unidad
            Unit tempUnit = other.GetComponent<Unit>();
            tempUnit.ReceiveDamage(damage);
            if (tempUnit.health <= 0)
                turret.DeleteUnit(tempUnit);

            //Destruye la bala
            Destroy(this.gameObject);
        }

    }

    //Da fuerza a la bala
    public void AddForce(Vector3 dir){ rb.AddForce(dir * 30f, ForceMode.Impulse); }

    //Establece el daño que realiza la bala
    public void SetDamage(int _damage){ damage = _damage; }

    //Establece la torreta a la que pertenece
    public void SetTurret(Turret _turret){ turret = _turret; }
}
