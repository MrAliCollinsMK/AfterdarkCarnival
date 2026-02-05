using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TakeCoverTest : MonoBehaviour
{
    [System.Serializable]
    public class CoverPoint
    {
        public Vector3 position;
        public CoverDirection direction;
        public Vector3 normal;
    }

    public enum CoverDirection
    {
        Right,
        Left,
        Center,
    }

    private NavMeshAgent agent;
    [SerializeField]
    float dot;
    [SerializeField]
    RaycastHit[] hits;

    public Transform target;

    [SerializeField]
    Vector3[] vertices;

    List<Vector3> nearVs = new List<Vector3>();
    public List<CoverPoint> coverPoints = new List<CoverPoint>();

    public CoverPoint CurrentCover;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
            TakeCover();
    }


    void TakeCover()
    {
        var navMesh = NavMesh.CalculateTriangulation();
        vertices = navMesh.vertices;
        nearVs.Clear();
        coverPoints.Clear();

        foreach(Vector3 v in vertices)
        {
            if (Vector3.Distance(v, transform.position) < 10 && Mathf.Abs(v.y - transform.position.y) > 1.5f)
                continue;

            Vector3 nml = Vector3.zero;
            if (!IsPointSafe(v, target.position, out nml))
                continue;

            AddCoverPoint(v, nml);
        }

        if (coverPoints.Count != 0)
        {
            CurrentCover = coverPoints[Random.Range(0, coverPoints.Count - 1)];
            agent.SetDestination(CurrentCover.position);
        }

    }

    void AddCoverPoint(Vector3 v, Vector3 nml)
    {



        CoverPoint cp = new CoverPoint();

        //correct cover pos with shooting rays to normals
        Vector3 posTo = target.position + Vector3.up * 1.5f;
        cp.normal = (target.position - v).normalized * -1;

        Vector3 rightCheckPos = v + cp.normal * 0.5f + (Quaternion.AngleAxis(90, cp.normal) * Vector3.up) * 0.5f + Vector3.up * 1.5f;
        Vector3 leftCheckPos = v + cp.normal * 0.5f + (Quaternion.AngleAxis(-90, cp.normal) * Vector3.up) * 0.5f + Vector3.up * 1.5f;
        Vector3 lowCheckPos = v + cp.normal * 0.1f + Vector3.up * 0.25f;
        RaycastHit rHit;
        RaycastHit lHit;
        RaycastHit lowHit;
        bool somethingOnRight = false;
        bool somethingOnLeft = false;
        bool somethingOnLow = false;

        if (Physics.Raycast(rightCheckPos, cp.normal * -1, out rHit, 2f))
        {
            Debug.DrawLine(rightCheckPos, rHit.point, Color.blue);
            somethingOnRight = true;
        }
        if (Physics.Raycast(leftCheckPos, cp.normal * -1, out lHit, 2f))
        {
            Debug.DrawLine(leftCheckPos, lHit.point, Color.red);
            somethingOnLeft = true;
        }

        if (!somethingOnRight && !somethingOnLeft)
        {
            if (Physics.Raycast(lowCheckPos, (posTo - lowCheckPos).normalized, out lowHit, 2f))
            {
                Debug.DrawLine(lowCheckPos, lowHit.point, Color.yellow);
                somethingOnLow = true;
                cp.position = new Vector3(lowCheckPos.x, v.y, lowCheckPos.z);
                cp.normal = (posTo - lowCheckPos).normalized * -1;
                cp.direction = CoverDirection.Center;
            }
        }

        if (somethingOnLeft && somethingOnRight)
            return;

        if (somethingOnRight)
        {
            //cp.position = Vector3.Lerp(v, new Vector3(rHit.point.x, v.y, rHit.point.z), 1f);
            cp.position = new Vector3(rightCheckPos.x, v.y, rightCheckPos.z);
            cp.direction = CoverDirection.Right;
        }
        else if (somethingOnLeft)
        {
            //cp.position = Vector3.Lerp(v, new Vector3(lHit.point.x, v.y, lHit.point.z), 1f);
            cp.position = new Vector3(leftCheckPos.x, v.y, leftCheckPos.z);
            cp.direction = CoverDirection.Left;
        }

        if (!somethingOnLeft && !somethingOnRight && !somethingOnLow)
            return;

        //check if we can shoot from cover pos
        bool canShootFromRight = false;
        bool canShootFromLeft = false;
        bool canShootFromCenter = false;

        if (!IsSomethingBetween(rightCheckPos, posTo))
        {
            Debug.DrawLine(rightCheckPos, posTo, Color.red);
            canShootFromLeft = true;
        }
        else if (!IsSomethingBetween(leftCheckPos, posTo))
        {
            Debug.DrawLine(leftCheckPos, posTo, Color.blue);
            canShootFromRight = true;
        }

        if (!IsSomethingBetween(lowCheckPos + Vector3.up * 1.25f, posTo))
        {
            Debug.DrawLine(lowCheckPos + Vector3.down * 0.25f, posTo, Color.yellow);
            canShootFromCenter = true;

        }

        if (!canShootFromCenter && !canShootFromLeft && !canShootFromRight)
            return;

        foreach (CoverPoint c in coverPoints)
        {
            if (Vector3.Distance(cp.position, c.position) < 0.01f)
                return;
        }

        coverPoints.Add(cp);


    }

    bool IsPointSafe(Vector3 pos, Vector3 targetPos, out Vector3 normal)
    {
        normal = Vector3.zero;
        NavMeshHit hit;
        if (!NavMesh.FindClosestEdge(pos, out hit, NavMesh.AllAreas))
            return false;
        normal = hit.normal;

        if (!IsFront((targetPos - transform.position).normalized, hit.normal))
            return false;
        
        Vector3 raystart = pos + Vector3.up*0.5f;
        Vector3 end = targetPos;

        NavMeshHit nHit;
        if (NavMesh.Raycast(raystart, end, out nHit, NavMesh.AllAreas))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    bool IsFront(Vector3 fwd, Vector3 dir)
    {
        dot = Vector3.Dot(fwd, dir);
        if (dot < 0.0f)
            return true;
        else
            return false;
    }

    bool IsSomethingBetween(Vector3 from, Vector3 to)
    {
        hits = Physics.RaycastAll(from, (to - from).normalized, Vector3.Distance(from, to));

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform != target && hits[i].transform != transform /*&& Vector3.Distance(hits[i].point,from)>0.1f*/)
            {
                return true;
            }
        }

        //Debug.DrawLine(from, to, Color.red);
        return false;
    }

    private void OnDrawGizmos()
    {
        

        if (vertices != null && vertices.Length != 0)
        {
            Gizmos.color = Color.white;

            foreach (Vector3 v in vertices)
            {
                Gizmos.DrawWireSphere(v, 0.25f);
            }
        }

        if (coverPoints.Count!= 0)
        {

            foreach (CoverPoint cp in coverPoints)
            {
                Color c = Color.white;
                switch (cp.direction)
                {
                    case CoverDirection.Left:
                        Gizmos.color = Color.red;
                        c = Color.red;
                        break;
                    case CoverDirection.Right:
                        Gizmos.color = Color.blue;
                        c = Color.blue;
                        break;
                    case CoverDirection.Center:
                        Gizmos.color = Color.yellow;
                        c = Color.yellow;
                        break;
                }

                Gizmos.DrawWireSphere(cp.position, 0.25f);
            }
        }
    }
}
