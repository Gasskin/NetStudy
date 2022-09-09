using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    private NavMeshAgent agent;
    private NetManager netManager;

    public void Init(NetManager netManager)
    {
        this.netManager = netManager;
        agent = GetComponent<NavMeshAgent>();

        netManager.AddListener("Move", OnMove);
        netManager.AddListener("Enter", OnEnter);
    }

    private void OnMove(string arg)
    {
        Debug.Log($"Move:{arg}");
    }

    private void OnEnter(string arg)
    {
        if (arg == netManager.socket.LocalEndPoint.ToString())
            return;
        var res = Resources.Load("Bot");
    }

    void Update()
    {
        if (agent == null || netManager == null)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
            if (Physics.Raycast(ray, out var hit, int.MaxValue, 1 << LayerMask.NameToLayer("ground")))
            {
                agent.SetDestination(hit.point);
                netManager.Send($"Move;{netManager.socket.LocalEndPoint};{hit.point.x};{hit.point.y};{hit.point.z}");
            }
        }
    }
}
