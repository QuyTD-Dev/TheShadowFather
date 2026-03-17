using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheShadowFather.Player
{
    /// <summary>
    /// Hiệu ứng chiêu Until của Demon form.
    /// - Tối màn hình
    /// - Rung Camera
    /// - Trừ 50% máu hiện tại của TẤT CẢ quái trong scene
    /// </summary>
    public class DemonUntilEffect : MonoBehaviour
    {
        [Header("Camera Shake")]
        [Tooltip("Cường độ rung camera")]
        public float shakeStrength = 0.3f;
        [Tooltip("Thời gian rung (giây)")]
        public float shakeDuration = 0.6f;

        [Header("Screen Darken")]
        [Tooltip("Màu tối phủ lên màn hình (alpha = độ đậm)")]
        public Color darkColor = new Color(0f, 0f, 0f, 0.75f);
        [Tooltip("Thời gian fade vào (giây)")]
        public float fadeInDuration = 0.3f;
        [Tooltip("Thời gian giữ màn hình tối (giây)")]
        public float holdDuration = 0.5f;
        [Tooltip("Thời gian fade ra (giây)")]
        public float fadeOutDuration = 0.6f;

        [Header("Effect Lifetime")]
        [Tooltip("Thời gian tồn tại của hiệu ứng trước khi tự huỷ")]
        public float lifetime = 2.5f;

        // Canvas tối màn hình — tự tạo bằng code, không cần kéo tay
        private static Canvas _darkCanvas;
        private static Image _darkPanel;

        private void Start()
        {
            // Tự tạo Canvas UI che màn hình (nếu chưa có)
            EnsureDarkCanvas();

            // Bắt đầu tất cả hiệu ứng cùng lúc
            StartCoroutine(ScreenDarkenRoutine());
            StartCoroutine(CameraShakeRoutine());
            StartCoroutine(DamageAllEnemiesRoutine());

            Destroy(gameObject, lifetime);
        }

        // ─────────────────────────────────────────────
        // Hiệu ứng tối màn hình
        // ─────────────────────────────────────────────
        private IEnumerator ScreenDarkenRoutine()
        {
            // Fade in (tối dần)
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, darkColor.a, elapsed / fadeInDuration);
                SetPanelAlpha(alpha);
                yield return null;
            }
            SetPanelAlpha(darkColor.a);

            // Giữ màn hình tối
            yield return new WaitForSeconds(holdDuration);

            // Fade out (sáng dần)
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(darkColor.a, 0f, elapsed / fadeOutDuration);
                SetPanelAlpha(alpha);
                yield return null;
            }
            SetPanelAlpha(0f);
        }

        private void SetPanelAlpha(float alpha)
        {
            if (_darkPanel == null) return;
            Color c = _darkPanel.color;
            c.a = alpha;
            _darkPanel.color = c;
        }

        // ─────────────────────────────────────────────
        // Camera shake
        // ─────────────────────────────────────────────
        private IEnumerator CameraShakeRoutine()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null) yield break;

            Vector3 originalPos = cam.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float strength = shakeStrength * (1f - elapsed / shakeDuration);
                cam.transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * strength;
                yield return null;
            }
            cam.transform.localPosition = originalPos;
        }

        // ─────────────────────────────────────────────
        // Trừ 50% máu tất cả quái/boss
        // ─────────────────────────────────────────────
        private IEnumerator DamageAllEnemiesRoutine()
        {
            // Chờ một chút để animation đã hiện ra đẹp hơn
            yield return new WaitForSeconds(fadeInDuration * 0.5f);

            // Quái thường — dùng Health.cs
            Health[] enemies = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health h in enemies)
            {
                if (h == null) continue;
                int halfDamage = Mathf.CeilToInt(h.CurrentHealth * 0.5f);
                h.TakeDamage(halfDamage);
                Debug.Log($"[DemonUntil] Trừ 50% ({halfDamage} HP) của {h.gameObject.name}");
            }

            // Boss — dùng BossHealth.cs
            BossHealth[] bosses = FindObjectsByType<BossHealth>(FindObjectsSortMode.None);
            foreach (BossHealth b in bosses)
            {
                if (b == null) continue;
                int halfDamage = Mathf.CeilToInt((float)b.CurrentHealth * 0.5f);
                b.TakeDamage(halfDamage);
                Debug.Log($"[DemonUntil] Trừ 50% ({halfDamage} HP) của Boss {b.gameObject.name}");
            }
        }

        // ─────────────────────────────────────────────
        // Tự tạo Panel tối màn hình (chạy 1 lần)
        // ─────────────────────────────────────────────
        private void EnsureDarkCanvas()
        {
            if (_darkCanvas != null && _darkPanel != null) return;

            GameObject canvasGO = new GameObject("DemonDarkCanvas");
            DontDestroyOnLoad(canvasGO);

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Luôn nằm trên tất cả UI khác

            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject panelGO = new GameObject("DarkPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);

            Image img = panelGO.AddComponent<Image>();
            img.color = new Color(darkColor.r, darkColor.g, darkColor.b, 0f); // Bắt đầu trong suốt
            img.raycastTarget = false; // Không chặn click chuột

            RectTransform rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _darkCanvas = canvas;
            _darkPanel = img;
        }
    }
}
