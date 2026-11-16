using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using UnityEngine;

                                                               
           
                                                                    
                                                                                
  
                          
                                                                 
                                                                             
                                                                           
                                                                              
                                                                      
                                                                    
                                                                    
                                                                                      
                                  
                                                                                
                                                               

public static class LuaErrors
{
                                                                        
    public static readonly Dictionary<string, string> Sources = new Dictionary<string, string>();

    static readonly Regex Loc = new Regex(@"^(.*?):\((\d+),\d+[^)]*\):\s*(.*)$", RegexOptions.Singleline);

    public static string Format(InterpreterException e)
    {
        string msg = e.DecoratedMessage ?? e.Message;
        var sb = new StringBuilder();

        var m = Loc.Match(msg);
        if (m.Success)
        {
            string chunk = m.Groups[1].Value;
            int line = int.Parse(m.Groups[2].Value);
            sb.Append("Lua error in ").Append(chunk).Append(" line ").Append(line).Append(": ").Append(m.Groups[3].Value.Trim());

            string src;
            if (Sources.TryGetValue(chunk, out src))
            {
                string[] lines = src.Split('\n');
                int from = Math.Max(1, line - 2);
                int to = Math.Min(lines.Length, line + 2);
                for (int n = from; n <= to; n++)
                {
                    sb.Append('\n')
                      .Append(n == line ? " > " : "   ")
                      .Append(n.ToString().PadLeft(4))
                      .Append(" | ")
                      .Append(lines[n - 1].TrimEnd('\r'));
                }
            }
        }
        else
        {
            sb.Append("Lua error: ").Append(msg);
        }

        var stack = e.CallStack;
        if (stack != null)
        {
            int shown = 0;
            foreach (var w in stack)
            {
                if (w == null) continue;
                if (shown++ >= 4) break;
                sb.Append("\n   at ").Append(string.IsNullOrEmpty(w.Name) ? "(anonymous function)" : w.Name);
                if (w.Location != null) sb.Append(" (line ").Append(w.Location.FromLine).Append(')');
            }
        }
        return sb.ToString();
    }

    public static void Log(InterpreterException e)
    {
        Debug.LogError(Format(e));
    }
}

public static class Luau
{
                                                     
    public static bool Enabled = true;

    enum TT { Ident, Keyword, Number, Str, Op }

    class Tok
    {
        public string Prefix = "";                                            
        public string Text;
        public TT Type;
    }

    class Blk
    {
        public string Kind;                                                          
        public bool AwaitDo;
        public int StartIdx, DoIdx = -1, EndIdx = -1, Id;
        public bool HasContinue;
    }

    static readonly HashSet<string> Keywords = new HashSet<string>
    {
        "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "goto", "if", "in",
        "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while"
    };

    static readonly string[] Ops =
    {
        "...", "..=", "..", "==", "~=", "<=", ">=", "::", "+=", "-=", "*=", "/=", "%=", "^=", "->"
    };

    static readonly HashSet<string> BinaryOps = new HashSet<string>
    {
        "+", "-", "*", "/", "%", "^", "..", "==", "~=", "<", ">", "<=", ">=", "and", "or", "not", ".", ":", ","
    };

    public static string Transpile(string src)
    {
        if (!Enabled || string.IsNullOrEmpty(src)) return src;
        try
        {
            string tail;
            var toks = Tokenize(src, out tail);
            StripTypes(toks);
            CompoundAssign(toks);
            Continues(toks);

            var sb = new StringBuilder(src.Length + 64);
            foreach (var t in toks) sb.Append(t.Prefix).Append(t.Text);
            sb.Append(tail);
            return sb.ToString();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Luau] Could not preprocess the script, running it unchanged: " + e.Message);
            return src;
        }
    }

                                                                   
                 
                                                                   

    static List<Tok> Tokenize(string s, out string tail)
    {
        var toks = new List<Tok>();
        var prefix = new StringBuilder();
        int i = 0, n = s.Length;

        while (i < n)
        {
            char c = s[i];

            if (char.IsWhiteSpace(c)) { prefix.Append(c); i++; continue; }

                                                            
            if (c == '-' && i + 1 < n && s[i + 1] == '-')
            {
                int end = -1;
                if (i + 2 < n && s[i + 2] == '[') end = LongBracketEnd(s, i + 2);
                if (end < 0)
                {
                    end = s.IndexOf('\n', i);
                    if (end < 0) end = n;
                }
                prefix.Append(s, i, end - i);
                i = end;
                continue;
            }

            var tok = new Tok { Prefix = prefix.ToString() };
            prefix.Length = 0;
            int start = i;

            if (char.IsLetter(c) || c == '_')
            {
                while (i < n && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
                tok.Text = s.Substring(start, i - start);
                tok.Type = Keywords.Contains(tok.Text) ? TT.Keyword : TT.Ident;
            }
            else if (char.IsDigit(c) || (c == '.' && i + 1 < n && char.IsDigit(s[i + 1])))
            {
                while (i < n)
                {
                    char d = s[i];
                    if (char.IsLetterOrDigit(d) || d == '_')
                    {
                        i++;
                        if ((d == 'e' || d == 'E') && i < n && (s[i] == '+' || s[i] == '-')) i++;
                    }
                    else if (d == '.' && !(i + 1 < n && s[i + 1] == '.')) i++;
                    else break;
                }
                tok.Text = s.Substring(start, i - start);
                tok.Type = TT.Number;
            }
            else if (c == '"' || c == '\'')
            {
                i++;
                while (i < n && s[i] != c)
                {
                    if (s[i] == '\\') i++;
                    i++;
                }
                i = Math.Min(n, i + 1);
                tok.Text = s.Substring(start, i - start);
                tok.Type = TT.Str;
            }
            else if (c == '[' && LongBracketEnd(s, i) >= 0)
            {
                i = LongBracketEnd(s, i);
                tok.Text = s.Substring(start, i - start);
                tok.Type = TT.Str;
            }
            else if (c == '`')
            {
                tok.Text = ParseBacktick(s, ref i);
                tok.Type = TT.Str;
            }
            else
            {
                string op = null;
                foreach (var cand in Ops)
                {
                    if (string.CompareOrdinal(s, i, cand, 0, cand.Length) == 0) { op = cand; break; }
                }
                if (op == null) op = c.ToString();
                i += op.Length;
                tok.Text = op;
                tok.Type = TT.Op;
            }

            toks.Add(tok);
        }

        tail = prefix.ToString();
        return toks;
    }

                                                                                                    
    static int LongBracketEnd(string s, int pos)
    {
        int j = pos + 1, level = 0;
        while (j < s.Length && s[j] == '=') { level++; j++; }
        if (j >= s.Length || s[j] != '[') return -1;
        string close = "]" + new string('=', level) + "]";
        int end = s.IndexOf(close, j + 1, StringComparison.Ordinal);
        return end < 0 ? s.Length : end + close.Length;
    }

                                                               
    static string ParseBacktick(string s, ref int i)
    {
        int n = s.Length, newlines = 0;
        i++;                    
        var parts = new List<string>();
        var lit = new StringBuilder();

        while (i < n && s[i] != '`')
        {
            char c = s[i];

            if (c == '\\' && i + 1 < n)
            {
                char d = s[i + 1];
                if (d == '{' || d == '`') lit.Append(d);
                else lit.Append('\\').Append(d);
                i += 2;
                continue;
            }

            if (c == '{')
            {
                int depth = 1, j = i + 1;
                while (j < n && depth > 0)
                {
                    if (s[j] == '{') depth++;
                    else if (s[j] == '}') depth--;
                    j++;
                }
                string expr = s.Substring(i + 1, Math.Max(0, j - i - 2));
                if (lit.Length > 0) { parts.Add("\"" + lit + "\""); lit.Length = 0; }
                parts.Add("tostring(" + expr + ")");
                i = j;
                continue;
            }

            if (c == '"') lit.Append("\\\"");
            else if (c == '\n') { lit.Append("\\n"); newlines++; }
            else if (c != '\r') lit.Append(c);
            i++;
        }
        i = Math.Min(n, i + 1);                    

        if (lit.Length > 0 || parts.Count == 0) parts.Add("\"" + lit + "\"");

                                                                                                   
        return "(" + string.Join(" .. ", parts.ToArray()) + ")" + new string('\n', newlines);
    }

                                                                   
                       
                                                                   

                                                           
    static void Remove(List<Tok> t, int from, int to)
    {
        if (to > t.Count) to = t.Count;
        for (int i = from; i < to; i++)
        {
            int nl = 0;
            foreach (char ch in t[i].Prefix) if (ch == '\n') nl++;
            foreach (char ch in t[i].Text) if (ch == '\n') nl++;
            t[i].Prefix = new string('\n', nl);
            t[i].Text = "";
        }
    }

                                                                   
                        
                                                                   

    static void StripTypes(List<Tok> t)
    {
        for (int i = 0; i < t.Count; i++)
        {
            var k = t[i];
            string x = k.Text;

                                                          
            if (k.Type == TT.Ident && x == "type" && i + 2 < t.Count && t[i + 1].Type == TT.Ident
                && (t[i + 2].Text == "=" || t[i + 2].Text == "<")
                && !(i > 0 && (t[i - 1].Text == "." || t[i - 1].Text == ":")))
            {
                int j = i + 2;
                if (t[j].Text == "<") j = SkipBalanced(t, j, "<", ">");
                if (j < t.Count && t[j].Text == "=") j = SkipType(t, j + 1);
                int first = (i > 0 && t[i - 1].Type == TT.Ident && t[i - 1].Text == "export") ? i - 1 : i;
                Remove(t, first, j);
                i = j - 1;
                continue;
            }

                                               
            if (k.Type == TT.Keyword && x == "local")
            {
                int j = i + 1;
                while (j < t.Count && t[j].Type == TT.Ident)
                {
                    j++;
                    if (j < t.Count && t[j].Text == ":")
                    {
                        int endType = SkipType(t, j + 1);
                        Remove(t, j, endType);
                        j = endType;
                    }
                    if (j < t.Count && t[j].Text == ",") j++;
                    else break;
                }
                continue;
            }

                                                                   
            if (k.Type == TT.Keyword && x == "function")
            {
                int j = i + 1;
                while (j < t.Count && t[j].Text != "(")
                {
                    if (t[j].Text == "<")
                    {
                        int endGeneric = SkipBalanced(t, j, "<", ">");
                        Remove(t, j, endGeneric);
                        j = endGeneric;
                        continue;
                    }
                    if (t[j].Type == TT.Keyword) break;
                    j++;
                }
                if (j >= t.Count || t[j].Text != "(") continue;

                int depth = 0, p = j;
                for (; p < t.Count; p++)
                {
                    string pt = t[p].Text;
                    if (pt == "(") depth++;
                    else if (pt == ")")
                    {
                        depth--;
                        if (depth == 0) break;
                    }
                    else if (depth == 1 && pt == ":" && p > 0 && (t[p - 1].Type == TT.Ident || t[p - 1].Text == "..."))
                    {
                        int endParam = SkipType(t, p + 1);
                        Remove(t, p, endParam);
                        p = endParam - 1;
                    }
                }

                if (p + 1 < t.Count && t[p + 1].Text == ":")
                {
                    int endReturn = SkipType(t, p + 2);
                    Remove(t, p + 1, endReturn);
                }
                continue;
            }

                                                                   
            if (k.Type == TT.Op && x == "::")
            {
                if (i + 2 < t.Count && t[i + 1].Type == TT.Ident && t[i + 2].Text == "::") { i += 2; continue; }
                int endCast = SkipType(t, i + 1);
                Remove(t, i, endCast);
                i = endCast - 1;
            }
        }
    }

                                                          
    static int SkipType(List<Tok> t, int i)
    {
        i = SkipTypeAtom(t, i);
        while (i < t.Count && (t[i].Text == "|" || t[i].Text == "&")) i = SkipTypeAtom(t, i + 1);
        return i;
    }

    static int SkipTypeAtom(List<Tok> t, int i)
    {
        if (i >= t.Count) return i;
        string x = t[i].Text;

        if (x == "(")
        {
            i = SkipBalanced(t, i, "(", ")");
            if (i < t.Count && t[i].Text == "->") i = SkipType(t, i + 1);
            return SkipQuestion(t, i);
        }

        if (x == "{") return SkipQuestion(t, SkipBalanced(t, i, "{", "}"));

        if (t[i].Type == TT.Ident || x == "nil")
        {
            string name = x;
            i++;
            while (i + 1 < t.Count && t[i].Text == "." && t[i + 1].Type == TT.Ident) i += 2;
            if (i < t.Count && t[i].Text == "<") i = SkipBalanced(t, i, "<", ">");
            else if (name == "typeof" && i < t.Count && t[i].Text == "(") i = SkipBalanced(t, i, "(", ")");
            return SkipQuestion(t, i);
        }

        if (t[i].Type == TT.Str) return SkipQuestion(t, i + 1);
        return i + 1;
    }

    static int SkipQuestion(List<Tok> t, int i)
    {
        while (i < t.Count && t[i].Text == "?") i++;
        return i;
    }

                                                                                       
    static int SkipBalanced(List<Tok> t, int i, string open, string close)
    {
        int depth = 0;
        for (; i < t.Count; i++)
        {
            if (t[i].Text == open) depth++;
            else if (t[i].Text == close)
            {
                depth--;
                if (depth == 0) return i + 1;
            }
        }
        return t.Count;
    }

                                                                   
                                                       
                                                                   

    static void CompoundAssign(List<Tok> t)
    {
        for (int i = 0; i < t.Count; i++)
        {
            var k = t[i];
            if (k.Type != TT.Op) continue;

            string op;
            switch (k.Text)
            {
                case "+=":  op = "+";  break;
                case "-=":  op = "-";  break;
                case "*=":  op = "*";  break;
                case "/=":  op = "/";  break;
                case "%=":  op = "%";  break;
                case "^=":  op = "^";  break;
                case "..=": op = ".."; break;
                default: continue;
            }

            int lhs = LhsStart(t, i);
            if (lhs < 0) continue;

            int rhsEnd = RhsEnd(t, i + 1);
            if (rhsEnd < i + 1) continue;

            k.Text = "= " + Join(t, lhs, i) + " " + op;
            t[i + 1].Text = "(" + t[i + 1].Text;
            t[rhsEnd].Text = t[rhsEnd].Text + ")";
        }
    }

                                                                                
    static int LhsStart(List<Tok> t, int opIdx)
    {
        int p = opIdx - 1;
        int start = -1;

        while (p >= 0)
        {
            var tk = t[p];

            if (tk.Type == TT.Ident)
            {
                start = p;
                p--;
                if (p >= 0 && t[p].Text == ".") { p--; continue; }
                break;
            }

            if (tk.Text == "]")
            {
                int depth = 0, q = p;
                for (; q >= 0; q--)
                {
                    if (t[q].Text == "]") depth++;
                    else if (t[q].Text == "[")
                    {
                        depth--;
                        if (depth == 0) break;
                    }
                }
                if (q < 0) return -1;
                start = q;
                p = q - 1;
                if (p >= 0 && (t[p].Type == TT.Ident || t[p].Text == "]")) continue;
                break;
            }

            break;
        }
        return start;
    }

                                                                                                      
    static int RhsEnd(List<Tok> t, int from)
    {
        int depth = 0;
        int last = from - 1;

        for (int k = from; k < t.Count; k++)
        {
            var tk = t[k];
            string x = tk.Text;
            if (x.Length == 0) continue;

            if (depth == 0 && k > from)
            {
                if (x == ";") break;
                if (tk.Prefix.IndexOf('\n') >= 0 && last >= from && !BinaryOps.Contains(t[last].Text) && !BinaryOps.Contains(x)) break;
            }

            if (x == "(" || x == "[" || x == "{") depth++;
            else if (x == ")" || x == "]" || x == "}")
            {
                depth--;
                if (depth < 0) break;
            }
            else if (tk.Type == TT.Keyword)
            {
                if (x == "function" || x == "if" || x == "do" || x == "repeat") depth++;
                else if (x == "end" || x == "until")
                {
                    if (depth == 0) break;
                    depth--;
                }
                else if ((x == "else" || x == "elseif") && depth == 0) break;
            }

            last = k;
        }
        return last;
    }

    static string Join(List<Tok> t, int from, int to)
    {
        var sb = new StringBuilder();
        for (int i = from; i < to; i++)
        {
            string x = t[i].Text;
            if (x.Length == 0) continue;
            if (sb.Length > 0 && IsWord(sb[sb.Length - 1]) && IsWord(x[0])) sb.Append(' ');
            sb.Append(x);
        }
        return sb.ToString();
    }

    static bool IsWord(char c) { return char.IsLetterOrDigit(c) || c == '_'; }

                                                                   
                
                                    
                                                                                       
                                                                  
                                                                   

    static void Continues(List<Tok> t)
    {
        var stack = new List<Blk>();
        var loops = new List<Blk>();
        var breaks = new List<KeyValuePair<int, Blk>>();
        var conts = new List<KeyValuePair<int, Blk>>();

        for (int i = 0; i < t.Count; i++)
        {
            var tk = t[i];
            string x = tk.Text;

            if (tk.Type == TT.Keyword)
            {
                if (x == "for" || x == "while")
                {
                    stack.Add(new Blk { Kind = "loop", AwaitDo = true, StartIdx = i });
                }
                else if (x == "do")
                {
                    Blk top = stack.Count > 0 ? stack[stack.Count - 1] : null;
                    if (top != null && top.AwaitDo) { top.AwaitDo = false; top.DoIdx = i; }
                    else stack.Add(new Blk { Kind = "other" });
                }
                else if (x == "repeat") stack.Add(new Blk { Kind = "repeat", StartIdx = i });
                else if (x == "function") stack.Add(new Blk { Kind = "func" });
                else if (x == "if") stack.Add(new Blk { Kind = "other" });
                else if (x == "end" || x == "until")
                {
                    if (stack.Count > 0)
                    {
                        Blk done = stack[stack.Count - 1];
                        stack.RemoveAt(stack.Count - 1);
                        done.EndIdx = i;
                        if (done.Kind == "loop" || done.Kind == "repeat") loops.Add(done);
                    }
                }
                else if (x == "break")
                {
                    Blk target = NearestLoop(stack);
                    if (target != null) breaks.Add(new KeyValuePair<int, Blk>(i, target));
                }
            }
            else if (tk.Type == TT.Ident && x == "continue" && IsContinueStatement(t, i))
            {
                Blk target = NearestLoop(stack);
                if (target != null)
                {
                    target.HasContinue = true;
                    conts.Add(new KeyValuePair<int, Blk>(i, target));
                }
            }
        }

        int id = 0;
        foreach (var b in loops)
            if (b.HasContinue && IsRewritable(b)) b.Id = ++id;

        foreach (var b in loops)
        {
            if (!b.HasContinue || !IsRewritable(b)) continue;
            string flag = "__c" + b.Id;
            if (b.Kind == "loop")
            {
                t[b.DoIdx].Text += " local " + flag + " = false repeat";
                t[b.EndIdx].Text = "until true if " + flag + " then break end end";
            }
            else
            {
                t[b.StartIdx].Text = "repeat local " + flag + " = false repeat";
                t[b.EndIdx].Text = "until true if " + flag + " then break end until";
            }
        }

        foreach (var kv in breaks)
            if (kv.Value.HasContinue && IsRewritable(kv.Value)) t[kv.Key].Text = "__c" + kv.Value.Id + " = true break";

        foreach (var kv in conts)
            if (IsRewritable(kv.Value)) t[kv.Key].Text = "break";
    }

    static bool IsRewritable(Blk b)
    {
        return b.EndIdx >= 0 && (b.Kind != "loop" || b.DoIdx >= 0);
    }

    static Blk NearestLoop(List<Blk> stack)
    {
        for (int s = stack.Count - 1; s >= 0; s--)
        {
            string kind = stack[s].Kind;
            if (kind == "func") return null;
            if (kind == "loop" || kind == "repeat") return stack[s];
        }
        return null;
    }

    static bool IsContinueStatement(List<Tok> t, int i)
    {
        if (i > 0 && (t[i - 1].Text == "." || t[i - 1].Text == ":")) return false;
        if (i + 1 < t.Count)
        {
            string next = t[i + 1].Text;
            if (next == "=" || next == "(" || next == "." || next == "[" || next == ":" || next == ",") return false;
        }
        return true;
    }
}