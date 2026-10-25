using System.Collections;
using UnityEngine;

namespace KantoSmash
{
    /// <summary>
    /// Controlador 2D de luchador para Kanto Smash (Jugador e IA).
    /// Controles Jugador:
    /// - A: Izquierda
    /// - D: Derecha
    /// - S: Agacharse (2 veces seguidas para bajar de una plataforma pequeña)
    /// - W: Saltar (2 veces seguidas para salto potenciado)
    /// - Click Izquierdo Ratón: Ataque Rápido (más rápido, llena menos acumulación %)
    /// - Click Derecho Ratón: Ataque Fuerte (más lento, llena más acumulación % y lanza más fuerte)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class FighterController : MonoBehaviour
    {
        [Header("Identidad del Pokémon")]
        public PokemonData pokemonData;
        public bool isAI = false;

        [Header("Vidas y Acumulación de Daño (Smash Bros Brawl)")]
        public int lives = 3;
        public float damagePercentage = 0f; // 0% a 300%+, 100% = Máximo de la barra de peligro

        [Header("Parámetros de Movimiento")]
        public float moveSpeed = 7.5f;
        public float jumpForce = 13.5f;
        public float boostedJumpForce = 19.5f; // Salto potenciado (doble W)
        public float weightFactor = 1.0f;
        public float attackMultiplier = 1.0f;

        [Header("Estado de Combate")]
        public int facingDirection = 1; // 1 = Derecha, -1 = Izquierda
        public bool isGrounded = false;
        public bool isCrouching = false;
        public bool isAttacking = false;
        public float hitstunTimer = 0f;
        public float invincibilityTimer = 0f;

        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Collider2D _currentStandingPlatform;

        // Tiempos para detectar doble pulsación de W (Salto Potenciado) y S (Bajar de Plataforma)
        private float _lastWTapTime = -10f;
        private float _lastSTapTime = -10f;
        private const float DoubleTapThreshold = 0.32f;

        private int _airJumpsRemaining = 1;
        private float _attackCooldownTimer = 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _rb.gravityScale = 3.2f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void InitializeFighter(PokemonData data, bool controlledByAI, Vector2 spawnPos, int initialFacing)
        {
            pokemonData = data;
            isAI = controlledByAI;
            facingDirection = initialFacing;
            lives = 3;
            damagePercentage = 0f;

            // Escalar estadísticas según el Pokémon de la Pokédex de Kanto
            moveSpeed = 6.5f + (data.speed - 5) * 0.45f;
            weightFactor = 0.8f + (data.weight - 5) * 0.06f;
            attackMultiplier = 0.9f + (data.attack - 5) * 0.05f;

            transform.position = spawnPos;
            _spriteRenderer.sprite = data.cachedSprite;

            StartCoroutine(KantoPokemonDatabase.LoadSpriteCoroutine(data, spr =>
            {
                if (_spriteRenderer != null) _spriteRenderer.sprite = spr;
            }));
        }

        private void Update()
        {
            if (SmashGameManager.Instance == null || !SmashGameManager.Instance.IsMatchActive)
                return;

            if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;
            if (invincibilityTimer > 0f) invincibilityTimer -= Time.deltaTime;
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;

            CheckGroundStatus();

            if (!isAI && hitstunTimer <= 0f)
            {
                HandlePlayerInput();
            }

            UpdateVisualAppearance();
            CheckBlastZoneBounds();
        }

        private void HandlePlayerInput()
        {
            // 1. Movimiento Horizontal: A (izquierda) y D (derecha)
            float horizontal = 0f;
            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;

            if (horizontal != 0f && !isAttacking)
            {
                facingDirection = horizontal > 0f ? 1 : -1;
            }

            // 2. Agacharse con S y bajar de plataforma con doble S (S + S)
            isCrouching = Input.GetKey(KeyCode.S) && isGrounded;
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (Time.time - _lastSTapTime <= DoubleTapThreshold)
                {
                    TryDropThroughPlatform();
                }
                _lastSTapTime = Time.time;
            }

            // 3. Saltar con W y Salto Potenciado con doble W (W + W)
            if (Input.GetKeyDown(KeyCode.W))
            {
                bool isDoubleTapW = (Time.time - _lastWTapTime) <= DoubleTapThreshold;
                _lastWTapTime = Time.time;
                PerformJump(isDoubleTapW);
            }

            // Aplicar velocidad horizontal
            float crouchMult = isCrouching ? 0.45f : 1.0f;
            float targetVx = horizontal * moveSpeed * crouchMult;
            _rb.linearVelocity = new Vector2(
                Mathf.Lerp(_rb.linearVelocity.x, targetVx, isGrounded ? 0.35f : 0.18f),
                _rb.linearVelocity.y
            );

            // 4. Ataques con el Ratón:
            // Click Izquierdo (0) = Ataque Rápido
            if (Input.GetMouseButtonDown(0))
            {
                TryExecuteAttack(isStrongAttack: false);
            }
            // Click Derecho (1) = Ataque Fuerte
            else if (Input.GetMouseButtonDown(1))
            {
                TryExecuteAttack(isStrongAttack: true);
            }
        }

        public void PerformJump(bool isBoostedJump)
        {
            if (hitstunTimer > 0f) return;

            if (isBoostedJump && (isGrounded || _airJumpsRemaining > 0))
            {
                // Salto potenciado (dos veces seguidas W)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, boostedJumpForce);
                if (!isGrounded) _airJumpsRemaining = 0;
                isGrounded = false;
            }
            else if (isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                isGrounded = false;
                _airJumpsRemaining = 1;
            }
            else if (_airJumpsRemaining > 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce * 0.92f);
                _airJumpsRemaining--;
            }
        }

        public void TryDropThroughPlatform()
        {
            if (_currentStandingPlatform != null && _currentStandingPlatform.CompareTag("PassThroughPlatform"))
            {
                StartCoroutine(DisablePlatformCollisionTemporarily(_currentStandingPlatform));
            }
        }

        private IEnumerator DisablePlatformCollisionTemporarily(Collider2D platformCollider)
        {
            Physics2D.IgnoreCollision(_collider, platformCollider, true);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -4f);
            yield return new WaitForSeconds(0.32f);
            if (platformCollider != null && _collider != null)
            {
                Physics2D.IgnoreCollision(_collider, platformCollider, false);
            }
        }

        public void TryExecuteAttack(bool isStrongAttack)
        {
            if (isAttacking || _attackCooldownTimer > 0f || hitstunTimer > 0f) return;
            StartCoroutine(AttackRoutine(isStrongAttack));
        }

        private IEnumerator AttackRoutine(bool isStrongAttack)
        {
            isAttacking = true;

            // El ataque rápido es veloz; el ataque fuerte tiene más carga y enfriamiento
            float startupDelay = isStrongAttack ? 0.18f : 0.05f;
            float recoveryDelay = isStrongAttack ? 0.32f : 0.12f;
            _attackCooldownTimer = isStrongAttack ? 0.45f : 0.16f;

            yield return new WaitForSeconds(startupDelay);

            if (hitstunTimer <= 0f)
            {
                float range = isStrongAttack ? 1.55f : 1.15f;
                Vector2 hitCenter = (Vector2)transform.position + new Vector2(facingDirection * range * 0.65f, 0f);
                Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, range * 0.6f);

                foreach (var hit in hits)
                {
                    FighterController target = hit.GetComponent<FighterController>();
                    if (target != null && target != this)
                    {
                        ApplySmashHit(target, isStrongAttack);
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(recoveryDelay);
            isAttacking = false;
        }

        private void ApplySmashHit(FighterController target, bool isStrongAttack)
        {
            if (target.invincibilityTimer > 0f) return;

            // 1. Acumulación de daño más pausada (para alargar las partidas):
            // Ataques rápidos llenan menos (2% - 4%); ataques fuertes llenan más (7% - 10%)
            float damageAdded = isStrongAttack
                ? Random.Range(7f, 10f) * attackMultiplier
                : Random.Range(2f, 4f) * attackMultiplier;

            target.damagePercentage = Mathf.Min(999f, target.damagePercentage + Mathf.Round(damageAdded));

            // 2. Al llegar al máximo de la barra (>= 100%), probabilidad alta de mandar disparado fuera del mapa
            bool criticalSmashLaunch = false;
            if (target.damagePercentage >= 100f)
            {
                float criticalProbability = isStrongAttack ? 0.82f : 0.58f;
                if (Random.value <= criticalProbability)
                {
                    criticalSmashLaunch = true;
                }
            }

            // 3. Calcular fuerza de empuje (Knockback estilo Super Smash Bros Brawl)
            float baseKnockback = isStrongAttack ? 8.5f : 4.5f;
            float scaledKnockback = (target.damagePercentage * (isStrongAttack ? 0.145f : 0.075f)) / target.weightFactor;
            float finalLaunchForce = baseKnockback + scaledKnockback;

            if (criticalSmashLaunch)
            {
                finalLaunchForce = Mathf.Max(34f, finalLaunchForce * 1.9f);
            }

            Vector2 launchDirection = new Vector2(facingDirection * 0.82f, isStrongAttack ? 0.58f : 0.44f).normalized;
            target.ReceiveKnockback(launchDirection * finalLaunchForce, isStrongAttack ? 0.45f : 0.22f);
        }

        public void ReceiveKnockback(Vector2 velocityImpulse, float stunDuration)
        {
            hitstunTimer = stunDuration;
            _rb.linearVelocity = velocityImpulse;
        }

        private void CheckGroundStatus()
        {
            Vector2 boxCenter = (Vector2)transform.position + Vector2.down * (_collider.size.y * 0.52f);
            Vector2 boxSize = new Vector2(_collider.size.x * 0.8f, 0.15f);

            Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f);
            if (hit != null && hit.gameObject != gameObject && _rb.linearVelocity.y <= 0.1f)
            {
                isGrounded = true;
                _currentStandingPlatform = hit;
                _airJumpsRemaining = 1;
            }
            else
            {
                isGrounded = false;
                _currentStandingPlatform = null;
            }
        }

        private void UpdateVisualAppearance()
        {
            _spriteRenderer.flipX = (facingDirection < 0);
            float scaleY = isCrouching ? 0.72f : 1.0f;
            transform.localScale = new Vector3(1.5f, 1.5f * scaleY, 1f);
        }

        private void CheckBlastZoneBounds()
        {
            // Límites del mapa (Blast Zones)
            if (transform.position.x < -14f || transform.position.x > 14f ||
                transform.position.y < -9f || transform.position.y > 11f)
            {
                SmashGameManager.Instance.OnFighterRingOut(this);
            }
        }

        public void RespawnAt(Vector2 spawnPosition)
        {
            transform.position = spawnPosition;
            _rb.linearVelocity = Vector2.zero;
            damagePercentage = 0f;
            hitstunTimer = 0f;
            invincibilityTimer = 2.0f;
        }
    }
}

