using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    public enum ShapeType { Pyramid, Cylinder, Rectangle, Sphere, Capsule }
    public ShapeType shapeType = ShapeType.Pyramid;

    public Material cubeMaterial; // Material used for GL lines

    // Common parameters
    public Vector3 cubeCenter = Vector3.zero;
    public float focalLength = 10f; // Optional focal length if needed

    // Rotation (x, y, and z). Setting only z will behave like before.
    public Vector3 cubeRotation = Vector3.zero;

    // Pyramid parameters
    public float pyramidBaseSize = 1f;
    public float pyramidHeight = 1f;

    // Cylinder parameters
    public float cylinderHeight = 2f;
    public float cylinderRadius = 1f;
    public int cylinderSegments = 20; // More than 5 segments recommended

    // Rectangle parameters (drawn on the XZ plane)
    public float rectangleWidth = 2f;
    public float rectangleHeight = 1f;

    // Sphere parameters
    public float sphereRadius = 1f;
    public int sphereSegments = 12; // More than 5 segments recommended

    // Capsule parameters (Bonus)
    public float capsuleRadius = 0.5f;
    public float capsuleHeight = 2f;
    public int capsuleSegments = 12;

    void OnPostRender()
    {
        if (cubeMaterial == null)
            return;
        cubeMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(cubeMaterial.color);

        DrawShape();

        GL.End();
        GL.PopMatrix();
    }

    void OnDrawGizmos()
    {
        if (cubeMaterial == null)
            return;
        cubeMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(cubeMaterial.color);

        DrawShape();

        GL.End();
        GL.PopMatrix();
    }

    // Rotates a vertex about cubeCenter using the Euler angles from cubeRotation.
    Vector3 RotateVertex(Vector3 vertex)
    {
        Quaternion rot = Quaternion.Euler(cubeRotation.x, cubeRotation.y, cubeRotation.z);
        return cubeCenter + rot * (vertex - cubeCenter);
    }

    // Draws a line between two points after applying the rotation.
    void DrawLine(Vector3 start, Vector3 end)
    {
        GL.Vertex(RotateVertex(start));
        GL.Vertex(RotateVertex(end));
    }

    // Draws the selected shape.
    void DrawShape()
    {
        switch (shapeType)
        {
            case ShapeType.Pyramid:
                DrawPyramid();
                break;
            case ShapeType.Cylinder:
                DrawCylinder();
                break;
            case ShapeType.Rectangle:
                DrawRectangle();
                break;
            case ShapeType.Sphere:
                DrawSphere();
                break;
            case ShapeType.Capsule:
                DrawCapsule();
                break;
        }
    }

    // Draws a pyramid with a square base.
    void DrawPyramid()
    {
        float half = pyramidBaseSize * 0.5f;
        Vector3 v0 = cubeCenter + new Vector3(-half, 0, -half);
        Vector3 v1 = cubeCenter + new Vector3(half, 0, -half);
        Vector3 v2 = cubeCenter + new Vector3(half, 0, half);
        Vector3 v3 = cubeCenter + new Vector3(-half, 0, half);
        Vector3 apex = cubeCenter + new Vector3(0, pyramidHeight, 0);

        // Base square
        DrawLine(v0, v1);
        DrawLine(v1, v2);
        DrawLine(v2, v3);
        DrawLine(v3, v0);

        // Edges from each base vertex to the apex
        DrawLine(v0, apex);
        DrawLine(v1, apex);
        DrawLine(v2, apex);
        DrawLine(v3, apex);
    }

    // Draws a cylinder oriented along the Y axis.
    void DrawCylinder()
    {
        Vector3 bottomCenter = cubeCenter - new Vector3(0, cylinderHeight / 2, 0);
        Vector3 topCenter = cubeCenter + new Vector3(0, cylinderHeight / 2, 0);
        float angleStep = 360f / cylinderSegments;
        List<Vector3> bottomPoints = new List<Vector3>();
        List<Vector3> topPoints = new List<Vector3>();

        for (int i = 0; i < cylinderSegments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            float x = Mathf.Cos(angle) * cylinderRadius;
            float z = Mathf.Sin(angle) * cylinderRadius;
            bottomPoints.Add(bottomCenter + new Vector3(x, 0, z));
            topPoints.Add(topCenter + new Vector3(x, 0, z));
        }

        // Draw bottom circle, top circle, and vertical edges.
        for (int i = 0; i < cylinderSegments; i++)
        {
            DrawLine(bottomPoints[i], bottomPoints[(i + 1) % cylinderSegments]);
            DrawLine(topPoints[i], topPoints[(i + 1) % cylinderSegments]);
            DrawLine(bottomPoints[i], topPoints[i]);
        }
    }

    // Draws a rectangle on the XZ plane.
    void DrawRectangle()
    {
        float halfWidth = rectangleWidth * 0.5f;
        float halfHeight = rectangleHeight * 0.5f;
        Vector3 v0 = cubeCenter + new Vector3(-halfWidth, 0, -halfHeight);
        Vector3 v1 = cubeCenter + new Vector3(halfWidth, 0, -halfHeight);
        Vector3 v2 = cubeCenter + new Vector3(halfWidth, 0, halfHeight);
        Vector3 v3 = cubeCenter + new Vector3(-halfWidth, 0, halfHeight);

        DrawLine(v0, v1);
        DrawLine(v1, v2);
        DrawLine(v2, v3);
        DrawLine(v3, v0);
    }

    // Draws a sphere using latitude and longitude lines.
    void DrawSphere()
    {
        int latSegments = sphereSegments;
        int lonSegments = sphereSegments;

        // Draw horizontal circles (latitudes)
        for (int i = 0; i <= latSegments; i++)
        {
            float lat = Mathf.Lerp(-90f, 90f, (float)i / latSegments);
            float radLat = Mathf.Deg2Rad * lat;
            float circleRadius = Mathf.Cos(radLat) * sphereRadius;
            float y = Mathf.Sin(radLat) * sphereRadius;
            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (int j = 0; j <= lonSegments; j++)
            {
                float lon = Mathf.Lerp(0, 360f, (float)j / lonSegments);
                float radLon = Mathf.Deg2Rad * lon;
                float x = Mathf.Cos(radLon) * circleRadius;
                float z = Mathf.Sin(radLon) * circleRadius;
                Vector3 point = cubeCenter + new Vector3(x, y, z);
                if (j > 0)
                    DrawLine(prevPoint, point);
                else
                    firstPoint = point;
                prevPoint = point;
            }
            DrawLine(prevPoint, firstPoint);
        }

        // Draw vertical circles (longitudes)
        for (int j = 0; j < lonSegments; j++)
        {
            float lon = Mathf.Lerp(0, 360f, (float)j / lonSegments);
            float radLon = Mathf.Deg2Rad * lon;
            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (int i = 0; i <= latSegments; i++)
            {
                float lat = Mathf.Lerp(-90f, 90f, (float)i / latSegments);
                float radLat = Mathf.Deg2Rad * lat;
                float y = Mathf.Sin(radLat) * sphereRadius;
                float r = Mathf.Cos(radLat) * sphereRadius;
                float x = Mathf.Cos(radLon) * r;
                float z = Mathf.Sin(radLon) * r;
                Vector3 point = cubeCenter + new Vector3(x, y, z);
                if (i > 0)
                    DrawLine(prevPoint, point);
                else
                    firstPoint = point;
                prevPoint = point;
            }
            DrawLine(prevPoint, firstPoint);
        }
    }

    // Draws a capsule combining a cylinder with hemispherical ends.
    void DrawCapsule()
    {
        // Cylinder portion along Y axis.
        Vector3 bottomCenter = cubeCenter - new Vector3(0, capsuleHeight / 2, 0);
        Vector3 topCenter = cubeCenter + new Vector3(0, capsuleHeight / 2, 0);
        int segments = capsuleSegments;
        float angleStep = 360f / segments;
        List<Vector3> bottomPoints = new List<Vector3>();
        List<Vector3> topPoints = new List<Vector3>();

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            float x = Mathf.Cos(angle) * capsuleRadius;
            float z = Mathf.Sin(angle) * capsuleRadius;
            bottomPoints.Add(bottomCenter + new Vector3(x, 0, z));
            topPoints.Add(topCenter + new Vector3(x, 0, z));
        }
        for (int i = 0; i < segments; i++)
        {
            DrawLine(bottomPoints[i], bottomPoints[(i + 1) % segments]);
            DrawLine(topPoints[i], topPoints[(i + 1) % segments]);
            DrawLine(bottomPoints[i], topPoints[i]);
        }

        // Top hemisphere (using horizontal slices)
        int hemiSteps = segments / 2;
        for (int i = 0; i <= hemiSteps; i++)
        {
            float lat = Mathf.Lerp(0, 90f, (float)i / hemiSteps);
            float radLat = Mathf.Deg2Rad * lat;
            float r = Mathf.Cos(radLat) * capsuleRadius;
            float yOffset = Mathf.Sin(radLat) * capsuleRadius;
            Vector3 centerPoint = topCenter + new Vector3(0, yOffset, 0);
            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (int j = 0; j <= segments; j++)
            {
                float lon = Mathf.Lerp(0, 360f, (float)j / segments);
                float radLon = Mathf.Deg2Rad * lon;
                float x = Mathf.Cos(radLon) * r;
                float z = Mathf.Sin(radLon) * r;
                Vector3 point = centerPoint + new Vector3(x, 0, z);
                if (j > 0)
                    DrawLine(prevPoint, point);
                else
                    firstPoint = point;
                prevPoint = point;
            }
            DrawLine(prevPoint, firstPoint);
        }

        // Bottom hemisphere
        for (int i = 0; i <= hemiSteps; i++)
        {
            float lat = Mathf.Lerp(0, 90f, (float)i / hemiSteps);
            float radLat = Mathf.Deg2Rad * lat;
            float r = Mathf.Cos(radLat) * capsuleRadius;
            float yOffset = Mathf.Sin(radLat) * capsuleRadius;
            Vector3 centerPoint = bottomCenter - new Vector3(0, yOffset, 0);
            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (int j = 0; j <= segments; j++)
            {
                float lon = Mathf.Lerp(0, 360f, (float)j / segments);
                float radLon = Mathf.Deg2Rad * lon;
                float x = Mathf.Cos(radLon) * r;
                float z = Mathf.Sin(radLon) * r;
                Vector3 point = centerPoint + new Vector3(x, 0, z);
                if (j > 0)
                    DrawLine(prevPoint, point);
                else
                    firstPoint = point;
                prevPoint = point;
            }
            DrawLine(prevPoint, firstPoint);
        }
    }
}
