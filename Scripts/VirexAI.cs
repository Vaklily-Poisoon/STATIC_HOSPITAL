using UnityEngine;

// =======================
// PLAYER STATS + FEAR
// =======================
public class PlayerStats : MonoBehaviour
{
    public float fear = 0f;
    public float maxFear = 100f;
    public bool inSafeZone = false;

    public float fearIncreaseRate = 5f;
    public float fearDecreaseRate = 2f;

    void Update()
    {
        if (!inSafeZone)
        {
            fear += fearIncreaseRate * Time.deltaTime;
        }
        else
        {
            fear -= fearDecreaseRate * Time.deltaTime;
        }

        fear = Mathf.Clamp(fear, 0, maxFear);
    }
}

// =======================
// VIREX AI
// =======================
public class VirexAI : MonoBehaviour
{
    public Transform player;
    public PlayerStats playerStats;

    public float baseVisionRange = 10f;
    public float baseAttackRange = 2f;
    public float baseMoveSpeed = 3f;

    public float visionRange;
    public float attackRange;
    public float moveSpeed;

    private string state = "Idle";
    private float searchTimer = 0f;
    private float maxSearchTime = 3f;

    private Vector3 idleTarget;

    void Start()
    {
        visionRange = baseVisionRange;
        attackRange = baseAttackRange;
        moveSpeed = baseMoveSpeed;

        PickNewIdlePoint();
    }

    void Update()
    {
        // Scale stats based on fear
        float fear = playerStats.fear;
        visionRange = baseVisionRange + (fear / 10f);   // more fear = more vision
        moveSpeed = baseMoveSpeed + (fear / 30f);       // more fear = faster

        float distance = Vector3.Distance(transform.position, player.position);

        // If fear is max, force Virex into chase
        if (playerStats.fear >= playerStats.maxFear && state != "Attacking")
        {
            state = "Chasing";
        }

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

    // --------- STATES ---------

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
        Debug.Log("Virex attacks!");
        // put damage / animation here
    }

    void SearchBehaviour()
    {
        transform.Rotate(0, 60 * Time.deltaTime, 0);
    }

    // --------- HELPERS ---------

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

// =======================
// SAFE ZONE
// =======================
public class SafeZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null) ps.inSafeZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null) ps.inSafeZone = false;
        }
    }
}

// =======================
// EXTRA FEAR ZONE (OPTIONAL)
// =======================
public class FearZone : MonoBehaviour
{
    public float extraFearRate = 10f;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.fear += extraFearRate * Time.deltaTime;
                ps.fear = Mathf.Clamp(ps.fear, 0, ps.maxFear);
            }
        }
    }
}
