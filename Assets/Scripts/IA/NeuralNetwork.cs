using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Red Neuronal
public class NeuralNetwork
{
    //Capa de input
    protected Layer inputLayer;

    //Capa oculta
    protected Layer hideLayer;

    //Capa de output
    protected Layer outputLayer;

    //Constructor de la Red Neuronal
    public NeuralNetwork(int nInputNodes, int nHideNodes, int nOutputNodes)
    {
        inputLayer = new Layer(nInputNodes);
        hideLayer = new Layer(nHideNodes);
        outputLayer = new Layer(nOutputNodes);

        inputLayer.Init(null, hideLayer);
        hideLayer.Init(inputLayer, outputLayer);
        outputLayer.Init(hideLayer, null);

        //Asignación de pesos aleatorios
        inputLayer.RandomWeights();
        hideLayer.RandomWeights();
    }

    //Establece el input
    public void SetInput(int i, float value)
    {
        if ((i >= 0) && (i < inputLayer.GetNodesCount()))
            inputLayer.neuronValues[i] = value;
    }

    //Obtiene el output
    public double GetOutput(int i)
    {
        if ((i >= 0) && (i < outputLayer.GetNodesCount()))
            return outputLayer.neuronValues[i];
        else
            return Const.OUTPUT_ERROR;
    }

    //Establece el output deseado
    public void SetDesiredOutput(int i, float value)
    {
        if ((i >= 0) && (i < outputLayer.GetNodesCount()))
            outputLayer.desiredValues[i] = value;

    }

    //Alimenta la red hacia adelante
    public void FeedForward()
    {
        inputLayer.CalculateNeurons();
        hideLayer.CalculateNeurons();
        outputLayer.CalculateNeurons();
    }

    //Propagación hacia atrás
    public void BackPropagation()
    {
        outputLayer.CalculateErrors();
        hideLayer.CalculateErrors();

        hideLayer.AdjustWeigths();
        inputLayer.AdjustWeigths();
    }

    //Devuelve el Ouput mas alto
    public double[] GetOutput(){ return outputLayer.neuronValues; }

    //Calcula el error
    public double CalculateError()
    {
        double error = 0;

        for(int i = 0; i< outputLayer.GetNodesCount(); ++i)
        {
            
            error += Math.Pow(outputLayer.neuronValues[i] - outputLayer.desiredValues[i], 2);
        }

        error /= outputLayer.GetNodesCount();

        return error;
    }
}
