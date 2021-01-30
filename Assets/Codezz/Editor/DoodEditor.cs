using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dood))]
public class DoodEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        var dood = (Dood) target;

        if (Application.isPlaying && dood.behaviourTree != null) {
            EditorTools.Label("<b>Behaviour tree details:</b>");
            EditorTools.Label("<b>Current behaviour tree input:</b>");
            EditorTools.Label(dood.behaviourTree.data.ToString());
            EditorTools.Divider();

            EditorTools.Label(dood.behaviourTree.command.ToString());

            EditorTools.Divider();

            EditorTools.Label("<b>Behaviour tree:</b>");
            BehaviourTreeDrawer<Dood.DoodAIInput, Dood.DoodAIOutput>.Draw(dood.behaviourTree);
        }
    }

    public override bool RequiresConstantRepaint() {
        return Application.isPlaying;
    }
}

public static class EditorTools {
    public static GUIStyle BuildStyle(bool wordWrap, bool bold, bool italic, TextAnchor alignment, Color textColor, bool useRichText, int fontSize,
                                      TextClipping clipping = TextClipping.Clip) {
        FontStyle fontStyle = FontStyle.Normal;
        if (bold && italic)
            fontStyle = FontStyle.BoldAndItalic;
        else if (bold)
            fontStyle = FontStyle.Bold;
        else if (italic)
            fontStyle = FontStyle.Italic;

        var defaultLabel = GUI.skin.label;
        if (textColor == default)
            textColor = defaultLabel.normal.textColor;

        var style = new GUIStyle(defaultLabel) {
            wordWrap = wordWrap,
            fontStyle = fontStyle,
            fontSize = fontSize,
            alignment = alignment,
            richText = useRichText,
            normal = {textColor = textColor},
            clipping = clipping
        };
        return style;
    }

    public static void Label(string label, bool wordWrap = true, bool bold = false, bool italic = false, TextAnchor alignment = TextAnchor.UpperLeft,
                             Color textColor = default, bool useRichText = true, int fontSize = 0, TextClipping clipping = TextClipping.Clip,
                             params GUILayoutOption[] options) {
        var style = BuildStyle(wordWrap, bold, italic, alignment, textColor, useRichText, fontSize, clipping);
        EditorGUILayout.LabelField(label, style, options);
    }

    public static void Divider(float thickness = 1, bool horizontal = true, Color color = default) {
        Divider(thickness, defaultDivider, horizontal, color);
    }

    /// <summary>
    /// Draws a divider line for GUILayout code.
    /// </summary>
    /// <param name="thickness">Thickness of the divider</param>
    /// <param name="dividerStyle">GUI style of the divider</param>
    /// <param name="horizontal">Should the divider be horizontal?</param>
    /// <param name="color"></param>
    public static void Divider(float thickness, GUIStyle dividerStyle, bool horizontal = true, Color color = default) {
        if (color == default)
            color = defaultDividerColor;

        Rect position;
        if (horizontal)
            position = GUILayoutUtility.GetRect(GUIContent.none, dividerStyle, GUILayout.Height(thickness));
        else {
            position = GUILayoutUtility.GetRect(GUIContent.none, dividerStyle, GUILayout.Width(thickness), GUILayout.ExpandHeight(true));
        }

        if (Event.current.type == EventType.Repaint) {
            Color restoreColor = GUI.color;
            GUI.color = color;
            dividerStyle.Draw(position, false, false, false, false);
            GUI.color = restoreColor;
        }
    }

    private static GUIStyle _defaultDivider;
    private static GUIStyle defaultDivider {
        get {
            if (_defaultDivider == null) {
                _defaultDivider = new GUIStyle {
                    normal = {
                        background = EditorGUIUtility.whiteTexture
                    },
                    stretchWidth = true,
                    margin = new RectOffset(0, 0, 7, 7)
                };
            }

            return _defaultDivider;
        }
    }

    private static Color _defaultDividerColor;
    private static Color defaultDividerColor {
        get {
            if (_defaultDividerColor == default)
                _defaultDividerColor = EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f);
            return _defaultDividerColor;
        }
    }
}