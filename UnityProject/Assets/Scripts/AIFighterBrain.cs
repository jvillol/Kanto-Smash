using UnityEngine;

namespace KantoSmash
{
    public enum AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    /// <summary>
    /// Cerebro de Inteligencia Artificial para el rival en Kanto Smash según la dificultad:
    /// - Fácil: Movimientos lentos, pausas/dudas frecuentes, ataques básicos y recuperación tardía.
    /// - Medio: Comportamiento equilibrado y ataques mixtos.
    /// - Difícil: Súper ágil, reflejos casi inmediatos, busca combos y KOs con ataques fuertes.
    /// </summary>
    [RequireComponent(typeof(FighterController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIFighterBrain : MonoBehaviour
    {
        public FighterController targetPlayer;
        public AIDifficulty difficulty = AIDifficulty.Medium;

        private FighterController _self;
        private Rigidbody2D _rb;
        private float _decisionCooldown = 0f;
        private bool _isHesitating = false;

        private void Awake()
        {
            _self = GetComponent<FighterController>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (SmashGameManager.Instance == null || !SmashGameManager.Instance.IsMatchActive)
                return;

            if (targetPlayer == null || _self.hitstunTimer > 0f)
                return;

            float speedMult = difficulty == AIDifficulty.Easy ? 0.60f : (difficulty == AIDifficulty.Medium ? 0.85f : 1.08f);

            // 1. Recuperación si está fuera de la plataforma principal
            if (Mathf.Abs(transform.position.x) > 5.8f || transform.position.y < -2.2f)
            {
                int dirToCenter = transform.position.x < 0f ? 1 : -1;
                _self.facingDirection = dirToCenter;

                // En fácil, la recuperación es más tardía
                float recoveryChance = difficulty == AIDifficulty.Easy ? 0.70f : 1.0f;
                if (Random.value <= recoveryChance)
                {
                    _rb.linearVelocity = new Vector2(dirToCenter * _self.moveSpeed * speedMult, _rb.linearVelocity.y);

                    if (_rb.linearVelocity.y < -0.5f && transform.position.y < -1.0f)
                    {
                        bool useBoosted = difficulty != AIDifficulty.Easy;
                        _self.PerformJump(isBoostedJump: useBoosted);
                    }
                }
                return;
            }

            Vector2 delta = targetPlayer.transform.position - transform.position;
            float distX = Mathf.Abs(delta.x);
            float distY = delta.y;

            // 2. Toma de decisiones
            _decisionCooldown -= Time.deltaTime;
            if (_decisionCooldown <= 0f)
            {
                switch (difficulty)
                {
                    case AIDifficulty.Easy:
                        _decisionCooldown = Random.Range(0.65f, 1.15f);
                        _isHesitating = Random.value < 0.40f; // Pausa / duda en fácil
                        break;
                    case AIDifficulty.Medium:
                        _decisionCooldown = Random.Range(0.25f, 0.48f);
                        _isHesitating = Random.value < 0.12f;
                        break;
                    case AIDifficulty.Hard:
                        _decisionCooldown = Random.Range(0.10f, 0.22f);
                        _isHesitating = false;
                        break;
                }

                if (!_isHesitating)
                {
                    float jumpChance = difficulty == AIDifficulty.Easy ? 0.25f : 0.65f;
                    if (distY > 1.6f && _self.isGrounded && Random.value < jumpChance)
                    {
                        _self.PerformJump(isBoostedJump: distY > 3.0f && difficulty != AIDifficulty.Easy);
                    }
                    else if (distY < -1.5f && _self.isGrounded && Random.value < jumpChance)
                    {
                        _self.TryDropThroughPlatform();
                    }

                    // Atacar cuando está en rango
                    float attackDist = difficulty == AIDifficulty.Easy ? 1.25f : 1.55f;
                    if (distX <= attackDist && Mathf.Abs(distY) <= 1.2f)
                    {
                        float strongChance = difficulty == AIDifficulty.Easy ? 0.10f : (difficulty == AIDifficulty.Medium ? 0.35f : 0.55f);
                        if (targetPlayer.damagePercentage >= 80f && difficulty != AIDifficulty.Easy)
                        {
                            strongChance = Mathf.Min(0.85f, strongChance * 1.8f);
                        }
                        _self.TryExecuteAttack(Random.value < strongChance);
                    }

                    // Usar Habilidad Especial única tácticamente
                    if (_self.specialCooldownTimer <= 0f && distX <= 3.5f)
                    {
                        float spChance = difficulty == AIDifficulty.Easy ? 0.15f : 0.45f;
                        if (Random.value < spChance)
                        {
                            _self.TryExecuteSpecialAbility();
                        }
                    }
                }
            }

            if (_isHesitating)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x * 0.5f, _rb.linearVelocity.y);
                return;
            }

            // Orientarse y acercarse al jugador
            if (distX > 0.25f && !_self.isAttacking)
            {
                _self.facingDirection = delta.x > 0f ? 1 : -1;
            }

            if (distX > 1.15f)
            {
                float targetVx = _self.facingDirection * _self.moveSpeed * speedMult;
                float accel = difficulty == AIDifficulty.Easy ? 0.12f : (difficulty == AIDifficulty.Medium ? 0.22f : 0.32f);
                _rb.linearVelocity = new Vector2(Mathf.Lerp(_rb.linearVelocity.x, targetVx, accel), _rb.linearVelocity.y);
            }
        }
    }
}
