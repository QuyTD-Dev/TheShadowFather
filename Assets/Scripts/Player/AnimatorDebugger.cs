using UnityEngine;
using UnityEngine.InputSystem;

namespace TheShadowFather.Player
{
    /// <summary>
    /// Debug tool để kiểm tra Animator state và parameters real-time
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showGUI = true;

        private Animator animator;
        private bool showDebugPanel = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Toggle debug panel với phím F1
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                showDebugPanel = !showDebugPanel;
            }

            if (enableDebugLogs)
            {
                LogAnimatorState();
            }
        }

        private void LogAnimatorState()
        {
            // Log state mỗi khi FormState hoặc ElementType thay đổi
            int formState = animator.GetInteger("FormState");
            int elementType = animator.GetInteger("ElementType");

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            if (clipInfo.Length > 0)
            {
                string clipName = clipInfo[0].clip.name;
                Debug.Log($"[ANIMATOR] FormState={formState}, ElementType={elementType}, CurrentClip={clipName}, StateHash={stateInfo.shortNameHash}");
            }
        }

        private void OnGUI()
        {
            if (!showGUI || !showDebugPanel) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== ANIMATOR DEBUGGER ===", GUI.skin.box);
            GUILayout.Space(10);

            // Parameters
            GUILayout.Label("PARAMETERS:", GUI.skin.box);
            GUILayout.Label($"FormState: {animator.GetInteger("FormState")}");
            GUILayout.Label($"ElementType: {animator.GetInteger("ElementType")}");
            GUILayout.Label($"Speed: {animator.GetFloat("Speed"):F2}");
            GUILayout.Label($"IsGrounded: {animator.GetBool("IsGrounded")}");
            GUILayout.Label($"IsRunning: {animator.GetBool("IsRunning")}");
            GUILayout.Label($"IsDashing: {animator.GetBool("IsDashing")}");
            GUILayout.Label($"VerticalVelocity: {animator.GetFloat("VerticalVelocity"):F2}");

            GUILayout.Space(10);

            // Current State
            GUILayout.Label("CURRENT STATE:", GUI.skin.box);
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            if (clipInfo.Length > 0)
            {
                GUILayout.Label($"Clip: {clipInfo[0].clip.name}");
                GUILayout.Label($"Progress: {stateInfo.normalizedTime:F2}");
                GUILayout.Label($"State Hash: {stateInfo.shortNameHash}");
            }

            GUILayout.Space(10);

            // Manual Controls
            GUILayout.Label("MANUAL CONTROLS:", GUI.skin.box);
            if (GUILayout.Button("Transform to Human (0)"))
            {
                animator.SetInteger("FormState", 0);
                Debug.Log("[DEBUG] Manual set FormState = 0 (Human)");
            }
            if (GUILayout.Button("Transform to Half-Demon Fire (1, 0)"))
            {
                animator.SetInteger("FormState", 1);
                animator.SetInteger("ElementType", 0);
                Debug.Log("[DEBUG] Manual set FormState = 1, ElementType = 0 (Half-Demon Fire)");
            }
            if (GUILayout.Button("Transform to Half-Demon Frost (1, 1)"))
            {
                animator.SetInteger("FormState", 1);
                animator.SetInteger("ElementType", 1);
                Debug.Log("[DEBUG] Manual set FormState = 1, ElementType = 1 (Half-Demon Frost)");
            }
            if (GUILayout.Button("Transform to Demon (2)"))
            {
                animator.SetInteger("FormState", 2);
                Debug.Log("[DEBUG] Manual set FormState = 2 (Demon)");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
