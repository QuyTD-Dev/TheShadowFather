using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Text;
using System.IO;

public class AnimatorFullExporter
{
    [MenuItem("Tools/Animator/Export FULL Animator Info")]
    static void ExportAnimator()
    {
        AnimatorController controller = Selection.activeObject as AnimatorController;

        if (controller == null)
        {
            Debug.LogError("❌ Hãy chọn file AnimatorController (.controller) trong Project window");
            return;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("===== FULL ANIMATOR EXPORT =====");
        sb.AppendLine($"Controller: {controller.name}");
        sb.AppendLine();

        // PARAMETERS
        sb.AppendLine("## PARAMETERS");
        foreach (var p in controller.parameters)
        {
            sb.AppendLine($"- {p.name} | {p.type} | Default: {GetDefault(p)}");
        }

        // LAYERS
        foreach (var layer in controller.layers)
        {
            sb.AppendLine();
            sb.AppendLine($"## LAYER: {layer.name}");
            sb.AppendLine($"Weight: {layer.defaultWeight}");
            sb.AppendLine($"Blending: {layer.blendingMode}");
            sb.AppendLine($"IK Pass: {layer.iKPass}");

            DumpStateMachine(layer.stateMachine, sb, "  ");
        }

        string path = $"Assets/{controller.name}_Animator_FULL_DUMP.txt";
        File.WriteAllText(path, sb.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"✅ Animator FULL DUMP saved at: {path}");
    }

    static void DumpStateMachine(AnimatorStateMachine sm, StringBuilder sb, string indent)
    {
        sb.AppendLine($"{indent}StateMachine: {sm.name}");
        sb.AppendLine($"{indent}DefaultState: {sm.defaultState?.name}");

        foreach (var state in sm.states)
        {
            sb.AppendLine($"{indent}  State: {state.state.name}");
            sb.AppendLine($"{indent}    Motion: {state.state.motion?.name}");
            sb.AppendLine($"{indent}    Speed: {state.state.speed}");
            sb.AppendLine($"{indent}    Tag: {state.state.tag}");
            sb.AppendLine($"{indent}    WriteDefaults: {state.state.writeDefaultValues}");

            foreach (var t in state.state.transitions)
            {
                DumpTransition(t, sb, indent + "    ", state.state.name);
            }
        }

        foreach (var t in sm.anyStateTransitions)
        {
            DumpTransition(t, sb, indent + "  ", "AnyState");
        }

        foreach (var sub in sm.stateMachines)
        {
            DumpStateMachine(sub.stateMachine, sb, indent + "  ");
        }
    }

    static void DumpTransition(AnimatorStateTransition t, StringBuilder sb, string indent, string from)
    {
        sb.AppendLine($"{indent}Transition FROM {from}");
        sb.AppendLine($"{indent}  To: {(t.destinationState ? t.destinationState.name : "Exit")}");
        sb.AppendLine($"{indent}  HasExitTime: {t.hasExitTime}");
        sb.AppendLine($"{indent}  ExitTime: {t.exitTime}");
        sb.AppendLine($"{indent}  Duration: {t.duration}");
        sb.AppendLine($"{indent}  CanTransitionToSelf: {t.canTransitionToSelf}");

        foreach (var c in t.conditions)
        {
            sb.AppendLine($"{indent}    Condition: {c.parameter} {c.mode} {c.threshold}");
        }
    }

    static string GetDefault(AnimatorControllerParameter p)
    {
        return p.type switch
        {
            AnimatorControllerParameterType.Float => p.defaultFloat.ToString(),
            AnimatorControllerParameterType.Int => p.defaultInt.ToString(),
            AnimatorControllerParameterType.Bool => p.defaultBool.ToString(),
            AnimatorControllerParameterType.Trigger => "Trigger",
            _ => "Unknown"
        };
    }
}
