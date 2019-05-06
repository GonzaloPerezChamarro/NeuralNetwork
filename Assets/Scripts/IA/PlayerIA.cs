using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerIA : Player
{
    //Input de la IA
    private float[] input;

    //Bases de las torretas de IA
    [SerializeField]
    private BaseTurret[] bases;

    //Estructura de una toma de decision
    public struct Decision
    {
        public float warrior;
        public float plane;
        public float turret;
        public float wait;
        public float boardState;
    }

    //Lista de las decisiones tomadas
    private List<Decision> decisions;

    //Define cada cuanto tiempo se reentrena la red
    [SerializeField] [Range(0f, 20f)]
    private float timeToRetrain = 5f;


    protected override void Start()
    {
        base.Start();
        decisions = new List<Decision>();
        input = new float[3];

        //Define cada cuanto toma una decision
        InvokeRepeating("TakeDecision", 0.5f, 0.5f);

        //Reentrena la red
        InvokeRepeating("RetrainNeuralNetwork", timeToRetrain, timeToRetrain);
    }

    protected override void Update(){ base.Update(); }

    //Metodo para la toma de decisiones de la Red
    void TakeDecision()
    {
        //Borra la consola
        //ClearConsole();
        
        //Evaluación del otro campo
        input[0] = EvaluateOtherField(enemy.GetUnitsInZone());

        //Evaluación de tu propio campo
        input[1] = EvaluateOwnField(GetUnitsInZone(), this.GetTeam());

        //Oro que posees
        input[2] = NeuronalGold();

        double[] outPut = RoyaleNeuralNetwork.instance.CheckAction(input);
        AddDecision((float)outPut[0], (float)outPut[1], (float)outPut[2], (float)outPut[3]);


        switch(GetHighIndex(outPut))
        {
            case 0:
                SpawnWarrior();                     //Spawnea un Warrior
                break;
            case 1:
                SpawnPlane();                       //Spawnea un Avion
                break;
            case 2:
                SpawnTurret(RandomBaseTurret());    //Spawnea una torreta
                break;
            case 3:
                break;                              //Espera
            default:
                break;                              //Espera

        }
    }

    //Devuvel la posicion con el valor mas alto
    private int GetHighIndex(double [] values)
    {
        int index = 0;
        double max = -1;

        for(int i = 0;i<values.Length;++i)
        {
            if (max <= values[i])
            {
                index = i;
                max = values[i]; 
            }
        }

        return index;
    }

    //Devuelve el oro en un formato especial para la Red Neuronal
    float NeuronalGold()
    {
        if (gold < 25) return 0f;
        else if (gold < 75) return 0.3f;
        else if (gold < 100) return 0.6f;
        else return 0.9f;
    }

    //Evalua el propio campo
    float EvaluateOwnField(List<Unit> _units, int _team)
    {
        float ev = 0.5f;
        int ownWarriors = 0, otherWarriors = 0;
        int ownPlanes = 0, otherPlanes = 0;
        int ownTurret = 0;

        if (_units.Count == 0)
            return 0.5f;

        //Conteo de unidades
        for(int i = 0; i< _units.Count;++i)
        {
            if (_units[i] != null && _units[i].GetComponent<Unit>() != null)
            {
                if (_units[i].GetComponent<Warrior>() != null)
                {
                    if (_units[i].team == _team)
                        ++ownWarriors;
                    else
                        ++otherWarriors;

                }
                else if (_units[i].GetComponent<Plane>() != null)
                {
                    if (_units[i].team == _team)
                        ++ownPlanes;
                    else
                        ++otherPlanes;
                }
                else if (_units[i].GetComponent<Turret>() != null)
                {
                    ++ownTurret;
                }
            }
        }

        if(otherWarriors > 0)
        {
            if (ownWarriors >= otherWarriors)
                ev = 1f;
            else if (ownPlanes * 1.2 + ownWarriors > otherWarriors)
                ev = 1f;
            else if (ownTurret * 2.5 + ownPlanes + ownWarriors >= otherWarriors)
                ev = 1f;
            else
                ev = 0;
        }

        if(otherPlanes > 0)
        {
            if (ownTurret * 2.5f > otherPlanes)
            { 
                if (ev == 0) ev = 0.5f;
                else if (ev == 1) ev = 1f;
                else if (ev == 0.5f) ev = 1f;
            }
            else
            {
                ev = 0f;
            }
        }

        if(otherPlanes == 0 && otherPlanes == 0)
        {
            if (ownTurret > 0 || ownPlanes > 0 || ownWarriors > 0) ev = 1f;
        }



        return ev;
    }

    //Evalua el campo contrario
    float EvaluateOtherField(List<Unit> _units)
    {
        float ev = EvaluateOwnField(_units, this.GetTeam() == 0 ? 1:0);

        if (ev == 1)
            return 0f;
        else if (ev == 0.5f)
            return ev;
        else if (ev == 0f)
            return 1f;

        return 0f;
    }

    //Evalua las unidades en un determinado campo
    float EvaluateUnits(List<Unit> _units)
    {
        float sideEvaluation = 0f;
        if (_units.Count == 0)
            return 0.5f;

        for(int i = 0; i < _units.Count; ++i)
        {
            if(_units[i] != null)
            {
                if (_units[i].name == "Warrior")
                    sideEvaluation += OnMySide(_units[i]);
                else if (_units[i].name == "Plane")

                    sideEvaluation += OnMySide(_units[i]) * 3;
                else
                    sideEvaluation += OnMySide(_units[i]) * 5;
            }
            else
            {
                _units.Remove(_units[i]);
            }
        }

        if(sideEvaluation > 0)
        {
            return 1.0f;
        }
        else if(sideEvaluation < 0)
        {
            return 0.0f;
        }

        return 0.5f;
    }

    //Comprueba en que campo se encuentra una unidad
    float OnMySide(Unit _unit){ return _unit.team == 1 ? 1.0f : -1.0f; }

    //Borra los datos de la Consola
    void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    //Devuelve una base para Torretas aleatoria
    public BaseTurret RandomBaseTurret(){
        int rnd = 0;
        int index = 0;
        do
        {
            rnd = Random.Range(0, bases.Length);
            index++;
        } while (bases[rnd].GetTaken() && index < 4);
        return bases[rnd];
    }

    //Añade una nueva decision a la lista con los datos de input y el estado del tablero
    void AddDecision(float _warrior, float _plane, float _turret, float _wait)
    {
        Decision newDecision = new Decision();
        newDecision.warrior = _warrior;
        newDecision.plane = _plane;
        newDecision.turret = _turret;
        newDecision.wait = _wait;
        newDecision.boardState = EvaluateBoard(GetTowers(), enemy.GetTowers(), gold, fallenUnits);
        fallenUnits = 0;
        decisions.Add(newDecision);
        
    }

    //Re-entrena la red neuronal
    void RetrainNeuralNetwork()
    {
       
        if (decisions.Count < 2) return;

        if (decisions[0].boardState < decisions[decisions.Count - 1].boardState)
        {
            //Si ha ido a mejor...
            float lastWorstBoard = 300000f;
            int idWorstBoard = 0;
            for (int i = 0; i < decisions.Count; i++)
            {
                //Busqueda del ultimo peor tablero
                if(lastWorstBoard > decisions[i].boardState)
                {
                    lastWorstBoard = decisions[i].boardState;
                    idWorstBoard = i;
                }

            }

            float[] output = {  decisions[idWorstBoard].warrior,
                                decisions[idWorstBoard].plane,
                                decisions[idWorstBoard].turret,
                                decisions[idWorstBoard].wait };

            RoyaleNeuralNetwork.instance.RetrainNetwork(input, output);
            
        }
        else
        {
            //Si ha ido a peor...
            float lastBestBoard = -300000f;
            int idBestBoard = 0;

            //Busqueda del ultimo mejor tablero
            for (int i = 0; i < decisions.Count; i++)
            {
                if(lastBestBoard < decisions[i].boardState)
                {
                    lastBestBoard = decisions[i].boardState;
                    idBestBoard = i;
                }

            }

            //Busqueda del peor tablero antes del ultimo mejor tablero
            float lastWorstBoard = 300000f;
            int idWorstBoard = 0;
            for (int i = 0; i < idBestBoard; i++)
            {
                if (lastWorstBoard > decisions[i].boardState)
                {
                    lastWorstBoard = decisions[i].boardState;
                    idWorstBoard = i;
                }
            }

            float[] output = {  decisions[idWorstBoard].warrior,
                                decisions[idWorstBoard].plane,
                                decisions[idWorstBoard].turret,
                                decisions[idWorstBoard].wait };
            RoyaleNeuralNetwork.instance.RetrainNetwork(input, output);

        }

        decisions.Clear();
    }
    
    //Evaluación del estado del tablero en un determinado input
    public float EvaluateBoard(List<Tower> ownTowers, List<Tower> enemyTowers, int ownGold, int _fallenUnits)
    {
        int ownHealth = 0, enemyHealth = 0;

        for(int i = 0; i < ownTowers.Count; i++){ ownHealth += ownTowers[i].health; }
        for (int i = 0; i < enemyTowers.Count; i++) { enemyHealth += enemyTowers[i].health; }

        return (ownHealth - enemyHealth) * 1.5f + ownGold - _fallenUnits;

    }
}
