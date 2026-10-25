using UnityEngine;

namespace KantoSmash
{
    /// <summary>
    /// Cerebro de Inteligencia Artificial para el rival en Kanto Smash.
    /// Persigue al jugador, salta a las plataformas pequeñas, regresa al mapa si sale lanzado
    /// y alterna ataques rápidos y fuertes según la acumulación de daño del rival.
    /// </summary>
    [RequireComponent(typeof(FighterController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIFighterBrain : MonoBehaviour
    {
        public FighterController targetPlayer;

        private FighterController _self;
        private Rigidbody2D _rb;
        private float _decisionCooldown = 0f;

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

            // 1. Recuperación si está fuera de la plataforma principal
            if (Mathf.Abs(transform.position.x) > 5.8f || transform.position.y < -2.2f)
            {
                int dirToCenter = transform.position.x < 0f ? 1 : -1;
                _self.facingDirection = dirToCenter;
                _rb.linearVelocity = new Vector2(dirToCenter * _self.moveSpeed, _rb.linearVelocity.y);

                if (_rb.linearVelocity.y < -0.5f && transform.position.y < -1.0f)
                {
                    _self.PerformJump(isBoostedJump: true);
                }
                return;
            }

            Vector2 delta = targetPlayer.transform.position - transform.position;
            float distX = Mathf.Abs(delta.x);
            float distY = delta.y;

            // 2. Orientarse y acercarse al jugador
            if (distX > 0.25f && !_self.isAttacking)
            {
                _self.facingDirection = delta.x > 0f ? 1 : -1;
            }

            if (distX > 1.25f)
            {
                float targetVx = _self.facingDirection * _self.moveSpeed * 0.9f;
                _rb.linearVelocity = new Vector2(Mathf.Lerp(_rb.linearVelocity.x, targetVx, 0.2f), _rb.linearVelocity.y);
            }

            // 3. Saltar a las plataformas pequeñas o bajar de ellas
            _decisionCooldown -= Time.deltaTime;
            if (_decisionCooldown <= 0f)
            {
                _decisionCooldown = Random.Range(0.25f, 0.55f);

                if (distY > 1.6f && _self.isGrounded)
                {
                    _self.PerformJump(isBoostedJump: distY > 3.2f);
                }
                else if (distY < -1.5f && _self.isGrounded)
                {
                    _self.TryDropThroughPlatform();
                }

                // 4. Atacar cuando está en rango
                if (distX <= 1.55f && Mathf.Abs(distY) <= 1.2f)
                {
                    // Si el jugador tiene alta acumulación de daño (>= 85%), la IA prioriza ataques fuertes
                    bool useStrong = Random.value < (targetPlayer.damagePercentage >= 85f ? 0.60f : 0.32f);
                    _self.TryExecuteAttack(useStrong);
                }
            }
        }
    }
}

