using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [Header("Rope Setup")]
    public GameObject ropeSegmentPrefab;
    public Rigidbody2D connectedObject;   // Gnome
    public Transform topAnchor;           // Fixed anchor at top of well

    [Header("Rope Settings")]
    public int maxSegments = 1000;        
    public float maxRopeSegmentLength = 1.5f;
    public float ropeSpeed = 20.0f;

    [Header("Collision Settings")]
    public Collider2D signalCollider;    

    [Header("Debug State")]
    public bool isIncreasing { get; private set; }
    public bool isDecreasing { get; private set; }

    private List<GameObject> ropeSegments = new List<GameObject>();
    private Queue<GameObject> ropeSegmentPool = new Queue<GameObject>();
    private LineRenderer lineRenderer;

    void Start()
    {
        if (topAnchor == null)
        {
            Debug.LogError("Top Anchor not assigned!");
            enabled = false;
            return;
        }

        lineRenderer = GetComponent<LineRenderer>();
        ResetLength();
    }

    public void ResetLength()
    {
        foreach (var segment in ropeSegments)
            ReleaseSegment(segment);

        ropeSegments.Clear();
        isIncreasing = false;
        isDecreasing = false;

        CreateRopeSegment(); // start with one
    }

    void FixedUpdate()
    {
        if (ropeSegments.Count == 0) return;

        var topSegment = ropeSegments[0];
        var joint = topSegment.GetComponent<SpringJoint2D>();

        if (isIncreasing && ropeSegments.Count < maxSegments)
        {
            if (joint.distance >= maxRopeSegmentLength)
            {
                CreateRopeSegment();
            }
            else
            {
                joint.distance += ropeSpeed * Time.fixedDeltaTime;
            }
        }
        else if (isDecreasing)
        {
            if (joint.distance <= 0.005f && ropeSegments.Count > 1)
            {
                DeactivateRopeSegment();
            }
            else
            {
                joint.distance -= ropeSpeed * Time.fixedDeltaTime;
            }
        }

        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        if (lineRenderer == null || topAnchor == null || connectedObject == null) return;

        lineRenderer.positionCount = ropeSegments.Count + 2;
        lineRenderer.SetPosition(0, topAnchor.position);

        for (int i = 0; i < ropeSegments.Count; i++)
            lineRenderer.SetPosition(i + 1, ropeSegments[i].transform.position);

        lineRenderer.SetPosition(ropeSegments.Count + 1, connectedObject.position);
    }

    void CreateRopeSegment()
    {
        Vector3 spawnPos = (ropeSegments.Count == 0)
            ? topAnchor.position
            : ropeSegments[0].transform.position;

        GameObject segment = GetPooledSegment();
        segment.transform.position = spawnPos;
        segment.transform.rotation = Quaternion.identity;
        segment.SetActive(true);

        var segmentRb = segment.GetComponent<Rigidbody2D>();
        var joint = segment.GetComponent<SpringJoint2D>();
        var segmentCol = segment.GetComponent<Collider2D>();

        ropeSegments.Insert(0, segment);

        if (signalCollider != null && segmentCol != null)
        {
            Physics2D.IgnoreCollision(segmentCol, signalCollider);
        }

        if (ropeSegments.Count == 1)
        {
            var anchorRb = topAnchor.GetComponent<Rigidbody2D>();
            joint.connectedBody = anchorRb != null ? anchorRb : null;
            joint.connectedAnchor = anchorRb == null ? topAnchor.position : Vector2.zero;
        }
        else
        {
            var nextSegment = ropeSegments[1];
            nextSegment.GetComponent<SpringJoint2D>().connectedBody = segmentRb;
        }

        if (connectedObject != null)
        {
            var gnomeJoint = connectedObject.GetComponent<SpringJoint2D>();
            if (gnomeJoint != null)
            {
                gnomeJoint.connectedBody = ropeSegments[ropeSegments.Count - 1].GetComponent<Rigidbody2D>();
                gnomeJoint.distance = 0.5f;
            }
        }
    }

    void DeactivateRopeSegment()
    {
        if (ropeSegments.Count <= 1) return;

        var topSegment = ropeSegments[0];
        ropeSegments.RemoveAt(0);
        ReleaseSegment(topSegment);

        if (connectedObject != null)
        {
            var gnomeJoint = connectedObject.GetComponent<SpringJoint2D>();
            if (gnomeJoint != null)
            {
                gnomeJoint.connectedBody = ropeSegments[ropeSegments.Count - 1].GetComponent<Rigidbody2D>();
                gnomeJoint.distance = 0.1f;
            }
        }
    }

    GameObject GetPooledSegment()
    {
        if (ropeSegmentPool.Count > 0)
            return ropeSegmentPool.Dequeue();

        return Instantiate(ropeSegmentPrefab);
    }

    void ReleaseSegment(GameObject segment)
    {
        segment.SetActive(false);
        ropeSegmentPool.Enqueue(segment);
    }

    // UI Events
    public void StartExtending() => isIncreasing = true;
    public void StopExtending() => isIncreasing = false;
    public void StartRetracting() => isDecreasing = true;
    public void StopRetracting() => isDecreasing = false;
}
