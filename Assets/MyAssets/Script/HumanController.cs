using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class HumanController : MonoBehaviour
{

    public Camera cam;

    public NavMeshAgent agent;

    public ThirdPersonCharacter character;

    void Start()
    {
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            character.Move(agent.desiredVelocity, false, false);
        }

        else
        {
            character.Move(Vector3.zero, false, false);
        }
        

    }
}