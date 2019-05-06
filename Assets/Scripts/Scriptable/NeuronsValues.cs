using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase que genera los SerializeObject de las propiedades de Entrenamiento
//para la Red Neuronal. Se definen los tres inputs y los 4 outputs.
[CreateAssetMenu(fileName = "NeuronsValues", menuName = "Scriptable/NeuronsValues", order = 2)]
public class NeuronsValues : ScriptableObject, ISerializationCallbackReceiver
{
    public int amount = 10;
    private int oldAmount = 10;

    public bool saved = true;

    public Training[] training;

    public void OnAfterDeserialize()
    {
        if(!saved && oldAmount != amount)
        {
            oldAmount = amount;
            Training[] temp = new Training[amount];
            for(int i = 0; i<training.Length && i<temp.Length;++i)
            {
                temp[i].otherFieldEv = training[i].otherFieldEv;
                temp[i].ownFieldEv = training[i].ownFieldEv;
                temp[i].gold = training[i].gold;
                temp[i].warrior = training[i].warrior;
                temp[i].plane = training[i].plane;
                temp[i].turret = training[i].turret;
                temp[i].wait = training[i].wait;
            }
            training = new Training[amount];
            training = temp;
        }
    }

    public void OnBeforeSerialize() { }
}

[System.Serializable]
public struct Training
{
    //Inputs
    [Tooltip("0 - 0.5 Lose | 0.5 - 1 Win")]
    public float otherFieldEv;
    [Tooltip("0 - 0.5 Lose | 0.5 - 1 Win")]
    public float ownFieldEv;
    [Tooltip("0.1 - Oro bajo | 0.9 - Oro alto")]
    public float gold;

    //Outputs
    public float warrior;
    public float plane;
    public float turret;
    public float wait;
}
