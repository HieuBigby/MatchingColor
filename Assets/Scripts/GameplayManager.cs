using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour
{
    #region START

    private bool hasGameFinished;

    public static GameplayManager Instance;

    public List<Color> Colors;
    public Player blockPrefab;
    public Transform transHolder;
    public List<Player> blocks = new List<Player>();
    public float blockSpacing = 0.5f;

    private void Awake()
    {
        Instance = this;

        hasGameFinished = false;
        GameManager.Instance.IsInitialized = true;

        score = 0;
        _scoreText.text = ((int)score).ToString();
        DistributeBlocks(false);
        StartCoroutine(SpawnScore());
    }

    public void DistributeBlocks(bool withAnim)
    {
        if (blocks.Count == 0) return;

        // Calculate the total width of all blocks plus spacing
        float totalWidth = blocks.Sum(block => block.Size.x) + (blocks.Count - 1) * blockSpacing;

        // Calculate the screen width more accurately
        float screenWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;

        // Calculate the starting position to center the blocks on the screen
        float startX = -totalWidth / 2f;

        // Position each block
        float currentX = startX;
        foreach (var block in blocks)
        {
            float blockWidth = block.Size.x;
            Vector3 targetPosition = new Vector3(
                currentX + blockWidth / 2f,
                block.transform.position.y,
                block.transform.position.z
            );

            if (withAnim)
            {
                if (block.IsDragging == false)
                {
                    // Animate the block movement with DOTween
                    block.transform
                    .DOMove(targetPosition, 0.2f) // 0.3 seconds animation duration
                    .SetEase(Ease.OutSine); // Smooth easing effect
                }
            }
            else
            {
                // Instant positioning without animation
                block.transform.position = targetPosition;
            }

            block.OriginalPosition = new Vector3(targetPosition.x, transHolder.position.y, 0f); // Update the original position

            // Move to the next block's position, including spacing
            currentX += blockWidth + blockSpacing;
        }
    }

    public void RegisterBlock(Player block)
    {
        blocks.Add(block);
    }

    public void UnregisterBlock(Player block)
    {
        int removedIndex = blocks.IndexOf(block);
        blocks.Remove(block);
        Player newBlock = Instantiate(blockPrefab, transHolder);
        Vector3 spawnPosition = new Vector3(
            Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect,
            transHolder.position.y,
            transHolder.position.z
        );
        newBlock.transform.position = spawnPosition;
        newBlock.SetColor(block.ColorId);
        blocks.Insert(0, newBlock);
        DistributeBlocks(true);
    }

    public Player GetOverlappedBlock(Player draggingBlock)
    {
        foreach (Player block in blocks)
        {
            if (block != draggingBlock && Vector3.Distance(draggingBlock.Position, block.Position) < 1f) // Adjust the distance threshold as needed
            {
                return block;
            }
        }
        return null;
    }

    public void SwapBlocks(Player block1, Player block2)
    {
        int index1 = blocks.IndexOf(block1);
        int index2 = blocks.IndexOf(block2);

        if (index1 != -1 && index2 != -1)
        {
            blocks[index1] = block2;
            blocks[index2] = block1;
        }

        DistributeBlocks(true);
    }

    #endregion

    #region GAME_LOGIC

    [SerializeField] private ScoreEffect _scoreEffect;

    #endregion

    #region SCORE

    private float score;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private AudioClip _pointClip;

    public void IncreaseScore(int color, Vector3 position)
    {
        var t = Instantiate(_scoreEffect, position, Quaternion.identity);
        t.Init(Colors[color]);

        UpdateScore();
    }

    private void UpdateScore()
    {
        score++;
        SoundManager.Instance.PlaySound(_pointClip);
        _scoreText.text = ((int)score).ToString();
    }

    [SerializeField] private float _spawnTime;
    [SerializeField] private Score _scorePrefab;
    private Score CurrentScore;

    private IEnumerator SpawnScore()
    {
        Score prevScore = null;

        while(!hasGameFinished)
        {
            var tempScore = Instantiate(_scorePrefab);

            if(prevScore == null)
            {
                prevScore = tempScore;
                CurrentScore = prevScore;
            }
            else
            {
                prevScore.NextScore = tempScore;
                prevScore = tempScore;
            }

            yield return new WaitForSeconds(_spawnTime);
        }
    }

    #endregion

    #region GAME_OVER

    [SerializeField] private AudioClip _loseClip;
    public UnityAction GameEnd;

    public void GameEnded()
    {
        //Debug.LogError("Game End");
        hasGameFinished = true;
        GameEnd?.Invoke();
        SoundManager.Instance.PlaySound(_loseClip);
        GameManager.Instance.CurrentScore = (int)score;
        StartCoroutine(GameOver());
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.GoToMainMenu();
    }

    #endregion
}
