using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Clase Unidad base a partir de la que heredan todas la Unidades spawneables
[RequireComponent(typeof(SphereCollider))]
public class Unit : MonoBehaviour
{
    //Propiedades base de cualquier unidad
    [SerializeField]
    private Properties properties = null;

    //0: Azul - 1: Rojo
    public int team;

    //Nombre de la unidad
    public string unitName;

    //Salud
    public int health;

    //Daño que hace
    protected int damage;

    //Coste de la unidad
    protected int cost;

    //Oro que genera al morir
    protected int gold;

    //Velocidad de la undidad
    protected int speed;

    //Ratio de ataque
    protected float attackRatio;

    //Tiempo cada vez que ataca
    protected float attackTime;

    //Rango de visión
    private float visionRatio = 5f;

    //Posición orientativa para recibir ataques
    [SerializeField]
    private GameObject position;
    
    //Trigger de vision
    protected SphereCollider attackTrigger;

    //Lista de unidades cercanas enemigas
    public List<Unit> nearEnemiesUnits;

    //Cantidad de unidades enemigas cercanas
    protected byte nearEnemiesCount;

    //Tiempo actual para el tiempo entre ataque y ataque
    private float currentTime;

    //False: No está atacando. True: está atacando.
    protected bool isAttacking;

    //Jugador al que pertenece
    [HideInInspector]
    public Player ownPlayer = null;//No cambiar a privado

    //Barra de salud
    [SerializeField]
    private Slider healthSlider;

    //Array de unidades a las que puede atacar
    protected bool[] canAttack;

    //False: no está muerto. True: está muerto.
    protected bool isDead;

    //Estado de la unidad. Looking: en movimiento. Attacking: atacando.
    protected enum State
    {
        LOOKING,
        ATTACKING
    }

    //Estado de la unidad
    protected State state;

    //Establecimiento de las bases de la Unidad
    private void Awake()
    {
        attackTrigger = GetComponent<SphereCollider>();
        attackTrigger.isTrigger = true;
        nearEnemiesCount = 0;
        nearEnemiesUnits = new List<Unit>();
        state = State.LOOKING;
        canAttack = new bool[4];
        isDead = false;
    }

    protected virtual void Start(){}

    protected virtual void Update()
    {
        if (isDead)
            Death();
        else
        {
            //Comprueba las unidades cercanas
            CheckNearUnits();
            if (nearEnemiesUnits.Count > 0 && nearEnemiesUnits[0])
            {

                Transform tr = nearEnemiesUnits[0].transform;

                //En base a la distancia con el enemigo cercano, lo ataca o lo envia hacia a él
                if (CalculateDistance(new Vector2(tr.position.x, tr.position.z),
                    new Vector2(transform.position.x, transform.position.z)) <= attackRatio)
                {

                    
                    currentTime += Time.deltaTime;
                    state = State.ATTACKING;

                    if (currentTime >= attackTime)
                    {
                        currentTime = 0f;
                        Attack(nearEnemiesUnits[0]);

                    }
                }
                else
                {
                    state = State.LOOKING;
                    currentTime = 0f;
                }
            }
            else
            {
                state = State.LOOKING;
                currentTime = 0f;
            }

            //Actualiza la barra de vida
            UpdateSlider();
        }

    }

    //Destruye el objeto
    protected virtual void Death(){ Destroy(gameObject); }

    //Devuelve la posicion del punto orientativo de posicion
    public Vector3 GetPosition() { return position.transform.position; }

    //Establece las propiedades de cada Unidad
    public void Generate(  int _team, string _name)
    {
        team = _team;
        unitName = _name;

        for (byte i = 0; i < properties.units.Length; ++i)
        {
            if(properties.units[i].name == _name)
            {
                health  = properties.units[i].health;
                damage  = properties.units[i].damage;
                cost    = properties.units[i].cost;
                
                gold    = properties.units[i].gold;
                speed   = properties.units[i].speed;
                attackRatio = properties.units[i].attackRatio;
              
                attackTime = properties.units[i].attackTime;

                canAttack[0] = properties.units[i].whatCanAttack.warrior;
                canAttack[1] = properties.units[i].whatCanAttack.plane;
                canAttack[2] = properties.units[i].whatCanAttack.turret;
                canAttack[3] = properties.units[i].whatCanAttack.tower;
                
                break;
            }
        }
        
        attackTrigger.radius = visionRatio;

        healthSlider.maxValue = health;
        healthSlider.minValue = 0;
        healthSlider.value = healthSlider.maxValue;
    }

    //Ataque a la unidad que se le pasa como parametro
    protected virtual void Attack(Unit unit) {

        if (!unit) return;

        unit.ReceiveDamage(damage);

        if (unit.health <= 0)
        {
            nearEnemiesUnits.Remove(unit);
            --nearEnemiesCount;
        }
    }

    //Recibe el daño que recibe como parametro
    public void ReceiveDamage(int damage)
    {
        health -= damage;
        if (health < 0)
        {
            health = 0;
            isDead = true;
        }
    }

    //Añade a la lista de enemigos cercanos a aquellas unidades que entren en su campo de visión
    //que sean del equipo contrario.
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {   
            if(other.GetComponent<Unit>() != null && CanAttackIt(other.GetComponent<Unit>()))
            {
                if (other.tag != gameObject.tag && (other.tag == "Red" || other.tag == "Blue"))
                {
                    ++nearEnemiesCount;
                    nearEnemiesUnits.Add(other.GetComponent<Unit>());
                    
                }
            }

        }
    }

    //Elimina de la lista a aquellas unidades que salen de su campo de vision 
    protected void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.GetComponent<Unit>() == null ||
            (other.GetComponent<Unit>() != null && !CanAttackIt(other.GetComponent<Unit>())))
            return;

        if (other.tag != gameObject.tag)
        {
            --nearEnemiesCount;
            nearEnemiesUnits.Remove(other.GetComponent<Unit>());
        }

        if(nearEnemiesCount == 0)
            currentTime = 0f;

    }

    //Comprueba si pueda atacar a la unidad que recibe como parametro
    private bool CanAttackIt(Unit unit)
    {
        if (unit.GetComponent<Warrior>() != null && canAttack[0])
            return true;

        else if (unit.GetComponent<Plane>() != null && canAttack[1])
            return true;
        else if (unit.GetComponent<Turret>() != null && canAttack[2])
            return true;
        else if (unit.GetComponent<Tower>() != null && canAttack[3])
            return true;

        else
            return false;
    }

    //Calcula la distancia entre el punto a y el punto b
    protected float CalculateDistance(Vector2 a, Vector2 b){ return Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y)); }

    //Devuelve el coste de la unidad
    public int GetCost(){ return cost; }

    //Actualiza la barra de vida
    protected void UpdateSlider() { healthSlider.value = health; }

    //Comprobar las unidades cercanas
    private void CheckNearUnits()
    {
        for(int i = 0; i < nearEnemiesUnits.Count; ++i)
        {
            if (!nearEnemiesUnits[i])
            {
                nearEnemiesUnits.Remove(nearEnemiesUnits[i]);
            }
        }
    }
}