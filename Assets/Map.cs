using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject block;

    // Start is called before the first frame update
    void Start()
    {
        //Create PD Grid
        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                GameObject gridBlock = Instantiate(block, new Vector3(-2.04f + (j * 1.02f), 0.01f, 2.04f - (i * 1.02f)), Quaternion.identity);
                gridBlock.transform.parent = this.transform;
                gridBlock.name = (i+1).ToString() + "-" + (j+1).ToString();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
