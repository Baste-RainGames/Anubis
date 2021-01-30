using System;
using System.Text;
using Debug = UnityEngine.Debug;

public class IndentingStringBuilder {
    public IndentingStringBuilder(int spacesPerIndent = 2) {
        this.spacesPerIndent = spacesPerIndent;
    }

    private readonly int spacesPerIndent;
    private readonly StringBuilder stringBuilder = new StringBuilder();

    private int indents;

    public int Indents {
        get => indents;
        set {
            if (value < 0) {
                Debug.LogError("Trying to set indents to less that 0!");
                indents = 0;
            }
            else {
                indents = value;
            }
        }
    }

    public void AppendLine(string s) {
        if (s.IndexOf('\n') != -1) {
            var lines = s.Split('\n');
            foreach (var line in lines) {
                AppendLine(line);
            }
        }
        else {
            InsertCurrentIndentationIfAtStartOfLine();
            stringBuilder.Append(s);
            stringBuilder.Append('\n');
        }
    }

    public void AppendLine() {
        stringBuilder.Append('\n');
    }

    private void InsertCurrentIndentationIfAtStartOfLine() {
        if (stringBuilder.Length == 0 || stringBuilder[stringBuilder.Length - 1] == '\n')
            for (int _ = 0; _ < Indents * spacesPerIndent; _++)
                stringBuilder.Append(' ');
    }

    public void AppendLineAndIndent(string s) {
        AppendLine(s);
        Indents++;
    }

    public void AppendLineAndDedent(string s) {
        AppendLine(s);
        Indents--;
    }

    public void Clear() {
        stringBuilder.Clear();
        Indents = 0;
    }

    public override string ToString() {
        return stringBuilder.ToString();
    }

    public void Append(string s) {
        InsertCurrentIndentationIfAtStartOfLine();
        stringBuilder.Append(s);
    }

    public IDisposable IndentScope => new IndentingStringBuilderIndentScope(this);

    private class IndentingStringBuilderIndentScope : IDisposable {
        private IndentingStringBuilder parent;

        public IndentingStringBuilderIndentScope(IndentingStringBuilder parent) {
            this.parent = parent;
            parent.indents++;
        }

        public void Dispose() {
            parent.Indents--;
        }
    }
}