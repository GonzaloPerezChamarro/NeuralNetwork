using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Red Neuronal del Juego
public class RoyaleNeuralNetwork : MonoBehaviour
{
    //Valores iniciales de entreno
    [SerializeField]
    private NeuronsValues neuronsValues;

    //Entreno
    public float[,] training;

    //Red Neuronal
    private NeuralNetwork network;

    //Sigleton de la Red Neuronal del Juego
    public static RoyaleNeuralNetwork instance;

    //Numero de capas
    private int nInput = 3, nHidden = 4, nOutput = 4;

    public RoyaleNeuralNetwork(){}

    private void Start()
    {
        instance = this;
        network = new NeuralNetwork(nInput, nHidden, nOutput);
        LoadTraining();
        TrainNetwork();
    }

    //Carga el entrenamiento del Scriptable Object
    public void LoadTraining()
    {
        training = new float[neuronsValues.training.Length, 7];
        for(int i = 0; i < neuronsValues.training.Length; ++i)
        {
            training[i, 0] = neuronsValues.training[i].otherFieldEv;
            training[i, 1] = neuronsValues.training[i].ownFieldEv;
            training[i, 2] = neuronsValues.training[i].gold;
            training[i, 3] = neuronsValues.training[i].warrior;
            training[i, 4] = neuronsValues.training[i].plane;
            training[i, 5] = neuronsValues.training[i].turret;
            training[i, 6] = neuronsValues.training[i].wait;
        }
    }

    //Entrena la Red Neuronal
    public void TrainNetwork()
    {
        double error = 1;
        int epoch = 0;

        while(error > 0.05f && epoch < 50000)
        {
            error = 0;
            ++epoch;
            
            for(int i = 0; i< training.GetLength(0); ++i)
            {
                for(int j = 0; j < nInput; ++j)
                {
                    network.SetInput(j, training[i, j]);
                }

                for(int j = nInput; j < training.GetLength(1); ++j)
                {
                    network.SetDesiredOutput(j - nInput, training[i, j]);
                }

                network.FeedForward();
                error += network.CalculateError();
                network.BackPropagation();
            }

            error /= training.GetLength(0);
        }
    }

    //Reentrena la red neuronal
    public void RetrainNetwork(float [] inputs, float[]output)
    {
        double error = 1; 
        int epoch = 0;

        while (error > 0.05f && epoch < 50000)
        {
            error = 0;
            ++epoch;
            
            for(int i = 0; i < nInput; ++i)
            {
                network.SetInput(i, inputs[i]);
            }

            for(int i = 0; i < nOutput; ++i)
            {
                network.SetDesiredOutput(i, output[i]);
            }

            network.FeedForward();
            error = network.CalculateError();
            network.BackPropagation();
        }
    }

    //Comprueba una nueva accion a partir del input dado
    public double[] CheckAction(float [] input)
    {
        for(int i = 0; i < nInput; i++)
        {
            network.SetInput(i, input[i]);
        }
        network.FeedForward();
        return network.GetOutput();
    }
}
