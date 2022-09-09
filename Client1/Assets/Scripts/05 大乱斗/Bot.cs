using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    private NavMeshAgent agent;
    private NetManager netManager;

    public void Init(NetManager netManager)
    {
        this.netManager = netManager;
        agent = GetComponent<NavMeshAgent>();

        netManager.AddListener("Move", OnMove);
    }

    private void OnMove(string arg)
    {
        var pos = arg.Split();
        var x = float.Parse(pos[0]);
        var y = float.Parse(pos[1]);
        var z = float.Parse(pos[2]);

        agent.SetDestination(new Vector3((float)x, (float)y, (float)z));
    }
}
