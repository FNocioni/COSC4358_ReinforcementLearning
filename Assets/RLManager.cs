using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RLManager : MonoBehaviour
{
    private float startDelay = 0.1f;
    bool start = false;
    private float stepTimer = 0.5f;
    public float speed = 0.5f;
    public int steps = 0;
    private string method = "random";
    private string finalStrategy = "greedy";
    public GameObject male, female;
    bool running = false;
    bool usingSarsa = false;

    double totalDistance = 0;
    double manhattanDistance = 0;

    public double learningRate;
    public double discountRate;
    
    private int bestRun = 8000;
    private int currentRun = 0;
    private int numberRuns = 0;
    private int stepsRunCombined = 0;
    private int experiment;

    public TextMeshProUGUI stepCounter;
    public TextMeshProUGUI bestCounter;
    public TextMeshProUGUI numCounter;
    public TextMeshProUGUI avgCounter;
    public TextMeshProUGUI speedCounter;
    public TextMeshProUGUI selectedStrategy;
    public TextMeshProUGUI selectedAlgorithm;
    public TextMeshProUGUI currentLearningRate;
    public TextMeshProUGUI E4Results;
    public TextMeshProUGUI AvgDistance;

    private List<int> exp4 = new List<int>();   //For storing experiment 4 runs

    // Start is called before the first frame update
    void Start()
    {
        speed = 0.5f;
    }

    void calculateManhattan()
    {
        int x = male.GetComponent<Agent>().currentX;
        int y = male.GetComponent<Agent>().currentY;
        int xx = female.GetComponent<Agent>().currentX;
        int yy = female.GetComponent<Agent>().currentY;
        totalDistance += Mathf.Abs(x - xx) + Mathf.Abs(y - yy);
        manhattanDistance = totalDistance / steps;
    }

    public void updateLearningRate()
    {
        male.GetComponent<Agent>().learningRate = learningRate;
        female.GetComponent<Agent>().learningRate = learningRate;
    }

    public void changeSpeed(float s) {
        speed = s;
        speedCounter.text = "Speed: " + speed.ToString("F3");
    } 
    public void changeStrategy(string s) { finalStrategy = s; }
    public void changeAlgorithm()
    {
        if (usingSarsa)
        {
            usingSarsa = false;
            male.GetComponent<Agent>().changeAlgorithm(false);
            female.GetComponent<Agent>().changeAlgorithm(false);
            selectedAlgorithm.text = "Q-Learning";
        }
        else
        {
            usingSarsa = true;
            male.GetComponent<Agent>().changeAlgorithm(true);
            female.GetComponent<Agent>().changeAlgorithm(true);
            selectedAlgorithm.text = "SARSA";
        }

    }

    public void resetOriginalPickup()
    {
        Block script;
        for (int x = 1; x <= 5; x++)
        {
            for (int y = 1; y <= 5; y++)
            {
                script = GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>();
                if (script.getState() == 2)
                {
                    script.changeState('n');
                }
            }
        }
        changeBlockState(1, 1, 'd', 0);
        changeBlockState(5, 1, 'd', 0);
        changeBlockState(3, 3, 'd', 0);
        changeBlockState(5, 5, 'd', 0);
        changeBlockState(2, 4, 'p', 10);
        changeBlockState(5, 3, 'p', 10);
    }

    public void startExperiment1()
    {
        totalDistance = 0;
        manhattanDistance = 0;
        resetOriginalPickup();
        stepTimer = speed;
        learningRate = 0.3;
        bestRun = 8000;
        currentRun = 0;
        numberRuns = 0;
        stepsRunCombined = 0;
        steps = 0;
        male.GetComponent<Agent>().resetQ();
        female.GetComponent<Agent>().resetQ();        
        method = "random";
        bestCounter.text = "Best Run: ";
        avgCounter.text = "Average: ";
        running = true;
        experiment = 1;
        updateLearningRate();
        if (usingSarsa) changeAlgorithm();
        resetStates();
    }

    public void startExperiment2()
    {
        totalDistance = 0;
        manhattanDistance = 0;
        resetOriginalPickup();
        stepTimer = speed;
        learningRate = 0.3;
        bestRun = 8000;
        currentRun = 0;
        numberRuns = 0;
        stepsRunCombined = 0;
        steps = 0;
        male.GetComponent<Agent>().resetQ();
        female.GetComponent<Agent>().resetQ();
        method = "random";
        finalStrategy = "exploit";
        bestCounter.text = "Best Run: ";
        avgCounter.text = "Average: ";
        running = true;
        experiment = 2;
        updateLearningRate();
        if (!usingSarsa) changeAlgorithm();
        resetStates();
    }

    public void startExperiment3_1()
    {
        totalDistance = 0;
        manhattanDistance = 0;
        resetOriginalPickup();
        stepTimer = speed;
        learningRate = 0.15;
        bestRun = 8000;
        currentRun = 0;
        numberRuns = 0;
        stepsRunCombined = 0;
        steps = 0;
        male.GetComponent<Agent>().resetQ();
        female.GetComponent<Agent>().resetQ();
        method = "random";
        bestCounter.text = "Best Run: ";
        avgCounter.text = "Average: ";
        running = true;
        experiment = 3;
        updateLearningRate();
        if (usingSarsa) changeAlgorithm();
        resetStates();
    }

    public void startExperiment3_2()
    {
        totalDistance = 0;
        manhattanDistance = 0;
        resetOriginalPickup();
        stepTimer = speed;
        learningRate = 0.45;
        bestRun = 8000;
        currentRun = 0;
        numberRuns = 0;
        stepsRunCombined = 0;
        steps = 0;
        male.GetComponent<Agent>().resetQ();
        female.GetComponent<Agent>().resetQ();
        method = "random";
        bestCounter.text = "Best Run: ";
        avgCounter.text = "Average: ";
        running = true;
        experiment = 3;
        updateLearningRate();
        if (usingSarsa) changeAlgorithm();
        resetStates();
    }

    public void startExperiment4()
    {
        totalDistance = 0;
        manhattanDistance = 0;
        exp4.Clear();
        stepTimer = speed;
        finalStrategy = "exploit";
        learningRate = 0.3;
        bestRun = 8000;
        currentRun = 0;
        numberRuns = 0;
        stepsRunCombined = 0;
        steps = 0;
        male.GetComponent<Agent>().resetQ();
        female.GetComponent<Agent>().resetQ();
        method = "random";
        bestCounter.text = "Best Run: ";
        avgCounter.text = "Average: ";
        running = true;
        experiment = 4;
        updateLearningRate();
        if (!usingSarsa) changeAlgorithm();
        resetStates();
    }

    void changePickup()
    {
        Block script;
        for(int x = 1; x <= 5; x++)
        {
            for(int y = 1; y <= 5; y++)
            {
                script = GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>();
                if(script.getState() == 2)
                {
                    script.changeState('n');
                }
            }
        }
        script = GameObject.Find("/Map/2-1").GetComponent<Block>();
        script.changeState('p');

        script = GameObject.Find("/Map/5-4").GetComponent<Block>();
        script.changeState('p');
        resetStates();
    }



    bool checkStates()
    {
        bool terminalState = true;
        for(int x = 1; x <= 5; x++)
        {
            for(int y = 1; y <= 5; y++)
            {
                Block script = GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>();
                if (script.getState() == 1 && script.getBlocks() < 5) terminalState = false;    //Blocks left to drop-off
                if (script.getState() == 2 && script.getBlocks() > 0) terminalState = false;    //Blocks left to pick up
            }
        }
        return terminalState;
    }

    void resetStates()
    {
        E4Results.enabled = false;
        for (int x = 1; x <= 5; x++)
        {
            for (int y = 1; y <= 5; y++)
            {
                Block script = GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>();
                if (script.getState() == 1) script.changeBlocks(0);
                if (script.getState() == 2) script.changeBlocks(10);    //Blocks left to pick up
            }
        }
        male.GetComponent<Agent>().move(3, 5);
        female.GetComponent<Agent>().move(3, 1);

        male.GetComponent<Agent>().carryingBlock = false;
        female.GetComponent<Agent>().carryingBlock = false;
    }

    // Update is called once per frame
    void Update()
    {
        stepCounter.text = "Steps: " + steps.ToString();
        if(bestRun != 8000) bestCounter.text = "Best Run: " + bestRun.ToString();
        numCounter.text = "Number of runs: " + numberRuns.ToString();
        if(numberRuns > 0) avgCounter.text = "Average: " + (stepsRunCombined / numberRuns).ToString();        
        selectedStrategy.text = "Selected strategy: " + finalStrategy;
        currentLearningRate.text = "Current Learning Rate: " + learningRate.ToString();
        AvgDistance.text = "Avg Distance: " + ((float)manhattanDistance).ToString("F2");

        if (startDelay > 0f) startDelay -= Time.deltaTime;
        if (startDelay < 0f && !start)
        {
            start = true;
            //Not for experiment 4
            changeBlockState(1, 1, 'd', 0);
            changeBlockState(5, 1, 'd', 0);
            changeBlockState(3, 3, 'd', 0);
            changeBlockState(5, 5, 'd', 0);
            changeBlockState(2, 4, 'p', 10);
            changeBlockState(5, 3, 'p', 10);
        }

        stepTimer -= Time.deltaTime;
        if(stepTimer < 0f && running)
        {            
            if (steps > 500)
            {
                method = finalStrategy;
            }
            steps++;
            currentRun++;
            female.GetComponent<Agent>().makeMove(method);
            male.GetComponent<Agent>().makeMove(method);
            stepTimer = speed;

            //Check if terminal state reached
            if (checkStates())
            {             
                if(experiment == 4)
                {
                    exp4.Add(currentRun);
                }
                if (experiment == 4 && numberRuns == 2) changePickup();
                resetStates();
                if (currentRun < bestRun) bestRun = currentRun;
                stepsRunCombined += currentRun;
                currentRun = 0;
                numberRuns++;
                                
                if(experiment == 4 && numberRuns == 6)
                {
                    running = false;
                    string temp = "Experiment 4 Runs:";                    
                    foreach(int i in exp4)
                    {
                        temp += "\n" + i;
                    }
                    E4Results.text = temp;
                    E4Results.enabled = true;
                    female.GetComponent<Agent>().printQTable();
                    male.GetComponent<Agent>().printQTable();
                }
            }
            calculateManhattan();   //Calculate manhattan distance of agents

            if (steps >= 8000)
            {
                female.GetComponent<Agent>().printQTable();
                male.GetComponent<Agent>().printQTable();                
                running = false;
            }
            
            
        }
        
    }

    void changeBlockState(int x, int y, char state, int blocks)
    {
        GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>().changeState(state);
        GameObject.Find("/Map/" + y + "-" + x).GetComponent<Block>().changeBlocks(blocks);
    }
}
