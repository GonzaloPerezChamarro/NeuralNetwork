using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase capa
public class Layer
{
    //Numero de Nodos, Numero de Nodos de la capa Hija, Numero de nodos de la capa Padre
    private int nNodes = 0, nNodesChild = 0, nNodesFather = 0;

    //Pesos
    public double[,] weigths = null;

    //Incremento de los pesos
    private double[,] weigthsIncrease = null;

    //Valores de las neuronas
    public double[] neuronValues;

    //Valores deseados de las neuronas
    public double[] desiredValues;

    //Errores
    private double[] errors;

    //Valores de los Bias
    private double[] vBias = null;

    //Peso de los Bias
    private double[] wBias = null;

    //Capa padre, Capa hija
    private Layer layerFather, layerChild;

    //Constructor
    public Layer(int _nNodes)
    {
        nNodes = _nNodes;

        layerChild = null;
        layerFather = null;

        neuronValues    = new double[nNodes];
        desiredValues    = new double[nNodes];
        errors      = new double[nNodes];
    }

    //Se llama a este método una vez que estan construidas las tres capas
    public void Init(Layer _layerFather, Layer _layerChild)
    {
        if (_layerFather != null)
        {
            layerFather = _layerFather;
            nNodesFather = _layerFather.GetNodesCount();
        }
        else
        {
            layerFather = null;
            nNodesFather = 0;
        }

        if (_layerChild != null)
        {
            layerChild = _layerChild;
            nNodesChild = _layerChild.GetNodesCount();
            weigths = new double[nNodes, nNodesChild];
            weigthsIncrease = new double[nNodes, nNodesChild];
            vBias = new double[nNodesChild];
            wBias = new double[nNodesChild];
        }
        else
        {
            layerChild = null;
            nNodesChild = 0;
            weigths = null;
            weigthsIncrease = null;
            vBias = null;
            wBias = null;
        }

        for(int i = 0; i < nNodes; i++)
        {
            neuronValues[i] = 0;
            desiredValues[i] = 0;
            errors[i] = 0;
        }

        if(layerChild != null)
        {
            for (int i = 0; i < nNodes; i++)
            {
                for(int j = 0; j < nNodesChild; j++)
                {
                    weigths[i, j] = 0;
                    weigthsIncrease[i, j] = 0;
                }
            }

            for (int j = 0; j < nNodesChild; j++)
            {
                vBias[j] = -1;
                wBias[j] = 0;
            }
        }
    }

    //Devuelve la cantidad de nodos de la copa
    public int GetNodesCount(){ return nNodes; }

    //Asignación aleatoria de pesos
    public void RandomWeights()
    {
        if (layerChild != null)
        {
            for(int i = 0; i < nNodes; ++i)
            {
                for(int j = 0; j < nNodesChild; ++j)
                {
                    weigths[i, j] = GetRandom();
                }
            }

            for(int i = 0; i < nNodesChild; ++i)
            {
                wBias[i] = GetRandom();
            }

        }
    }

    //Devuelve un valor aleatorio para los pesos y los pesos de los Bias
    private double GetRandom() { return Random.Range(-1f, 1f); }

    //Calcula el valor de las neuronas
    public void CalculateNeurons()
    {
        if (layerFather != null)
        {
            for(int j = 0; j < nNodes; ++j)
            {
                double value = 0;
                for(int i = 0; i < nNodesFather; ++i)
                {
                    value += layerFather.neuronValues[i] * layerFather.weigths[i, j];
                }
                value += layerFather.vBias[j] * layerFather.wBias[j];

                if (layerChild == null && Const.OUTPUT_LINEAL)
                    neuronValues[j] = value;
                else
                    neuronValues[j] = 1 / (1 + Mathf.Exp((float)-value));
            }   
        }
    }

    //Calcula el valor de los errores
    public void CalculateErrors()
    {
        if (layerChild == null)
        {
            for(int i = 0; i < nNodes; ++i)
            {
                errors[i] = (desiredValues[i] - neuronValues[i]) * neuronValues[i] * (1 - neuronValues[i]);
            }
        }
        else if (layerFather != null)
        {
            for(int i = 0; i < nNodes; ++i)
            {
                double value = 0;
                for(int j = 0; j < nNodesChild; ++j)
                {
                    value += layerChild.errors[j] * weigths[i, j];
                }
                errors[i] = value * neuronValues[i] * (1 - neuronValues[i]);
            }
        }
    }

    //Ajuste de pesos
    public void AdjustWeigths()
    {
        if (layerChild != null)
        {
            for(int i = 0; i < nNodes; ++i)
            {
                for(int j = 0; j < nNodesChild; ++j)
                {
                    double newWeigth = Const.LEARNING_RATIO * layerChild.errors[j] * neuronValues[i];
                    if (Const.INERTIA)
                    {
                        weigths[i, j] += newWeigth + Const.RATIO_INERTIA * weigthsIncrease[i, j];
                        weigthsIncrease[i, j] = newWeigth;
                    }
                    else
                    {
                        
                        weigths[i, j] += newWeigth;
                    }
                }
            }

            for(int i =0; i < nNodesChild; ++i)
            {
                double newWeigth = Const.LEARNING_RATIO * layerChild.errors[i] * vBias[i];
                wBias[i] += newWeigth;
            }
        }
    }

}
