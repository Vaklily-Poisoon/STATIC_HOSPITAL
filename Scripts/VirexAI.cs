// VirusAI.cs content goes here
using UnityEngine;

public class VirexAI : MonoBehaviour
{
    public Transform player;
    public float visionRange = 12f;
    public float attackRange = 2f;
    public float moveSpeed = 4f;

    private string state = "Idle";
    private float searchTimer = 0f;
    private float maxSearchTime = 3f;

    private Vector3 idleTarget;

    void Start()
    {
        PickNewIdlePoint();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case "Idle":
                IdleBehaviour();
                if (distance < visionRange)
                    state = "Chasing";
                break;

            case "Chasing":
                ChaseBehaviour();
                if (distance < attackRange)
                    state = "Attacking";
                if (distance > visionRange)
                {
                    state = "Searching";
                    searchTimer = maxSearchTime;
                }
                break;

            case "Attacking":
                AttackBehaviour();
                if (distance > attackRange)
                    state = "Chasing";
                break;

            case "Searching":
                SearchBehaviour();
                searchTimer -= Time.deltaTime;

                if (distance < visionRange)
                    state = "Chasing";

                if (searchTimer <= 0)
                    state = "Idle";
                break;
        }
    }

    // -------------------------
    // STATE FUNCTIONS
    // -------------------------

    void IdleBehaviour()
    {
        MoveTowards(idleTarget);

        if (Vector3.Distance(transform.position, idleTarget) < 1f)
            PickNewIdlePoint();
    }

    void ChaseBehaviour()
    {
        MoveTowards(player.position);
        Face(player.position);
    }

    void AttackBehaviour()
    {
        Face(player.position);
        // Put your animation or damage code here
        Debug.Log("Virex attacks!");
    }

    void SearchBehaviour()
    {
        // Look around or rotate slowly
        transform.Rotate(0, 60 * Time.deltaTime, 0);
    }

    // -------------------------
    // HELPERS
    // -------------------------

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void Face(Vector3 target)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    void PickNewIdlePoint()
    {
        idleTarget = transform.position + new Vector3(
            Random.Range(-4f, 4f),
            0,
            Random.Range(-4f, 4f)
        );
    }
}
