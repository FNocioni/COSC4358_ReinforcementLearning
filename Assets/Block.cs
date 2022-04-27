using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Block : MonoBehaviour
{
    public Material normal, dropoff, pickup;
    private Material active;
    public GameObject counter;
    private TextMeshPro counterText;

    //0 = normal, 1 = dropoff, 2 = pickup
    int state = 0;
    int blocks;

    public void changeState(char s)
    {
        if(s == 'd')
        {
            state = 1;
            GetComponent<MeshRenderer>().material = dropoff;
        }
        else if(s == 'p')
        {
            state = 2;
            GetComponent<MeshRenderer>().material = pickup;
        }
        else
        {
            state = 0;
            GetComponent<MeshRenderer>().material = normal;
        }
    }

    public void changeBlocks(int b) { blocks = b; }

    public void takeBlock() { blocks--; }
    public void giveBlock() { blocks++; }
    public int getState() { return state; }
    public int getBlocks() { return blocks; }

    // Start is called before the first frame update
    void Start()
    {
        counterText = counter.transform.GetComponent<TextMeshPro>();
        counterText.text = blocks.ToString();
        GetComponent<MeshRenderer>().material = normal;
    }

    // Update is called once per frame
    void Update()
    {
        counterText.text = blocks.ToString();
        switch (state)
        {
            case 0:
                counter.SetActive(false);
                break;
            case 1:
                counterText.color = new Color(0, 80, 0, 255);
                counter.SetActive(true);
                break;
            case 2:
                counterText.color = new Color(0, 0, 180, 255);
                counter.SetActive(true);
                break;
        }
    }
}
