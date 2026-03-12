using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameMode { idle, playing, levelEnd }

public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S;

    [Header("Inscribed")]
    public int shotsMax = 5;
    public Text uitLevel;
    public Text uitShots;
    public Vector3 castlePos;
    public GameObject[] castles;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Dynamic")]
    public int level;
    public int levelMax;
    public int shotsTaken;
    public int pendingShots;
    public GameObject castle;
    public GameMode mode = GameMode.idle;
    public string showing = "Show Slingshot";

    void Start()
    {
        S = this;

        level = 0;
        shotsTaken = 0;
        levelMax = castles.Length;
        StartLevel();
    }

    void StartLevel()
    {
        if (castle != null)
        {
            Destroy(castle);
        }

        Projectile.DESTROY_PROJECTILES();

        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        Goal.goalMet = false;
        shotsTaken = 0;
        pendingShots = 0;

        UpdateGUI();

        mode = GameMode.playing;

        FollowCam.SWITCH_VIEW(FollowCam.eView.both);
    }

    void UpdateGUI()
    {
        uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;
        uitShots.text = "Shots Taken: " + (shotsTaken + pendingShots) + " of " + shotsMax;
    }

    void Update()
    {
        UpdateGUI();

        if (mode == GameMode.playing && Goal.goalMet)
        {
            mode = GameMode.levelEnd;
            FollowCam.SWITCH_VIEW(FollowCam.eView.both);
            Invoke("NextLevel", 2f);
        }
        else if (mode == GameMode.playing && pendingShots == 0 && shotsTaken >= shotsMax)
        {
            mode = GameMode.levelEnd;
            FollowCam.SWITCH_VIEW(FollowCam.eView.both);
            Invoke("ShowGameOver", 2f);
        }
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        mode = GameMode.idle;
    }

    void NextLevel()
    {
        level++;
        
        if (level == levelMax)
        {
            winPanel.SetActive(true);
            mode = GameMode.idle;
            return;
        }

        StartLevel();
    }

    public void PlayAgain()
    {
        level = 0;
        shotsTaken = 0;
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        
        StartLevel();
    }

    static public void SHOT_FIRED()
    {
        S.pendingShots++;
    }

    static public void SHOT_SETTLED()
    {
        if (S.pendingShots > 0)
        {
            S.pendingShots--;
            S.shotsTaken++;
        }
    }

    static public bool SHOTS_REMAINING()
    {
        return (S.shotsTaken + S.pendingShots < S.shotsMax);
    }

    static public GameObject GET_CASTLE()
    {
        return S.castle;
    }
}