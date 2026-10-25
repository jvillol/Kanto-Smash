# Kanto Smash - Super Pokémon Brawl (2D)

Juego de lucha en plataformas 2D inspirado en **Super Smash Bros. Brawl** con los **151 Pokémon de la Pokédex de Kanto**.

---

## 🚀 Cómo Jugar Directamente en el PC (Sin Instalar Nada)

1. Haz doble clic en **`KantoSmash.exe`** (o en `Jugar_Kanto_Smash.bat`, o abre directamente `Playable/index.html`).
2. Se abrirá la ventana del juego con la **Selección de Personajes** de los 151 Pokémon de Kanto.

---

## 🎮 Controles y Mecánicas

| Acción | Control | Detalles |
| :--- | :--- | :--- |
| **Ir a la izquierda** | `A` | Movimiento horizontal hacia la izquierda |
| **Ir a la derecha** | `D` | Movimiento horizontal hacia la derecha |
| **Agacharse / Bajar** | `S` / `S + S` | Mantén `S` para agacharte. Pulsa **`S` dos veces seguidas** para bajar de una plataforma pequeña |
| **Saltar / Salto Potenciado** | `W` / `W + W` | Pulsa `W` para saltar. Pulsa **`W` dos veces seguidas** para ejecutar un **Salto Potenciado** |
| **Ataque Rápido** | `Click Izquierdo` | Golpe veloz que llena menos la barra de acumulación de daño (`%`) |
| **Ataque Fuerte (Smash)** | `Click Derecho` | Golpe más lento pero contundente que llena mucho más la acumulación (`%`) y lanza más lejos |

### Reglas del Combate (Estilo Super Smash Bros. Brawl)
- **Escenario**: Una plataforma grande principal y 3 plataformas pequeñas flotantes a las que puedes saltar y de las que puedes bajar pulsando `S` dos veces seguidas.
- **3 Vidas (Stocks)** para cada luchador (Jugador 1 vs Rival IA).
- **Acumulación de Daño (`%`)**: Cada golpe recibido incrementa tu porcentaje acumulado. Cuando alcanza el máximo de la barra (`100%+`), aparece el aviso de peligro y cualquier golpe (especialmente el Ataque Fuerte) tiene un **% alto de probabilidad de mandarte disparado fuera del mapa**.
- **Tiempo Máximo de 5 Minutos (`05:00`)**:
  - Si un luchador deja al rival con **0 vidas** antes de acabar el tiempo, gana inmediatamente.
  - Si se agotan los **5 minutos**, gana quien **más vidas tenga** en ese momento. Si **ambos tienen 2 vidas** (o las mismas vidas), se declara **Empate**.
- **Pantalla de Resultados**: Muestra el rótulo de **`¡GANADOR!`** (o `¡EMPATE!`) con botones para **Volver a Jugar** o **Volver a la Selección de Personajes**.

---

## 🛠️ Proyecto de Unity (C#)

En la carpeta **`UnityProject/`** tienes el proyecto estructurado para **Unity Editor**:
- `Assets/Scripts/KantoPokemonDatabase.cs`: Base de datos de los 151 Pokémon de Kanto, estadísticas, tipos y carga automática de sprites.
- `Assets/Scripts/FighterController.cs`: Física 2D (`Rigidbody2D`), controles `AWSD`, doble pulsación de `W` (salto potenciado) y `S` (bajar de plataforma), ataques con Click Izquierdo (rápido) y Click Derecho (fuerte), acumulación de daño y lanzamiento crítico fuera del mapa.
- `Assets/Scripts/AIFighterBrain.cs`: Inteligencia Artificial del rival que persigue, salta entre plataformas, se recupera si sale del escenario y ataca según el porcentaje del jugador.
- `Assets/Scripts/SmashStageBuilder.cs`: Generador del mapa con 1 plataforma grande y 3 plataformas pequeñas atravesables (`PlatformEffector2D`).
- `Assets/Scripts/SmashGameManager.cs`: Temporizador de 5 minutos, control de las 3 vidas y reglas de victoria/empate.
- `Assets/Scripts/SmashUIManager.cs`: Interfaz de selección de los 151 Pokémon + botón Al Azar, confirmación, botón "¡Luchar!", HUD de combate y pantalla de ganador.
- `Assets/Scripts/SmashBootstrap.cs`: Auto-inicializador con `[RuntimeInitializeOnLoadMethod]` para que al abrir el proyecto en Unity y pulsar **Play** todo funcione automáticamente sin necesidad de configurar prefabs manualmente.

