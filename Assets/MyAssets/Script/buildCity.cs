using System;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;
using System.Diagnostics;
using System.Threading;
using System.Collections;


public class buildCity : MonoBehaviour
{
    public GameObject[] buildings;
    public GameObject[] randomObjects;
    public GameObject xstreets;
    public GameObject zstreets;
    public GameObject crossroads;    

    public int mapWidth = 20;
    public int mapHeight = 20;
    public int buildingFootprint = 30;
    public int[,] mapgrid;

    public NavMeshSurface surface;
    public NavMeshAgent human;
    public NavMeshAgent a;

    public Vector3 start_pos;
    public Vector3 stop_pos;

    public static int nb_habitants = 200;

    NavMeshAgent[] agents = new NavMeshAgent[nb_habitants];
    Vector3[] start_pos_tab = new Vector3[nb_habitants];
    Vector3[] stop_pos_tab = new Vector3[nb_habitants];

    bool testOneHabitant = true;
    bool testManyHabitants = false;

    // Start is called before the first frame update
    void Start()
    {

        System.Random R = new System.Random();
        mapgrid = new int[mapWidth, mapHeight];

        // Generate map data
        for (int h = 0; h < mapHeight; h++)
        {
            for (int w = 0; w < mapWidth; w++)
            {
                mapgrid[w, h] = (int)(Mathf.PerlinNoise(w / 7.0f, h / 7.0f) * (buildings.Length+2));
            }
        }

        // Build streets
        int x = 0;
        for (int n = 0; n < 50; n++)
        {
            for (int h = 0; h < mapHeight; h++)
            {
                mapgrid[x, h] = -1;
            }
            x += R.Next(2, 5);
            if (x >= mapWidth) break;
        }

        int z = 0;
        for (int n = 0; n < 10; n++)
        {
            for (int w = 0; w < mapWidth; w++)
            {
                if (mapgrid[w, z] == -1) // Put in a cross road
                    mapgrid[w, z] = -3;
                else
                    mapgrid[w, z] = -2;
            }
            z += R.Next(3, 7);
            if (z >= mapHeight) break;
        }

        int seed = R.Next(0, 100);
        // Building other regions
        for (int h = 0; h < mapHeight; h++)
        {
            for (int w = 0; w < mapWidth; w++)
            {
                int result = mapgrid[w, h];
                Vector3 pos = new Vector3(w * buildingFootprint, 0, h * buildingFootprint);
                //Vector3 ro_pos = new Vector3(w * buildingFootprint + 5f, 0, h * buildingFootprint); //random object pos
                if (result < -2)
                    Instantiate(crossroads, pos, crossroads.transform.rotation);
                else if (result < -1)
                {
                    Instantiate(xstreets, pos, xstreets.transform.rotation);
                    //Instantiate(randomObjects[0], ro_pos, Quaternion.identity);
                }
                else if (result < 0)
                    Instantiate(zstreets, pos, zstreets.transform.rotation);
                else
                {
                    for (int i = 0; i < buildings.Length; i++)
                    {
                        if (result >= i && result < (i + 1))
                            Instantiate(buildings[i], pos, Quaternion.identity);
                    }
                }
            }
        }

        surface.BuildNavMesh();

        GameObject[] homeTag = GameObject.FindGameObjectsWithTag("home");
        GameObject[] buildingTag = GameObject.FindGameObjectsWithTag("building");

        int nb_homes = homeTag.Length; // Number of homes in the city 
        int nb_buildings = buildingTag.Length; // Number of buildings in the city
        int hb_per_home = (int)(nb_habitants / nb_homes);


        // CODE FOR MANY HABITANTS ----------------------------------------
        if (testManyHabitants)
        {
            int j = 0;
            for (int m = 0; m < nb_homes; m++)
            {
                for (int n = 0; n < hb_per_home; n++)
                {
                    // Starting position of human
                    start_pos = homeTag[m].transform.position;
                    start_pos.x = start_pos.x + 15f;
                    transform.position = start_pos;
                    start_pos_tab[j] = start_pos;

                    // Stopping position of human
                    stop_pos = buildingTag[R.Next(0, buildingTag.Length - 1)].transform.position;
                    stop_pos.x = stop_pos.x - 3f;
                    transform.position = stop_pos;
                    stop_pos_tab[j] = stop_pos;

                    // Create and move agent
                    agents[j] = (NavMeshAgent)Instantiate(human, start_pos, Quaternion.identity);
                    agents[j].destination = stop_pos;
                    agents[j].isStopped = false;

                    j++;
                }
            }
        }
        // -----------------------------------------------------------------

        // CODE FOR ONE HABITANT -------------------------------------------
        if (testOneHabitant)
        {
            // Starting position of habitant
            start_pos = homeTag[0].transform.position;
            start_pos.x = start_pos.x + 15f;
            transform.position = start_pos;

            // Stopping position of human
            stop_pos = buildingTag[R.Next(0, buildingTag.Length - 1)].transform.position;
            stop_pos.x = stop_pos.x - 3f;
            transform.position = stop_pos;

            // Create and move agent
            a = (NavMeshAgent)Instantiate(human, start_pos, Quaternion.identity);

            // Habitant goes to work
            a.destination = stop_pos;
            a.isStopped = false;
        }
        // ------------------------------------------------------------------

    }

    // Update is called once per frame
    void Update()
    {
        // CODE FOR MANY HABITANTS ----------------------------------------
        if (testManyHabitants)
        {
            for (int i = 0; i < nb_habitants; i++)
            {
                bool backHome = false;
                // Distance between agent position and destination
                var dist = Vector3.Distance(stop_pos_tab[i], agents[i].transform.position);
                if (dist < 2 && backHome == false)
                {
                    agents[i].isStopped = true;
                    // Make agent wait for few seconds before coming back home
                    //StartCoroutine(Test(10f));
                    agents[i].Resume();
                    backHome = true;

                    // Habitant comes back home
                    agents[i].destination = start_pos_tab[i];
                    agents[i].isStopped = false;
                }
            }
        }
        // ------------------------------------------------------------------

        // CODE FOR ONLY ONE HABITANT ----------------------------------------
        if (testOneHabitant)
        {
            bool backHome = false;
            // Distance between agent position and destination
            var dist = Vector3.Distance(stop_pos, a.transform.position);
            if (dist < 2 && backHome == false)
            {
                a.isStopped = true;
                // Make agent wait for few seconds before coming back home
                //StartCoroutine(Test(10f));
                a.Resume();
                backHome = true;

                // Habitant comes back home
                a.destination = start_pos;
                a.isStopped = false;
            }
            
        }
        // ------------------------------------------------------------------
    }


    IEnumerator Test(float sec)
    {
        yield return new WaitForSeconds(sec);
    }
}
