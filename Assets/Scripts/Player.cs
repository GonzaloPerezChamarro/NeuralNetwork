using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Clase Player
public class Player : MonoBehaviour
{
    //Controlador de la escena
    [SerializeField]
    private GameController controller;

    //Equipo al que pertenece. 0 Azul - 1 Rojo
    [SerializeField]
    private byte team;

    //Oro que tiene el jugador
    protected int gold = 0;

    //Cuando oro recibe cada determinado tiempo
    [SerializeField] [Range(0, 500)]
    private int repeatingGold = 0;

    //Cada cuanto tiempo recibe dicho oro
    [SerializeField] [Range(0, 10)]
    private int repeatingTime = 0;

    //Texto en el que se muestra la cantidad de oro que posee
    [SerializeField]
    private Text goldText;

    //Prefab del Warrior
    [SerializeField]
    private GameObject warrior;

    //Prefab del avión
    [SerializeField]
    private GameObject plane;

    //Prefab de la torreta
    [SerializeField]
    private GameObject turret;

    //Lista de las torres
    [SerializeField]
    private List<Tower> towers;

    //Enemigo
    [SerializeField]
    protected Player enemy;

    //Esta en modo establecer torreta
    private bool setTurret;

    //Unidades spawneadas que posee
    protected List<Unit> playerUnits;

    //Propiedades de las unidades a spawnear
    [SerializeField]
    private Properties properties;

    //Oro infinito. False: no infinito. True: si infinito.
    public bool infiniteGold = false;

    //Unidades cualesquiera que tiene en su zona
    [SerializeField]
    protected List<Unit> unitsInZone;

    //Numero de unidades caidas desde la última decisión
    protected int fallenUnits;


    protected virtual void Start()
    {
        playerUnits = new List<Unit>();
        unitsInZone = new List<Unit>();

        //Establecimiento de la repetición del oro.
        InvokeRepeating("RepeatingGold", repeatingTime, repeatingTime);

        //Establecimiento de las torres
        foreach (Tower t in towers)
        {
            t.Generate(team, "Tower");
            t.ownPlayer = this;
        }

        setTurret = false;
    }

    protected virtual void Update()
    {
        //Revisión del estado de las torres
        ResizeTowersList();

        //Comprobación de las unidades
        CheckUnits();
        CheckUnitsInZone();

        //Revisión del estado de juego
        if (!CheckTowers())
        {
            FinishGame();
            return;
        }

        //Establecimiento de las torretas
        if (Input.GetMouseButtonDown(0) && setTurret)
        {

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("Base");
            if (Physics.Raycast(ray, out hit, 100.0f, layerMask))
            {
                Debug.DrawRay(hit.point, ray.direction, Color.red);

                setTurret = false;
                if (hit.transform.name == "Base" && hit.transform.tag == this.tag)
                {

                    if (!hit.transform.GetComponent<BaseTurret>().GetTaken())
                    {
                        SpawnTurret(hit.transform.GetComponent<BaseTurret>());
                    }
                }
            }
        }

        //Actualización del oro en el HUD
        goldText.text = gold.ToString();
    }

    //Spawneo del Warrior
    public void SpawnWarrior()
    {
        if (towers.Count > 0 && towers[0])
        {
            int goldWarrior = 0;

            for (byte i = 0; i < properties.units.Length; ++i)
            {
                if (properties.units[i].name == "Warrior")
                {
                    goldWarrior = properties.units[i].cost;
                    break;
                }
            }

            //Revisa qye se lo pueda permitir antes de generarlo
            if (CheckGold(goldWarrior))
            {
                GameObject warriorTemp = (GameObject)Instantiate(warrior, towers[0].GetSpawnPosition(), Quaternion.identity);
                warriorTemp.GetComponent<Warrior>().ownPlayer = this;
                warriorTemp.GetComponent<Warrior>().Generate(team, "Warrior");
                gold -= warriorTemp.GetComponent<Warrior>().GetCost();

                playerUnits.Add(warriorTemp.GetComponent<Unit>());
            }
        }

    }

    //Spawneo del Avión
    public void SpawnPlane()
    {
        if (towers.Count > 0 && towers[0])
        {

            int goldPlane = 0;

            for (byte i = 0; i < properties.units.Length; ++i)
            {
                if (properties.units[i].name == "Plane")
                {
                    goldPlane = properties.units[i].cost;
                    break;
                }
            }

            //Revisa que se lo pueda permitir antes de spawnearlo
            if (CheckGold(goldPlane))
            {
                GameObject planeTemp = Instantiate(plane, towers[0].GetSpawnPosition(), Quaternion.identity);
                planeTemp.GetComponent<Plane>().ownPlayer = this;
                planeTemp.GetComponent<Plane>().Generate(team, "Plane");

                gold -= planeTemp.GetComponent<Plane>().GetCost();
                playerUnits.Add(planeTemp.GetComponent<Unit>());

            }
        }

    }

    //Spawneo de la torreta en una determinada base
    public void SpawnTurret(BaseTurret _base)
    {
        if (towers.Count > 0 && towers[0])
        {
            int goldTurret = 0;

            for (byte i = 0; i < properties.units.Length; ++i)
            {
                if (properties.units[i].name == "Turret")
                {
                    goldTurret = properties.units[i].cost;
                    break;
                }
            }

            //Revisa si puede permitirselo antes de spawnearlo
            if (CheckGold(goldTurret))
            {
                _base.SetTaken(true);

                GameObject turretTemp = (GameObject)Instantiate(turret, _base.gameObject.transform.position, Quaternion.identity);
                turretTemp.GetComponent<Turret>().ownPlayer = this;
                turretTemp.GetComponent<Turret>().Generate(team, "Turret");
                turretTemp.GetComponent<Turret>().SetBase(_base.gameObject.transform.GetComponent<BaseTurret>());
                gold -= turretTemp.GetComponent<Turret>().GetCost();

                playerUnits.Add(turretTemp.GetComponent<Unit>());

            }
        }
    }

    //Modo creación de torretas
    public void CreateTurret(){ setTurret = !setTurret; }

    //Repetición de adición de Oro
    void RepeatingGold() { AddGold(repeatingGold); }

    //Añadir tanto oro como se le pase por parametro
    void AddGold(int amount){ gold += amount; }

    //Obtener equipo
    public byte GetTeam() { return team; }

    //Obtener siguiente destino
    public Vector3 GetNextDestiny()
    {
        if (towers.Count > 0 && towers[0] && controller.running)
        {
            Vector3 position = enemy.GetRandomTower().GetSpawnPosition();
            return position;
        }
        else
            return new Vector3();
    }

    //Obtener la Torre principal
    public Tower GetMainTower()
    {
        if (towers.Count > 0)
            return towers[0];

        return null;
    }

    //Re-escala la lista de Torres
    private void ResizeTowersList()
    {
        for(int i = towers.Count -1; i > -1;--i)
        {
            if (towers[i] == null) towers.RemoveAt(i);
        }
    }

    //Comprueba las unidades caidas
    private void CheckUnits()
    {
        for(int i = playerUnits.Count -1; i > -1;--i)
        {
            if(playerUnits[i] == null)
            {
                playerUnits.RemoveAt(i);
                fallenUnits++;
            }

                
        }
    }

    //Elimina todas las unidades. Se llama al final de la partida.
    public void RemoveUnits()
    {
        foreach(Unit u in playerUnits){ Destroy(u.gameObject); }
        playerUnits.Clear();
    }

    //Comprueba la cantidad de Torres restantes
    private bool CheckTowers()
    {
        return towers.Count > 0;
    }

    //Llama al final del juego
    private void FinishGame()
    {
        RemoveUnits();
        controller.End(this);
    }

    //Elimina a una determinada Torre
    public void RemoveTower(Tower tower){ towers.Remove(tower); }

    //Debuelve una Torre de manera aleatoria
    public Tower GetRandomTower(){ return towers[Random.Range(0, towers.Count)]; }

    //Devuelve True si tiene la cantidad de oro recibida.
    private bool CheckGold(int goldToCheck)
    {
        if (infiniteGold)
            return true;
        return goldToCheck <= gold;
    }

    //Añade las unidades a la lista de Unidades en Zona si se encuentran
    //en la zona del jugador.
    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.GetComponent<Unit>())
            {
                unitsInZone.Add(other.GetComponent<Unit>());
            }
        }
        
    }

    //Elimina de la lista a las unidades que salen de su campo
    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.GetComponent<Unit>())
            {
                if (unitsInZone.Contains(other.GetComponent<Unit>()))
                {
                    unitsInZone.Remove(other.GetComponent<Unit>());
                }
            }
        }
            
    }

    //Revisa si las unidades de la Zona siguen existiendo
    private void CheckUnitsInZone()
    {
        for(int i = unitsInZone.Count -1;i > -1; --i)
        {
            if (unitsInZone[i] == null)
                unitsInZone.RemoveAt(i);
        }
    }

    //Devuelve la lista de las unidades que hay en la zona
    public List<Unit> GetUnitsInZone(){ return unitsInZone; }

    //Devuelve la lista de las Torres que tiene el jugador
    public List<Tower> GetTowers() { return towers; }
}
