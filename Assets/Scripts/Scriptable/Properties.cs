using UnityEngine;

//Clase que genera los SerializeObject de las propiedades de las unidades
//que se utilizan durante el juego.
[CreateAssetMenu(fileName = "Properties", menuName ="Scriptable/Properties", order = 1)]
public class Properties : ScriptableObject, ISerializationCallbackReceiver
{
    public int types = 3;
    private int oldSide = 3;

    public bool saved = true;

    public MyArray[] units;

    public void OnAfterDeserialize()
    {
        if(oldSide != types && !saved)
        {
            oldSide = types;
            units = new MyArray[types * (types + 1) / 2];
            for(int i = 0; i < units.Length; ++i)
            {
                units[i] = new MyArray();
            }

        }
    }
    public void OnBeforeSerialize() {}

}

[System.Serializable]
public class MyArray
{
    public string name;
    public int health;
    public int damage;
    public int cost;
    public int gold;
    public int speed;
    public float attackRatio;
    public float attackTime;
    public UnitsTypes whatCanAttack;
}

[System.Serializable]
public class UnitsTypes
{
    public bool warrior;
    public bool plane;
    public bool turret;
    public bool tower;
}
