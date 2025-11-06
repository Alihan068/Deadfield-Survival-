using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeightedObject {
    public GameObject gameObject;
    public float weight;
}

public class DropLootOnDeath : MonoBehaviour {

    public float frequency = 1; //0 - never create. 1 - always create. 0.5 - create 50% of the time.

    public List<WeightedObject> weightedObjectList;

    //Game game;

    // Use this for initialization
    void Start() {
        //game = FindObjectOfType<Game>();
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnDestroy() {
        if (Random.Range(0f, .99f) < frequency) {
            GameObject gameObjectToCreate = SelectWeightedObject();
            Instantiate(gameObjectToCreate, transform.position, transform.rotation);
        }
    }

    private GameObject SelectWeightedObject() {
        GameObject selected = null;
        float maxChoice = SumOfWeights;
        float randChoice = Random.Range(0, maxChoice);
        float weightSum = 0;

        foreach (WeightedObject weightedObject in weightedObjectList) {
            weightSum += weightedObject.weight;
            if (randChoice <= weightSum) {
                selected = weightedObject.gameObject;
                break;
            }
        }

        return selected;
    }

    private float SumOfWeights {
        get {
            float sumOfWeights = 0;
            foreach (WeightedObject weightedObject in weightedObjectList) {
                sumOfWeights += weightedObject.weight;
            }

            return sumOfWeights;
        }
    }
}