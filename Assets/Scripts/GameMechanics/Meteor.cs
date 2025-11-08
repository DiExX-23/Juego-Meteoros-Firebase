// Meteor.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Meteor : MonoBehaviour
{
    public float directFallSpeed = 0f;
    public float lifetimeSeconds = 10f;
    public float despawnY = -6f;
    public int dodgePoints = 1;

    Rigidbody body;
    bool collidedWithPlayer;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (directFallSpeed > 0f) body.isKinematic = true;
        Destroy(gameObject, lifetimeSeconds);
    }

    void FixedUpdate()
    {
        if (directFallSpeed > 0f) transform.position += Vector3.down * directFallSpeed * Time.fixedDeltaTime;
        if (!collidedWithPlayer && transform.position.y <= despawnY)
        {
            ScoreManager.Instance?.AddPoints(dodgePoints);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collidedWithPlayer = true;
            GameManager.Instance.HandlePlayerHit();
        }
        Destroy(gameObject);
    }
}