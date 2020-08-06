using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	// INITIALIZED FROM UNITY
	public Text scoreTitle;
	public Text scoreNumber;
	public Text tetraminoTitle;
	public Image tetraminoImage;
	public GameObject pauseScreen;
	public GameObject gameOverScreen;
	public Image gameOverBackground;
	public Text gameOverTitle;
	public Button gameOverRestartButton;
	public Text gameOverRestartText;
	public Button gameOverBackToMenuButton;
	public Text gameOverBackToMenuText;
	public AudioSource gameOverSoundBasic;
	public AudioSource gameOverSoundAdvanced;

	// INITIALIZE PARAMETERS
	public static bool IsAdvancedMode { get; set; }
	public static int Width { get; set; }
	public static int Height { get; set; }

	// TYPES
	private enum GameMode { Game, Pause, GameOver };
	delegate void Action();

	// CONSTANTS
	private const float BLOCK_SIZE = 50f;
	private const int BORDERS_COUNT = 2;
	private const float BORDERS_PADDING = BLOCK_SIZE / 4f;
	private const float FALL_TIME = 0.75f;
	private const float INCREASED_FALL_TIME = 0.075f;
	private const int LINE_COST = 100;
	private const int MAX_NUMBER_LENGTH = 5;
	private const float BACKGROUND_FADING_SPEED = 2f;
	private const float MIN_TITLE_SCAlE = 0.8f;
	private const float MAX_TITLE_SCAlE = 1f;
	private const float TITLE_SCALE_DIST = MAX_TITLE_SCAlE - MIN_TITLE_SCAlE;

	// CLASS FIELDS
	private static GameObject[,] field;
	private static GameObject[,] borders;
	private static Tetramino tetramino;
	private static Tetramino nextTetramino;
	private static float previousDownTime;
	private static float gameOverStartTime;
	private static int curScore;
	private float fixedSoundtrackVolume;
	private AudioSource backgroundSoundtrack;
	private GameMode gameMode;

	private void Awake()
	{
		gameMode = GameMode.Game;
		backgroundSoundtrack = GameObject.Find("BackgroundSoundtrack").GetComponent<AudioSource>();
		nextTetramino = new Tetramino();
		field = new GameObject[Height, Width];
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				field[i, j] = new GameObject("Field: (" + i.ToString() + ", " + j.ToString() + ")", typeof(SpriteRenderer));
				field[i, j].transform.position = new Vector3(j * BLOCK_SIZE, (Height - 1 - i) * BLOCK_SIZE, 0);
				Vector3 offset = new Vector3(Width * BLOCK_SIZE / 2 - BLOCK_SIZE / 2, Height * BLOCK_SIZE / 2 - BLOCK_SIZE / 2, 0);
				field[i, j].transform.position -= offset;
				SpriteRenderer renderer = field[i, j].GetComponent<SpriteRenderer>();
				renderer.sprite = Resources.Load<Sprite>("Sprites/block");
				field[i, j].SetActive(false);
			}
		}
		borders = new GameObject[Height, BORDERS_COUNT];
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < BORDERS_COUNT; j++)
			{
				borders[i, j] = new GameObject("Borders: (" + i.ToString() + ", " + j.ToString() + ")", typeof(SpriteRenderer));
				borders[i, j].transform.position = new Vector3(j * Width * BLOCK_SIZE, (Height - 1 - i) * BLOCK_SIZE, 0);
				Vector3 offset = new Vector3(Width * BLOCK_SIZE / 2, Height * BLOCK_SIZE / 2 - BLOCK_SIZE / 2, 0);
				borders[i, j].transform.position -= offset;
				borders[i, j].transform.position += (j == 0 ? -1 : 1) * new Vector3(BORDERS_PADDING, 0, 0);
				SpriteRenderer renderer = borders[i, j].GetComponent<SpriteRenderer>();
				renderer.sprite = Resources.Load<Sprite>("Sprites/border");
			}
		}
		NewTetramino();
		tetramino.SetActive(field, true);
	}

	private void Update()
	{
		if (gameMode == GameMode.Game)
		{
			GameUpdate();
		}
		else if (gameMode == GameMode.Pause)
		{
			PauseUpdate();
		}
		else if (gameMode == GameMode.GameOver)
		{
			if (!IsAdvancedMode)
				GameOverBasicUpdate();
			else
				GameOverAdvancedUpdate();
		}
	}

	private void NewTetramino()
	{
		tetramino = nextTetramino;
		nextTetramino = new Tetramino();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < BORDERS_COUNT; j++)
			{
				float H, S, V;
				Color.RGBToHSV(tetramino.Color, out H, out S, out V);
				S = (float)(i + 1) / Height;
				SpriteRenderer renderer = borders[i, j].GetComponent<SpriteRenderer>();
				renderer.color = Color.HSVToRGB(H, S, V);
			}
		}
		scoreTitle.color = tetramino.Color;
		scoreNumber.color = tetramino.Color;
		tetraminoTitle.color = nextTetramino.Color;
		tetraminoImage.color = nextTetramino.Color;
		tetraminoImage.sprite = Resources.Load<Sprite>("TetraminoImages/tetramino_" + nextTetramino.Id.ToString());
	}

	private void GameOver()
	{
		gameMode = GameMode.GameOver;
		fixedSoundtrackVolume = backgroundSoundtrack.volume;
		if (!IsAdvancedMode)
		{
			gameOverScreen.SetActive(true);
			gameOverSoundBasic.Play();
			gameOverTitle.gameObject.SetActive(true);
			gameOverTitle.font = Resources.Load<Font>("Fonts/kenney_future_narrow");
			gameOverTitle.text = "Game over";
			gameOverTitle.fontSize = 125;
			gameOverTitle.color = new Color(1f, 1f, 1f, 0f);
		}
		else
		{
			gameOverScreen.SetActive(true);
			gameOverSoundAdvanced.Play();
			gameOverTitle.gameObject.SetActive(false);
			gameOverTitle.font = Resources.Load<Font>("Fonts/optimus_princeps");
			gameOverTitle.text = "YOU DIED";
			gameOverTitle.fontSize = 150;
			gameOverTitle.color = new Color(0.6f, 0.15f, 0.15f);
		}
		gameOverStartTime = Time.time;
	}

	private void GameUpdate()
	{
		Action moveLeft = delegate () { tetramino.MoveLeft(); };
		Action moveRight = delegate () { tetramino.MoveRight(); };
		Action rotate90 = delegate () { tetramino.Rotate(); };
		Action rotate270 = delegate () { tetramino.Rotate(); tetramino.Rotate(); tetramino.Rotate(); };
		const int ACTIONS_COUNT = 3;
		Action[] actions = { moveLeft, moveRight, rotate90 };
		Action[] backActions = { moveRight, moveLeft, rotate270 };
		KeyCode[] keyCodes = { KeyCode.A, KeyCode.D, KeyCode.W };
		tetramino.SetActive(field, false);
		for (int i = 0; i < ACTIONS_COUNT; i++)
		{
			if (Input.GetKeyDown(keyCodes[i]))
			{
				actions[i]();
				if (!tetramino.Check(field))
					backActions[i]();
			}
		}
		if ((Input.GetKey(KeyCode.S) ? INCREASED_FALL_TIME : FALL_TIME) < Time.time - previousDownTime)
		{
			tetramino.MoveDown();
			if (!tetramino.Check(field))
			{
				tetramino.MoveUp();
				tetramino.SetActive(field, true);
				CheckLineToDelete();
				NewTetramino();
				if (!tetramino.Check(field))
				{
					GameOver();
					return;
				}
			}
			previousDownTime = Time.time;
		}
		tetramino.SetActive(field, true);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			gameMode = GameMode.Pause;
			pauseScreen.SetActive(true);
		}
	}

	private void PauseUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			gameMode = GameMode.Game;
			pauseScreen.SetActive(false);
		}
	}

	private void GameOverBasicUpdate()
	{
		Sequencer sequencer = new Sequencer();
		Sequencer.Step step1 = (float progress) =>
		{
			gameOverRestartButton.interactable = false;
			gameOverBackToMenuButton.interactable = false;
			backgroundSoundtrack.volume = fixedSoundtrackVolume / 4;
		};
		Sequencer.Step step2 = (float progress) =>
		{
			gameOverBackground.color = Utilities.SetAlpha(gameOverBackground.color, progress);
			gameOverTitle.color = Utilities.SetAlpha(gameOverTitle.color, progress);
			gameOverBackToMenuText.color = Utilities.SetAlpha(gameOverBackToMenuText.color, progress);
			gameOverRestartText.color = Utilities.SetAlpha(gameOverRestartText.color, progress);
		};
		Sequencer.Step flatSet = (float progress) =>
		{
			gameOverRestartButton.interactable = true;
			gameOverBackToMenuButton.interactable = true;
			backgroundSoundtrack.volume = fixedSoundtrackVolume;

			gameOverBackground.color = Utilities.SetAlpha(gameOverBackground.color, 1f);
			gameOverTitle.color = Utilities.SetAlpha(gameOverTitle.color, 1f);
			gameOverBackToMenuText.color = Utilities.SetAlpha(gameOverBackToMenuText.color, 1f);
			gameOverRestartText.color = Utilities.SetAlpha(gameOverRestartText.color, 1f);
		};
		sequencer.SetSteps(step1, step2, flatSet);
		sequencer.SetStepsDuration(1f, 1f);
		sequencer.Invoke(Time.time - gameOverStartTime);
	}

	private void GameOverAdvancedUpdate()
	{
		Sequencer sequencer = new Sequencer();
		Sequencer.Step step1 = (float progress) =>
		{
			gameOverRestartButton.interactable = false;
			gameOverBackToMenuButton.interactable = false;
			backgroundSoundtrack.volume = fixedSoundtrackVolume / 4;
		};
		Sequencer.Step step2 = (float progress) =>
		{
			gameOverTitle.gameObject.SetActive(true);
			float curScale = MIN_TITLE_SCAlE + progress * TITLE_SCALE_DIST;
			gameOverTitle.transform.localScale = new Vector3(curScale, curScale, 1f);
			gameOverTitle.color = Utilities.SetAlpha(gameOverTitle.color, progress);
			gameOverBackground.color = Utilities.SetAlpha(gameOverBackground.color, Mathf.Min(progress * BACKGROUND_FADING_SPEED, 1f));
		};
		Sequencer.Step step3 = (float progress) =>
		{
			gameOverTitle.color = Utilities.SetAlpha(gameOverTitle.color, 1f - progress);
		};
		Sequencer.Step step4 = (float progress) =>
		{
			gameOverBackToMenuText.color = Utilities.SetAlpha(gameOverBackToMenuText.color, progress);
			gameOverRestartText.color = Utilities.SetAlpha(gameOverRestartText.color, progress);
		};
		Sequencer.Step flatSet = (float progress) =>
		{
			gameOverRestartButton.interactable = true;
			gameOverBackToMenuButton.interactable = true;
			backgroundSoundtrack.volume = fixedSoundtrackVolume;
			gameOverTitle.transform.localScale = new Vector3(MAX_TITLE_SCAlE, MAX_TITLE_SCAlE, 1f);
			gameOverTitle.color = Utilities.SetAlpha(gameOverTitle.color, 0f);
			gameOverBackground.color = Utilities.SetAlpha(gameOverBackground.color, 1f);
			gameOverBackToMenuText.color = Utilities.SetAlpha(gameOverBackToMenuText.color, 1f);
			gameOverRestartText.color = Utilities.SetAlpha(gameOverRestartText.color, 1f);
		};
		sequencer.SetSteps(step1, step2, step3, step4, flatSet);
		sequencer.SetStepsDuration(1f, 4.5f, 0.5f, 0.5f);
		sequencer.Invoke(Time.time - gameOverStartTime);
	}

	private void CheckLineToDelete()
	{
		bool[] completes = new bool[Height];
		for (int i = 0; i < Height; i++)
		{
			bool isAllActive = true;
			for (int j = 0; j < Width; j++)
			{
				if (!field[i, j].activeSelf)
				{
					isAllActive = false;
					break;
				}
			}
			completes[i] = isAllActive;
		}
		for (int i = 0; i < Height; i++)
		{
			if (!IsAdvancedMode)
			{
				if (completes[i])
					DeleteLine(i);
			}
			else
			{
				if (i + 1 < Height && completes[i] && completes[i + 1])
				{
					while (i < Height && completes[i])
						DeleteLine(i++);
				}
			}
		}
	}

	private void DeleteLine(int index)
	{
		for (int i = index; i > 0; i--)
		{
			for (int j = 0; j < Width; j++)
			{
				field[i, j].SetActive(field[i - 1, j].activeSelf);
				SpriteRenderer renderer1 = field[i, j].GetComponent<SpriteRenderer>();
				SpriteRenderer renderer2 = field[i - 1, j].GetComponent<SpriteRenderer>();
				renderer1.color = renderer2.color;
			}
		}
		for (int i = 0; i < Width; i++)
			field[0, i].SetActive(false);
		curScore += LINE_COST;
		scoreNumber.text = curScore.ToString("D" + MAX_NUMBER_LENGTH.ToString());
	}

	public void BackToMenu()
	{
		SceneManager.LoadScene("Menu");
	}

	public void Restart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
