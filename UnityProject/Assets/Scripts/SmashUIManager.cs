using System.Collections.Generic;
using UnityEngine;

namespace KantoSmash
{
    /// <summary>
    /// Interfaz de usuario completa en Unity para Kanto Smash:
    /// - Selección de Personajes (151 Pokémon de Kanto + botón Al Azar, selección de Jugador e IA, Confirmar y "¡LUCHAR!")
    /// - HUD en partida (Temporizador 05:00, 3 vidas, porcentaje de acumulación de daño)
    /// - Rótulo de "¡GANADOR!" / "¡EMPATE!" con botones "Volver a jugar" y "Volver a la selección de personajes"
    /// </summary>
    public class SmashUIManager : MonoBehaviour
    {
        public static SmashUIManager Instance { get; private set; }

        private enum ScreenMode { CharacterSelect, InGameHUD, ResultScreen }
        private ScreenMode _currentScreen = ScreenMode.CharacterSelect;

        private bool _selectingForPlayer = true;
        private bool _selectionConfirmed = false;
        private Vector2 _rosterScrollPos = Vector2.zero;
        private string _searchQuery = "";

        private string _resultBanner = "¡GANADOR!";
        private string _resultSubtitle = "";
        private string _resultDetail = "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ShowCharacterSelectScreen()
        {
            _selectionConfirmed = false;
            _currentScreen = ScreenMode.CharacterSelect;
        }

        public void ShowInGameHUD()
        {
            _currentScreen = ScreenMode.InGameHUD;
        }

        public void RefreshInGameHUD()
        {
            // El renderizado de OnGUI se actualiza cada frame automáticamente
        }

        public void ShowMatchResult(string banner, string subtitle, string detail)
        {
            _resultBanner = banner;
            _resultSubtitle = subtitle;
            _resultDetail = detail;
            _currentScreen = ScreenMode.ResultScreen;
        }

        private void OnGUI()
        {
            if (SmashGameManager.Instance == null) return;

            switch (_currentScreen)
            {
                case ScreenMode.CharacterSelect:
                    DrawCharacterSelectGUI();
                    break;
                case ScreenMode.InGameHUD:
                    DrawInGameHUDGUI();
                    break;
                case ScreenMode.ResultScreen:
                    DrawInGameHUDGUI();
                    DrawResultOverlayGUI();
                    break;
            }
        }

        private void DrawCharacterSelectGUI()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.84f, 0.1f);

            GUI.Box(new Rect(15, 10, Screen.width - 30, Screen.height - 20), "");
            GUI.Label(new Rect(20, 16, Screen.width - 40, 36), "KANTO SMASH - SELECCIÓN DE POKÉMON (151 KANTO)", titleStyle);

            // Selector de turno (Jugador 1 vs Rival IA)
            GUI.backgroundColor = _selectingForPlayer ? new Color(1f, 0.25f, 0.35f) : Color.gray;
            if (GUI.Button(new Rect(30, 58, 260, 34), "Eligiendo: JUGADOR 1 (TÚ)"))
            {
                _selectingForPlayer = true;
            }

            GUI.backgroundColor = !_selectingForPlayer ? new Color(0.2f, 0.65f, 1f) : Color.gray;
            if (GUI.Button(new Rect(300, 58, 260, 34), "Eligiendo: RIVAL (IA)"))
            {
                _selectingForPlayer = false;
            }

            // Buscador
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(580, 64, 70, 25), "Buscar:");
            _searchQuery = GUI.TextField(new Rect(645, 62, 220, 26), _searchQuery);

            // Cuadrícula de los 151 Pokémon + Botón Al Azar
            Rect scrollViewRect = new Rect(30, 100, Screen.width - 60, Screen.height - 250);
            List<PokemonData> allPokemon = KantoPokemonDatabase.GetAllPokemon();
            List<PokemonData> filtered = new List<PokemonData>();

            foreach (var p in allPokemon)
            {
                if (string.IsNullOrEmpty(_searchQuery) ||
                    p.name.ToLowerInvariant().Contains(_searchQuery.ToLowerInvariant()) ||
                    p.FormattedNumber.Contains(_searchQuery))
                {
                    filtered.Add(p);
                }
            }

            int cols = Mathf.Max(4, (int)((scrollViewRect.width - 30) / 135));
            int totalItems = filtered.Count + 1; // +1 para el botón AL AZAR
            int rows = Mathf.CeilToInt(totalItems / (float)cols);
            Rect contentRect = new Rect(0, 0, scrollViewRect.width - 25, rows * 68 + 10);

            _rosterScrollPos = GUI.BeginScrollView(scrollViewRect, _rosterScrollPos, contentRect);

            // Casilla #0: AL AZAR
            GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
            if (GUI.Button(new Rect(6, 6, 128, 60), "🎲 AL AZAR\n(Aleatorio)"))
            {
                AssignPokemonToCurrentSlot(KantoPokemonDatabase.GetRandomPokemon());
            }

            for (int i = 0; i < filtered.Count; i++)
            {
                int itemIndex = i + 1;
                int r = itemIndex / cols;
                int c = itemIndex % cols;
                PokemonData poke = filtered[i];

                bool isP1 = SmashGameManager.Instance.selectedPlayerPokemon.id == poke.id;
                bool isAI = SmashGameManager.Instance.selectedAIPokemon.id == poke.id;

                if (isP1 && isAI) GUI.backgroundColor = new Color(0.85f, 0.45f, 1f);
                else if (isP1) GUI.backgroundColor = new Color(1f, 0.3f, 0.4f);
                else if (isAI) GUI.backgroundColor = new Color(0.25f, 0.65f, 1f);
                else GUI.backgroundColor = new Color(0.18f, 0.22f, 0.34f);

                string tag = (isP1 ? " [J1]" : "") + (isAI ? " [IA]" : "");
                string label = $"{poke.FormattedNumber} {poke.name}{tag}\n({poke.primaryType})";

                if (GUI.Button(new Rect(6 + c * 134, 6 + r * 66, 128, 60), label))
                {
                    AssignPokemonToCurrentSlot(poke);
                }
            }

            GUI.EndScrollView();

            // Panel inferior de confirmación y botón "¡LUCHAR!"
            float bottomY = Screen.height - 135;
            var p1Sel = SmashGameManager.Instance.selectedPlayerPokemon;
            var aiSel = SmashGameManager.Instance.selectedAIPokemon;

            GUI.backgroundColor = new Color(1f, 0.25f, 0.35f);
            GUI.Box(new Rect(30, bottomY, 310, 110),
                $"JUGADOR 1 (TÚ)\n{p1Sel.FormattedNumber} {p1Sel.name} ({p1Sel.primaryType})\n★ Especial: {p1Sel.SpecialAbilityName}\nVEL:{p1Sel.speed} ATQ:{p1Sel.attack} PESO:{p1Sel.weight}");

            GUI.backgroundColor = new Color(0.2f, 0.65f, 1f);
            GUI.Box(new Rect(Screen.width - 340, bottomY, 310, 110),
                $"RIVAL (IA)\n{aiSel.FormattedNumber} {aiSel.name} ({aiSel.primaryType})\n★ Especial: {aiSel.SpecialAbilityName}\nVEL:{aiSel.speed} ATQ:{aiSel.attack} PESO:{aiSel.weight}");

            // Selector de Dificultad de la IA
            float diffX = Screen.width / 2f - 150f;
            float diffY = bottomY - 38f;
            var diff = SmashGameManager.Instance.currentDifficulty;

            GUI.backgroundColor = diff == AIDifficulty.Easy ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.25f, 0.25f, 0.3f);
            if (GUI.Button(new Rect(diffX, diffY, 96, 28), "🟢 Fácil"))
                SmashGameManager.Instance.currentDifficulty = AIDifficulty.Easy;

            GUI.backgroundColor = diff == AIDifficulty.Medium ? new Color(1f, 0.8f, 0.2f) : new Color(0.25f, 0.25f, 0.3f);
            if (GUI.Button(new Rect(diffX + 102, diffY, 96, 28), "🟡 Medio"))
                SmashGameManager.Instance.currentDifficulty = AIDifficulty.Medium;

            GUI.backgroundColor = diff == AIDifficulty.Hard ? new Color(1f, 0.3f, 0.3f) : new Color(0.25f, 0.25f, 0.3f);
            if (GUI.Button(new Rect(diffX + 204, diffY, 96, 28), "🔴 Difícil"))
                SmashGameManager.Instance.currentDifficulty = AIDifficulty.Hard;

            float centerX = Screen.width / 2f - 150f;
            GUI.backgroundColor = _selectionConfirmed ? Color.green : Color.yellow;
            string confirmText = _selectionConfirmed ? "✓ SELECCIÓN CONFIRMADA" : "1. CONFIRMAR PERSONAJES";
            if (GUI.Button(new Rect(centerX, bottomY + 6, 300, 44), confirmText))
            {
                _selectionConfirmed = !_selectionConfirmed;
            }

            GUI.enabled = _selectionConfirmed;
            GUI.backgroundColor = new Color(1f, 0.35f, 0.1f);
            if (GUI.Button(new Rect(centerX, bottomY + 58, 300, 50), "2. ⚔️ ¡LUCHAR!"))
            {
                SmashGameManager.Instance.StartMatch();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
        }

        private void AssignPokemonToCurrentSlot(PokemonData chosen)
        {
            _selectionConfirmed = false;
            if (_selectingForPlayer)
            {
                SmashGameManager.Instance.selectedPlayerPokemon = chosen;
                _selectingForPlayer = false;
            }
            else
            {
                SmashGameManager.Instance.selectedAIPokemon = chosen;
            }
        }

        private void DrawInGameHUDGUI()
        {
            var gm = SmashGameManager.Instance;
            int mins = Mathf.FloorToInt(gm.matchRemainingSeconds / 60f);
            int secs = Mathf.FloorToInt(gm.matchRemainingSeconds % 60f);

            GUIStyle timerStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            timerStyle.normal.textColor = Color.white;
            GUI.Box(new Rect(Screen.width / 2f - 85f, 14f, 170f, 46f), $"{mins:D2}:{secs:D2}", timerStyle);

            // Controles recordatorio arriba a la izquierda
            GUI.Box(new Rect(14f, 14f, 440f, 54f),
                "A/D: Mover | W: Saltar (2x W: Salto Potenciado) | S: Bajar plataforma\nClick Izq: Rápido | Click Der: Fuerte | E / Espacio: Habilidad Especial");

            if (gm.playerFighter != null)
            {
                DrawFighterHUDCard(new Rect(Screen.width / 2f - 340f, Screen.height - 105f, 300f, 88f),
                    $"TÚ: {gm.playerFighter.pokemonData.name}",
                    gm.playerFighter.lives,
                    gm.playerFighter.damagePercentage,
                    new Color(1f, 0.25f, 0.35f));
            }

            if (gm.aiFighter != null)
            {
                string diffTag = gm.currentDifficulty == AIDifficulty.Easy ? "🟢 FÁCIL" : (gm.currentDifficulty == AIDifficulty.Medium ? "🟡 MEDIO" : "🔴 DIFÍCIL");
                DrawFighterHUDCard(new Rect(Screen.width / 2f + 40f, Screen.height - 105f, 300f, 88f),
                    $"IA ({diffTag}): {gm.aiFighter.pokemonData.name}",
                    gm.aiFighter.lives,
                    gm.aiFighter.damagePercentage,
                    new Color(0.2f, 0.65f, 1f));
            }
        }

        private void DrawFighterHUDCard(Rect rect, string title, int lives, float damagePct, Color borderColor)
        {
            GUI.backgroundColor = borderColor;
            GUI.Box(rect, "");
            GUI.backgroundColor = Color.white;

            string hearts = new string('●', Mathf.Max(0, lives)) + new string('○', Mathf.Max(0, 3 - lives));
            string danger = damagePct >= 100f ? " [¡MÁXIMO - PELIGRO KO!]" : "";

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(rect.x + 12, rect.y + 8, rect.width - 24, 26), $"{title}   Vidas: {hearts}", style);

            GUIStyle pctStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold
            };
            pctStyle.normal.textColor = damagePct >= 100f ? Color.red : Color.yellow;
            GUI.Label(new Rect(rect.x + 12, rect.y + 38, rect.width - 24, 38), $"{damagePct:F0}%{danger}", pctStyle);
        }

        private void DrawResultOverlayGUI()
        {
            Rect boxRect = new Rect(Screen.width / 2f - 240f, Screen.height / 2f - 130f, 480f, 260f);
            GUI.backgroundColor = new Color(0.08f, 0.12f, 0.22f, 0.96f);
            GUI.Box(boxRect, "");

            GUIStyle bannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            bannerStyle.normal.textColor = new Color(1f, 0.84f, 0.1f);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = Color.white;

            GUIStyle detailStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            detailStyle.normal.textColor = new Color(0.8f, 0.85f, 0.95f);

            GUI.Label(new Rect(boxRect.x, boxRect.y + 20, boxRect.width, 46), _resultBanner, bannerStyle);
            GUI.Label(new Rect(boxRect.x, boxRect.y + 72, boxRect.width, 32), _resultSubtitle, subStyle);
            GUI.Label(new Rect(boxRect.x, boxRect.y + 110, boxRect.width, 28), _resultDetail, detailStyle);

            GUI.backgroundColor = new Color(1f, 0.35f, 0.2f);
            if (GUI.Button(new Rect(boxRect.x + 35, boxRect.y + 175, 195, 50), "🔄 Volver a Jugar"))
            {
                SmashGameManager.Instance.StartMatch();
            }

            GUI.backgroundColor = new Color(0.25f, 0.55f, 0.95f);
            if (GUI.Button(new Rect(boxRect.x + 250, boxRect.y + 175, 195, 50), "📋 Selección Personajes"))
            {
                SmashGameManager.Instance.ReturnToCharacterSelect();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}

