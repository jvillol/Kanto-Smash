using UnityEngine;

namespace KantoSmash
{
    /// <summary>
    /// Gestor central de la partida de Kanto Smash:
    /// - Controla el límite de 5 minutos (300 segundos)
    /// - Controla las 3 vidas de cada luchador
    /// - Determina el ganador por KO (dejar al rival con 0 vidas) o por tiempo (quien más vidas tenga, o empate si ambos tienen 2 vidas / mismas vidas)
    /// </summary>
    public class SmashGameManager : MonoBehaviour
    {
        public static SmashGameManager Instance { get; private set; }

        [Header("Selección de Personajes y Dificultad")]
        public PokemonData selectedPlayerPokemon;
        public PokemonData selectedAIPokemon;
        public AIDifficulty currentDifficulty = AIDifficulty.Medium;

        [Header("Estado de la Partida")]
        public bool IsMatchActive { get; private set; } = false;
        public float matchRemainingSeconds = 300f; // 5 minutos exactos
        public const float MaxMatchSeconds = 300f;

        public FighterController playerFighter;
        public FighterController aiFighter;
        private GameObject _currentStage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            var all = KantoPokemonDatabase.GetAllPokemon();
            selectedPlayerPokemon = all[24]; // #025 Pikachu por defecto
            selectedAIPokemon = all[5];      // #006 Charizard por defecto
        }

        private void Update()
        {
            if (!IsMatchActive) return;

            matchRemainingSeconds -= Time.deltaTime;
            if (matchRemainingSeconds <= 0f)
            {
                matchRemainingSeconds = 0f;
                EvaluateMatchEndByTimeOut();
            }

            if (SmashUIManager.Instance != null)
            {
                SmashUIManager.Instance.RefreshInGameHUD();
            }
        }

        public void StartMatch()
        {
            CleanupCurrentMatchObjects();

            _currentStage = SmashStageBuilder.BuildBattleStage();

            // Crear Luchador del Jugador 1
            GameObject p1Obj = new GameObject($"Fighter_Player_{selectedPlayerPokemon.name}");
            playerFighter = p1Obj.AddComponent<FighterController>();
            playerFighter.InitializeFighter(selectedPlayerPokemon, controlledByAI: false, spawnPos: new Vector2(-3.0f, 0.5f), initialFacing: 1);

            // Crear Luchador Rival (IA)
            GameObject aiObj = new GameObject($"Fighter_AI_{selectedAIPokemon.name}");
            aiFighter = aiObj.AddComponent<FighterController>();
            aiFighter.InitializeFighter(selectedAIPokemon, controlledByAI: true, spawnPos: new Vector2(3.0f, 0.5f), initialFacing: -1);

            AIFighterBrain brain = aiObj.AddComponent<AIFighterBrain>();
            brain.targetPlayer = playerFighter;
            brain.difficulty = currentDifficulty;

            matchRemainingSeconds = MaxMatchSeconds;
            IsMatchActive = true;

            if (SmashUIManager.Instance != null)
            {
                SmashUIManager.Instance.ShowInGameHUD();
            }
        }

        public void OnFighterRingOut(FighterController fallenFighter)
        {
            if (!IsMatchActive) return;

            fallenFighter.lives = Mathf.Max(0, fallenFighter.lives - 1);

            if (fallenFighter.lives <= 0)
            {
                // Rival con 0 vidas -> Victoria inmediata del superviviente
                IsMatchActive = false;
                bool playerWon = (fallenFighter == aiFighter);
                string winnerTitle = "¡GANADOR!";
                string subtitle = playerWon
                    ? $"JUGADOR 1 ({playerFighter.pokemonData.name.ToUpper()})"
                    : $"RIVAL IA ({aiFighter.pokemonData.name.ToUpper()})";
                string detail = playerWon
                    ? "¡Has dejado al rival con 0 vidas!"
                    : "¡Te has quedado sin vidas!";

                SmashUIManager.Instance?.ShowMatchResult(winnerTitle, subtitle, detail);
            }
            else
            {
                Vector2 respawnPos = fallenFighter.isAI ? new Vector2(2.5f, 3.2f) : new Vector2(-2.5f, 3.2f);
                fallenFighter.RespawnAt(respawnPos);
            }
        }

        private void EvaluateMatchEndByTimeOut()
        {
            IsMatchActive = false;

            int p1Lives = playerFighter != null ? playerFighter.lives : 0;
            int aiLives = aiFighter != null ? aiFighter.lives : 0;

            // Regla: Cuando acaben los 5 minutos, gana quien más vidas tenga y hay empate si ambos tienen 2 vidas (o las mismas vidas)
            if (p1Lives > aiLives)
            {
                SmashUIManager.Instance?.ShowMatchResult(
                    "¡GANADOR!",
                    $"JUGADOR 1 ({playerFighter.pokemonData.name.ToUpper()})",
                    $"Fin del tiempo (5:00) • Ganaste por tener más vidas ({p1Lives} vs {aiLives})"
                );
            }
            else if (aiLives > p1Lives)
            {
                SmashUIManager.Instance?.ShowMatchResult(
                    "¡GANADOR!",
                    $"RIVAL IA ({aiFighter.pokemonData.name.ToUpper()})",
                    $"Fin del tiempo (5:00) • La IA ganó por tener más vidas ({aiLives} vs {p1Lives})"
                );
            }
            else
            {
                SmashUIManager.Instance?.ShowMatchResult(
                    "¡EMPATE!",
                    $"{playerFighter.pokemonData.name.ToUpper()} vs {aiFighter.pokemonData.name.ToUpper()}",
                    $"Se agotaron los 5 minutos y ambos tenéis {p1Lives} vidas."
                );
            }
        }

        public void ReturnToCharacterSelect()
        {
            IsMatchActive = false;
            CleanupCurrentMatchObjects();
            SmashUIManager.Instance?.ShowCharacterSelectScreen();
        }

        private void CleanupCurrentMatchObjects()
        {
            if (playerFighter != null) Destroy(playerFighter.gameObject);
            if (aiFighter != null) Destroy(aiFighter.gameObject);
            if (_currentStage != null) Destroy(_currentStage);
        }
    }
}

