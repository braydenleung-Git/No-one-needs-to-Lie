using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfVision : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Alert Settings")]
    public float yellowAlertTime = 2f;
    public float redAlertTime = 2f;
    public float chaseTime = 3f;

    public enum AlertState { None, Yellow, Red, Chasing }
    public AlertState currentState = AlertState.None;

    private Mesh mesh;
    private float alertTimer = 0f;
    private bool playerInSight = false;
    private Transform player;
    private IPatroller patrol;
    private MeshRenderer meshRenderer;
    private Material coneMaterial;
    private Owner_Patrol ownerPatrol;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();

        coneMaterial = new Material(Shader.Find("Sprites/Default"));
        coneMaterial.color = new Color(1f, 1f, 1f, 0.15f);
        meshRenderer.material = coneMaterial;

        patrol = GetComponentInParent<IPatroller>();
        ownerPatrol = GetComponentInParent<Owner_Patrol>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void LateUpdate()
    {
        UpdateConeRotation();
        DrawCone();
        CheckVision();
        HandleAlertState();
        UpdateConeColor();
    }

    private void UpdateConeRotation()
    {
        if (ownerPatrol != null && ownerPatrol.currentDirection != Vector2.zero)
        {
            float angle = UtilsClass.GetAngleFromVectorFloat(ownerPatrol.currentDirection);
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    private void DrawCone()
    {
        int rayCount = 50;
        float angle = UtilsClass.GetAngleFromVectorFloat(transform.up) + viewAngle / 2f;
        float angleIncrease = viewAngle / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                UtilsClass.GetVectorFromAngle(angle),
                viewRadius,
                obstacleMask
            );

            if (hit.collider == null)
                vertex = transform.InverseTransformPoint(
                    transform.position + UtilsClass.GetVectorFromAngle(angle) * viewRadius);
            else
                vertex = transform.InverseTransformPoint(hit.point);

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
    }

    private void CheckVision()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= viewRadius)
        {
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            float angleTo = Vector2.Angle(transform.up, dirToPlayer);

            if (angleTo < viewAngle / 2f)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    dirToPlayer,
                    distToPlayer,
                    obstacleMask
                );
                playerInSight = hit.collider == null;
                return;
            }
        }

        playerInSight = false;
    }

    private void HandleAlertState()
    {
        if (playerInSight)
        {
            alertTimer += Time.deltaTime;

            if (alertTimer >= yellowAlertTime + redAlertTime && currentState != AlertState.Chasing)
            {
                currentState = AlertState.Chasing;
                patrol.StartChase(player);
                StartCoroutine(ChaseTimeout());
            }
            else if (alertTimer >= yellowAlertTime && currentState == AlertState.Yellow)
            {
                currentState = AlertState.Red;
            }
            else if (currentState == AlertState.None)
            {
                currentState = AlertState.Yellow;
            }
        }
        else
        {
            if (currentState != AlertState.Chasing)
            {
                alertTimer = 0f;
                currentState = AlertState.None;
            }
        }
    }

    private void UpdateConeColor()
    {
        switch (currentState)
        {
            case AlertState.None:
                coneMaterial.color = new Color(1f, 1f, 1f, 0.15f);
                break;
            case AlertState.Yellow:
                coneMaterial.color = new Color(1f, 1f, 0f, 0.3f);
                break;
            case AlertState.Red:
            case AlertState.Chasing:
                coneMaterial.color = new Color(1f, 0f, 0f, 0.4f);
                break;
        }
    }

    IEnumerator ChaseTimeout()
    {
        yield return new WaitForSeconds(chaseTime);
        currentState = AlertState.None;
        alertTimer = 0f;
        patrol.StopChase();
    }
}