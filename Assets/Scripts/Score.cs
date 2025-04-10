using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _speedVariation = 1f;
    [SerializeField] private List<Vector3> _spawnPos;

    [HideInInspector]
    public int ColorId;

    [HideInInspector]
    public Score NextScore;

    private void Awake()
    {
        hasGameFinished = false;
        transform.position = _spawnPos[Random.Range(0,_spawnPos.Count)];
        int colorCount = GameplayManager.Instance.Colors.Count;
        ColorId = Random.Range(0, colorCount);
        GetComponent<SpriteRenderer>().color = GameplayManager.Instance.Colors[ColorId];

        float speedIncrement = GameplayManager.Instance.SpeedIncrement;
        float maxSpeed = GameplayManager.Instance.MaxSpeed;
        _moveSpeed = Mathf.Min(_moveSpeed + speedIncrement, maxSpeed);
        _moveSpeed = _moveSpeed + Random.Range(-_speedVariation, _speedVariation);
        //Debug.Log("Speed: " + _moveSpeed);  
    }

    private void FixedUpdate()
    {
        if (hasGameFinished) return;
        transform.Translate(_moveSpeed * Time.fixedDeltaTime * Vector3.down);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Obstacle"))
        {
            GameplayManager.Instance.GameEnded();
        }
        else if (collision.CompareTag("Block"))
        {
            Player block = collision.GetComponent<Player>();
            if (block != null && block.ColorId == ColorId)
            {
                // The block and score have the same color, proceed with the game logic
                GameplayManager.Instance.IncreaseScore(ColorId, transform.position);
                Destroy(gameObject);
                Destroy(block.gameObject);
            }
            else
            {
                // The block and score have different colors, end the game
                block.StopMoving();
                GameplayManager.Instance.GameEnded();
            }
        }
    }

    private void OnEnable()
    {
        GameplayManager.Instance.GameEnd += GameEnded;
    }

    private void OnDisable()
    {
        GameplayManager.Instance.GameEnd -= GameEnded;
    }

    private bool hasGameFinished;

    private void GameEnded()
    {
        hasGameFinished = true;
    }
}
