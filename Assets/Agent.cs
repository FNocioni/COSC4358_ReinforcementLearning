using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using System.IO;

public class Agent : MonoBehaviour
{
    //Q-Table columns: N, S, E, W, P, D
    public double[,] qTable = new double[25, 6];
    public double[,] qTablePickup = new double[25, 6];
    public int currentX, currentY;
    public bool carryingBlock = false;
    public GameObject block;
    public GameObject other;
    private Agent otherAgent;
    public bool usingSarsa = true; //When false, use Q-learning. Otherwise use SARSA

    public double learningRate;
    public double discountFactor;

    public void changeAlgorithm(bool b) { usingSarsa = b; }
    public bool sarsaActive() { return usingSarsa; }

    public void resetQ()
    {
        //Initialize q values
        //No Pickup
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int i = 0; i < 6; i++)
                {
                    qTable[((x * 5) + y), i] = 0.0;
                }
            }
        }


        //Pickup
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int i = 0; i < 6; i++)
                {
                    qTablePickup[((x * 5) + y), i] = 0.0;
                }
            }
        }

        //Define walls in q-table
        for (int i = 0; i < 5; i++)
        {
            qTable[i, 0] = -99.9;
            qTablePickup[i, 0] = -99.9;
        }
        for (int i = 20; i < 25; i++)
        {
            qTable[i, 1] = -99.9;
            qTablePickup[i, 1] = -99.9;
        }
        for (int i = 0; i < 21; i += 5)
        {
            qTable[i, 3] = -99.9;
            qTablePickup[i, 3] = -99.9;
        }
        for (int i = 4; i < 26; i += 5)
        {
            qTable[i, 2] = -99.9;
            qTablePickup[i, 2] = -99.9;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Initialize starting position
        transform.position = new Vector3(-2.04f + ((currentX - 1) * 1.02f), 0.2f, 2.04f + ((currentY - 1) * -1.02f));
        otherAgent = other.GetComponent<Agent>();

        //Initialize q values
        //No Pickup
        for(int x = 0; x < 5; x++)
        {
            for(int y = 0; y < 5; y++)
            {
                for(int i = 0; i < 6; i++)
                {
                    qTable[((x * 5) + y), i] = 0.0;
                }
            }
        }


        //Pickup
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int i = 0; i < 6; i++)
                {
                    qTablePickup[((x * 5) + y), i] = 0.0;
                }
            }
        }

        //Define walls in q-table
        for(int i = 0; i < 5; i++)
        {            
            qTable[i, 0] = -99.9;
            qTablePickup[i, 0] = -99.9;            
        }    
        for (int i = 20; i < 25; i++)
        {
            qTable[i, 1] = -99.9;
            qTablePickup[i, 1] = -99.9;
        }
        for(int i = 0; i < 21; i += 5)
        {
            qTable[i, 3] = -99.9;
            qTablePickup[i, 3] = -99.9;
        }
        for (int i = 4; i < 26; i += 5)
        {
            qTable[i, 2] = -99.9;
            qTablePickup[i, 2] = -99.9;
        }
    }

    public void printQTable()
    {
        string filename = Application.dataPath + "/" + this.name + "_qTable.csv";
        TextWriter tw = new StreamWriter(filename, false);
        tw.WriteLine("Blocks (Y, X), N, S, E, W, P, D");
        tw.Close();
        tw = new StreamWriter(filename, true);
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                string temp = (x + 1) + "_" + (y + 1);
                for (int i = 0; i < 6; i++)
                {
                    temp += "," + qTable[((x * 5) + y), i];
                }
                tw.WriteLine(temp);
            }
        }
        tw.WriteLine();
        tw.WriteLine();
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                string temp = (x + 1) + "_" + (y + 1);
                for (int i = 0; i < 6; i++)
                {
                    temp += "," + qTablePickup[((x * 5) + y), i];
                }
                tw.WriteLine(temp);
            }
        }
        tw.Close();
    }


    // Update is called once per frame
    void Update()
    {
        if (carryingBlock)
        {
            block.SetActive(true);
        }
        else
        {
            block.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Space)){
            //makeMove("random");
        }
    }

    //Structure for all possible operators
    struct Operator
    {
        public int x, y;
        public char dir;
        public double q;

        public Operator(int x, int y, char dir, double q)
        {
            this.x = x;
            this.y = y;
            this.dir = dir;
            this.q = q;
        }
    }

    public void makeMove(string method) //method = {random, exploit, greedy}
    {
        //Get Applicable operators
        List<Operator> aplop = getApplicableOperators();
        if (aplop.Count < 1) return;
        Operator chosen = new Operator(0, 0, ' ', 0.0);

        //Check if we have a pickup/dropoff operator available
        bool pickup = false;
        bool dropoff = false;
        foreach(Operator op in aplop)
        {
            //if(this.name == "Female Agent") Debug.Log("(" + op.x + ", " + op.y + ") " + op.dir + ", " + op.q);
            if (op.dir == 'p')
            {
                chosen = op;
                pickup = true;
            }
            if (op.dir == 'd')
            {
                chosen = op;
                dropoff = true;
            }
        }
        //Get operator Q values or choose random
        if(pickup)
        {            
            updateQ(currentX, currentY, chosen);
            move(chosen.x, chosen.y);
            carryingBlock = true;
            GameObject.Find("/Map/" + chosen.y + "-" + chosen.x).GetComponent<Block>().takeBlock();           
        }
        else if(dropoff)
        {
            updateQ(currentX, currentY, chosen);
            move(chosen.x, chosen.y);
            carryingBlock = false;
            GameObject.Find("/Map/" + chosen.y + "-" + chosen.x).GetComponent<Block>().giveBlock();
        }
        else
        {
            //Random
            if(method == "random")
            {
                chosen = aplop[UnityEngine.Random.Range(0, aplop.Count)];
            }

            //Greedy
            if(method == "greedy")
            {
                chosen = getChosen(aplop);
            }

            //Exploit (20/80)
            if(method == "exploit")
            {
                int gen = UnityEngine.Random.Range(0, 10);
                if (gen == 1 || gen == 2)
                {
                    chosen = aplop[UnityEngine.Random.Range(0, aplop.Count)];   //20% to choose random
                }
                else
                {
                    chosen = getChosen(aplop);  //80% to choose max operator
                }
            }
            updateQ(currentX, currentY, chosen);
            move(chosen.x, chosen.y);            
        }        
        
    }

    //Update QTable before moving
    void updateQ(int x, int y, Operator chosen)
    {
        double reward;
        int col = -1;
        if (!carryingBlock) //Update q table for not carrying block
        {
            switch (chosen.dir)
            {
                case 'n':
                    col = 0;
                    break;
                case 's':
                    col = 1;
                    break;
                case 'e':
                    col = 2;
                    break;
                case 'w':
                    col = 3;
                    break;
                case 'p':
                    col = 4;
                    break;
                case 'd':
                    col = 5;
                    break;
            }
            //Debug.Log("Updating q table (not carrying) for:" + (((y - 1) * 5) + (x - 1)) + ", " + col);                        
            if(col == 4)
            {
                reward = 13.0;
            }
            else
            {
                reward = -1.0;
            }
            /*
            if (this.name == "Female Agent")
            {
                Debug.Log("Calculation: " + ((1.0 - learningRate) * qTable[((y - 1) * 5) + (x - 1), col] + learningRate * (reward + (discountFactor * max))));
                Debug.Log("Learning: " + learningRate + " Current: " + qTable[((y - 1) * 5) + (x - 1), col] + " Reward: " + reward + " Discount: " + discountFactor + " Max: " + max);
            }*/
            if (usingSarsa)
            {
                double max = sarsaQ(chosen.x, chosen.y, carryingBlock);
                qTable[((y - 1) * 5) + (x - 1), col] = qTable[((y - 1) * 5) + (x - 1), col] + learningRate * (reward + (discountFactor * max) - qTable[((y - 1) * 5) + (x - 1), col]);
            }
            else
            {
                double max = getMaxQ(chosen.x, chosen.y, carryingBlock);
                qTable[((y - 1) * 5) + (x - 1), col] = (1.0 - learningRate) * qTable[((y - 1) * 5) + (x - 1), col] + learningRate * (reward + (discountFactor * max));
            }
        }
        else    //Update q table for carrying block
        {
            switch (chosen.dir)
            {
                case 'n':
                    col = 0;
                    break;
                case 's':
                    col = 1;
                    break;
                case 'e':
                    col = 2;
                    break;
                case 'w':
                    col = 3;
                    break;
                case 'p':
                    col = 4;
                    break;
                case 'd':
                    col = 5;
                    break;
            }
            //Debug.Log("Updating q table (carrying) for:" + (((y - 1) * 5) + (x - 1)) + ", " + col);
            
            if (col == 5)
            {
                reward = 13.0;
            }
            else
            {
                reward = -1.0;
            }
            /*
            if (this.name == "Female Agent")
            {
                Debug.Log("Calculation: " + ((1.0 - learningRate) * qTablePickup[((y - 1) * 5) + (x - 1), col] + learningRate * (reward + (discountFactor * max))));
                Debug.Log("Learning: " + learningRate + " Current: " + qTablePickup[((y - 1) * 5) + (x - 1), col] + " Reward: " + reward + " Discount: " + discountFactor + " Max: " + max);
            }*/
            if (usingSarsa)
            {
                double max = sarsaQ(chosen.x, chosen.y, carryingBlock);
                qTablePickup[((y - 1) * 5) + (x - 1), col] = qTablePickup[((y - 1) * 5) + (x - 1), col] + (learningRate * (reward + (discountFactor * max) - qTablePickup[((y - 1) * 5) + (x - 1), col]));
            }
            else
            {
                double max = getMaxQ(chosen.x, chosen.y, carryingBlock);
                qTablePickup[((y - 1) * 5) + (x - 1), col] = (1.0 - learningRate) * qTablePickup[((y - 1) * 5) + (x - 1), col] + (learningRate * (reward + (discountFactor * max)));
            }            
        }
    }

    public void move(int x, int y)
    {
        currentX = x;
        currentY = y;
        transform.position = new Vector3(-2.04f + ((currentX - 1) * 1.02f), 0.2f, 2.04f + ((currentY - 1) * -1.02f));
    }

    //Get best operator for next state to include in SARSA Q-Table
    double sarsaQ(int currentX, int currentY, bool carryingBlock)
    {
        List<Operator> aplop = new List<Operator>();
        if (currentY > 1 && !(otherAgent.currentX == currentX && otherAgent.currentY == currentY - 1))    //Check above current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX, currentY - 1) && GameObject.Find("/Map/" + (currentY - 1) + "-" + currentX).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY - 1, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 0]));
            }
            else if (isDropoff(currentX, currentY - 1) && GameObject.Find("/Map/" + (currentY - 1) + "-" + currentX).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY - 1, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 0]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX, currentY - 1, 'n', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 0]));
                }
                else
                {
                    aplop.Add(new Operator(currentX, currentY - 1, 'n', qTable[((currentY - 1) * 5 + (currentX - 1)), 0]));
                }
            }
        }

        if (currentY < 5 && !(otherAgent.currentX == currentX && otherAgent.currentY == currentY + 1))    //Check below current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX, currentY + 1) && GameObject.Find("/Map/" + (currentY + 1) + "-" + currentX).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY + 1, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 1]));
            }
            else if (isDropoff(currentX, currentY + 1) && GameObject.Find("/Map/" + (currentY + 1) + "-" + currentX).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY + 1, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 1]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX, currentY + 1, 's', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 1]));
                }
                else
                {
                    aplop.Add(new Operator(currentX, currentY + 1, 's', qTable[((currentY - 1) * 5 + (currentX - 1)), 1]));
                }
            }
        }

        if (currentX > 1 && !(otherAgent.currentX == currentX - 1 && otherAgent.currentY == currentY))    //Check left of current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX - 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX - 1)).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX - 1, currentY, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 3]));
            }
            else if (isDropoff(currentX - 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX - 1)).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX - 1, currentY, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 3]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX - 1, currentY, 'w', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 3]));
                }
                else
                {
                    if (!carryingBlock) aplop.Add(new Operator(currentX - 1, currentY, 'w', qTable[((currentY - 1) * 5 + (currentX - 1)), 3]));
                }
            }
        }

        if (currentX < 5 && !(otherAgent.currentX == currentX + 1 && otherAgent.currentY == currentY))    //Check right of current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX + 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX + 1)).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX + 1, currentY, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 2]));
            }
            else if (isDropoff(currentX + 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX + 1)).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX + 1, currentY, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 2]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX + 1, currentY, 'e', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 2]));
                }
                else
                {
                    if (!carryingBlock) aplop.Add(new Operator(currentX + 1, currentY, 'e', qTable[((currentY - 1) * 5 + (currentX - 1)), 2]));
                }
            }
        }
        Operator chosen = new Operator(-1, -1, 'v', -99.9);
        foreach(Operator op in aplop)
        {
            if (op.q > chosen.q && !(op.x == this.currentX && op.y == this.currentY)) chosen = op;  //Prevent back-track
        }
        if(chosen.dir == 'v')
        {
            foreach (Operator op in aplop)
            {
                if (op.q > chosen.q) chosen = op;  //If we have no other state to go to
            }
        }
        double max = chosen.q;
        return max;
    }


    List<Operator> getApplicableOperators()
    {
        List<Operator> aplop = new List<Operator>();
        if(currentY > 1 && !(otherAgent.currentX == currentX && otherAgent.currentY == currentY-1))    //Check above current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX, currentY - 1) && GameObject.Find("/Map/" + (currentY - 1) + "-" + currentX).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY - 1, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 0]));
            } else if (isDropoff(currentX, currentY - 1) && GameObject.Find("/Map/" + (currentY - 1) + "-" + currentX).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY - 1, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 0]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX, currentY - 1, 'n', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 0]));
                }
                else {
                    aplop.Add(new Operator(currentX, currentY - 1, 'n', qTable[((currentY - 1) * 5 + (currentX - 1)), 0]));
                }                               
            }            
        }

        if(currentY < 5 && !(otherAgent.currentX == currentX && otherAgent.currentY == currentY + 1))    //Check below current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX, currentY + 1) && GameObject.Find("/Map/" + (currentY+1) + "-" + currentX).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {                
                aplop.Add(new Operator(currentX, currentY + 1, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 1]));
            }
            else if (isDropoff(currentX, currentY + 1) && GameObject.Find("/Map/" + (currentY+1) + "-" + currentX).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY + 1, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 1]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX, currentY + 1, 's', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 1]));
                }
                else
                {
                    aplop.Add(new Operator(currentX, currentY + 1, 's', qTable[((currentY - 1) * 5 + (currentX - 1)), 1]));
                }                    
            }
        }

        if(currentX > 1 && !(otherAgent.currentX == currentX - 1 && otherAgent.currentY == currentY))    //Check left of current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX - 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX-1)).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX - 1, currentY, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 3]));
            }
            else if (isDropoff(currentX - 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX-1)).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX - 1, currentY, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 3]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX - 1, currentY, 'w', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 3]));
                }
                else
                {
                    if (!carryingBlock) aplop.Add(new Operator(currentX - 1, currentY, 'w', qTable[((currentY - 1) * 5 + (currentX - 1)), 3]));
                }                
            }
        }

        if(currentX < 5 && !(otherAgent.currentX == currentX + 1 && otherAgent.currentY == currentY))    //Check right of current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX + 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX+1)).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX + 1, currentY, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 2]));
            }
            else if (isDropoff(currentX + 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX+1)).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX + 1, currentY, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 2]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX + 1, currentY, 'e', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 2]));
                }
                else
                {
                    if (!carryingBlock) aplop.Add(new Operator(currentX + 1, currentY, 'e', qTable[((currentY - 1) * 5 + (currentX - 1)), 2]));
                }                
            }
        }
        return aplop;
    }

    bool isPickup(int x, int y)
    {
        if (GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>().getState() == 2) return true;
        return false;
    }

    bool isDropoff(int x, int y)
    {
        if (GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>().getState() == 1) return true;
        return false;
    }
    
    double getMaxQ(int x, int y, bool carrying)
    {
        double max = -99999.9;
        List<Operator> aplop = getFutureApplicableOperators(x, y, carrying);
        foreach(Operator op in aplop)
        {
            if(op.q > max)
            {
                max = op.q;
            }
        }
        return max;
    }

    Operator getChosen(List<Operator> aplop)
    {
        double max = -99999.9;
        Operator chosen = new Operator(-1, -1, 'v', 0.0);
        foreach(Operator op in aplop)
        {
            if(op.q > max)
            {
                max = op.q;
            }
        }
        List<Operator> refinedAplop = new List<Operator>();
        foreach(Operator op in aplop)
        {
            if(op.q == max)
            {
                refinedAplop.Add(op);
            }
        }
        return refinedAplop[UnityEngine.Random.Range(0, refinedAplop.Count)];
    }

    List<Operator> getFutureApplicableOperators(int currentX, int currentY, bool carryingBlock)
    {
        List<Operator> aplop = new List<Operator>();
        if (currentY > 1 && !(otherAgent.currentX == currentX && otherAgent.currentY == currentY - 1))    //Check above current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX, currentY - 1) && GameObject.Find("/Map/" + (currentY - 1) + "-" + currentX).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY - 1, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 0]));
            }
            else if (isDropoff(currentX, currentY - 1) && GameObject.Find("/Map/" + (currentY - 1) + "-" + currentX).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY - 1, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 0]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX, currentY - 1, 'n', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 0]));
                }
                else
                {
                    aplop.Add(new Operator(currentX, currentY - 1, 'n', qTable[((currentY - 1) * 5 + (currentX - 1)), 0]));
                }
            }
        }

        if (currentY < 5 && !(otherAgent.currentX == currentX && otherAgent.currentY == currentY + 1))    //Check below current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX, currentY + 1) && GameObject.Find("/Map/" + (currentY + 1) + "-" + currentX).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY + 1, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 1]));
            }
            else if (isDropoff(currentX, currentY + 1) && GameObject.Find("/Map/" + (currentY + 1) + "-" + currentX).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX, currentY + 1, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 1]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX, currentY + 1, 's', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 1]));
                }
                else
                {
                    aplop.Add(new Operator(currentX, currentY + 1, 's', qTable[((currentY - 1) * 5 + (currentX - 1)), 1]));
                }
            }
        }

        if (currentX > 1 && !(otherAgent.currentX == currentX - 1 && otherAgent.currentY == currentY))    //Check left of current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX - 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX - 1)).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX - 1, currentY, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 3]));
            }
            else if (isDropoff(currentX - 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX - 1)).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX - 1, currentY, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 3]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX - 1, currentY, 'w', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 3]));
                }
                else
                {
                    if (!carryingBlock) aplop.Add(new Operator(currentX - 1, currentY, 'w', qTable[((currentY - 1) * 5 + (currentX - 1)), 3]));
                }
            }
        }

        if (currentX < 5 && !(otherAgent.currentX == currentX + 1 && otherAgent.currentY == currentY))    //Check right of current block
        {
            //Check pickup and dropoff
            if (isPickup(currentX + 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX + 1)).GetComponent<Block>().getBlocks() > 0 && !carryingBlock)
            {
                aplop.Add(new Operator(currentX + 1, currentY, 'p', qTable[((currentY - 1) * 5 + (currentX - 1)), 2]));
            }
            else if (isDropoff(currentX + 1, currentY) && GameObject.Find("/Map/" + currentY + "-" + (currentX + 1)).GetComponent<Block>().getBlocks() < 5 && carryingBlock)
            {
                aplop.Add(new Operator(currentX + 1, currentY, 'd', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 2]));
            }
            else
            {
                if (carryingBlock)
                {
                    aplop.Add(new Operator(currentX + 1, currentY, 'e', qTablePickup[((currentY - 1) * 5 + (currentX - 1)), 2]));
                }
                else
                {
                    if (!carryingBlock) aplop.Add(new Operator(currentX + 1, currentY, 'e', qTable[((currentY - 1) * 5 + (currentX - 1)), 2]));
                }
            }
        }
        return aplop;
    }
}
