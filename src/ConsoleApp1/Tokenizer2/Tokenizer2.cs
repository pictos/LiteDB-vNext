﻿using System.Text;

namespace LiteDB;

/// <summary>
/// Class to tokenize TextReader input used in JsonRead/BsonExpressions
/// This class are not thread safe
/// </summary>
internal class Tokenizer2
{
    private TextReader _reader;
    private char _char = '\0';
    private Token2? _current = null;
    private Token2? _ahead = null;
    private bool _eof = false;
    private long _position = 0;

    public bool EOF => _eof && _ahead == null;
    public long Position => _position;
    public Token2 Current => _current!;

    /// <summary>
    /// If EOF throw an invalid token exception (used in while()) otherwise return "false" (not EOF)
    /// </summary>
    public bool CheckEOF()
    {
        if (_eof) throw new Exception(this.Current.ToString());

        return false;
    }

    public Tokenizer2(string source)
        : this(new StringReader(source))
    {
    }

    public Tokenizer2(TextReader reader)
    {
        _reader = reader;

        _position = 0;
        this.ReadChar();
    }

    /// <summary>
    /// Checks if char is an valid part of a word [a-Z_]+[a-Z0-9_$]*
    /// </summary>
    public static bool IsWordChar(char c, bool first)
    {
        if (first)
        {
            return char.IsLetter(c) || c == '_' || c == '$';
        }

        return char.IsLetterOrDigit(c) || c == '_' || c == '$';
    }

    /// <summary>
    /// Read next char in stream and set in _current
    /// </summary>
    private char ReadChar()
    {
        if (_eof) return '\0';

        var c = _reader.Read();

        _position++;

        if (c == -1)
        {
            _char = '\0';
            _eof = true;
        }
        else
        {
            _char = (char)c;
        }

        return _char;
    }

    /// <summary>
    /// Look for next token but keeps in buffer when run "ReadToken()" again.
    /// </summary>
    public Token2 LookAhead(bool eatWhitespace = true, int tokensCount = 1)
    {
        if (_ahead != null)
        {
            if (eatWhitespace && _ahead.Type == TokenType2.Whitespace)
            {
                _ahead = this.ReadNext(eatWhitespace);
            }

            return _ahead;
        }

        return _ahead = this.ReadNext(eatWhitespace);
    }

    /// <summary>
    /// Read next token (or from ahead buffer).
    /// </summary>
    public Token2 ReadToken(bool eatWhitespace = true)
    {
        if (_ahead == null)
        {
            return _current = this.ReadNext(eatWhitespace);
        }

        if (eatWhitespace && _ahead.Type == TokenType2.Whitespace)
        {
            _ahead = this.ReadNext(eatWhitespace);
        }

        _current = _ahead;
        _ahead = null;
        return _current;
    }

    /// <summary>
    /// Read next token from reader
    /// </summary>
    private Token2 ReadNext(bool eatWhitespace)
    {
        // remove whitespace before get next token
        if (eatWhitespace) this.EatWhitespace();

        if (_eof)
        {
            return new Token2(TokenType2.EOF, "", _position);
        }

        Token2? token = null;

        switch (_char)
        {
            case '{':
                token = new Token2(TokenType2.OpenBrace, "{", _position);
                this.ReadChar();
                break;

            case '}':
                token = new Token2(TokenType2.CloseBrace, "}", _position);
                this.ReadChar();
                break;

            case '[':
                token = new Token2(TokenType2.OpenBracket, "[", _position);
                this.ReadChar();
                break;

            case ']':
                token = new Token2(TokenType2.CloseBracket, "]", _position);
                this.ReadChar();
                break;

            case '(':
                token = new Token2(TokenType2.OpenParenthesis, "(", _position);
                this.ReadChar();
                break;

            case ')':
                token = new Token2(TokenType2.CloseParenthesis, ")", _position);
                this.ReadChar();
                break;

            case ',':
                token = new Token2(TokenType2.Comma, ",", _position);
                this.ReadChar();
                break;

            case ':':
                token = new Token2(TokenType2.Colon, ":", _position);
                this.ReadChar();
                break;

            case ';':
                token = new Token2(TokenType2.SemiColon, ";", _position);
                this.ReadChar();
                break;

            case '@':
                token = new Token2(TokenType2.At, "@", _position);
                this.ReadChar();
                break;

            case '#':
                token = new Token2(TokenType2.Hashtag, "#", _position);
                this.ReadChar();
                break;

            case '~':
                token = new Token2(TokenType2.Til, "~", _position);
                this.ReadChar();
                break;

            case '.':
                token = new Token2(TokenType2.Period, ".", _position);
                this.ReadChar();
                break;

            case '&':
                token = new Token2(TokenType2.Ampersand, "&", _position);
                this.ReadChar();
                break;

            case '$':
                this.ReadChar();
                if (IsWordChar(_char, true))
                {
                    token = new Token2(TokenType2.Word, "$" + this.ReadWord(), _position);
                }
                else
                {
                    token = new Token2(TokenType2.Dollar, "$", _position);
                }
                break;

            case '!':
                this.ReadChar();
                if (_char == '=')
                {
                    token = new Token2(TokenType2.NotEquals, "!=", _position);
                    this.ReadChar();
                }
                else
                {
                    token = new Token2(TokenType2.Exclamation, "!", _position);
                }
                break;

            case '=':
                this.ReadChar();
                if (_char == '>')
                {
                    token = new Token2(TokenType2.Arrow, "=>", _position);
                    this.ReadChar();
                }
                else
                {
                    token = new Token2(TokenType2.Equals, "=", _position);
                }
                break;

            case '>':
                this.ReadChar();
                if (_char == '=')
                {
                    token = new Token2(TokenType2.GreaterOrEquals, ">=", _position);
                    this.ReadChar();
                }
                else
                {
                    token = new Token2(TokenType2.Greater, ">", _position);
                }
                break;

            case '<':
                this.ReadChar();
                if (_char == '=')
                {
                    token = new Token2(TokenType2.LessOrEquals, "<=", _position);
                    this.ReadChar();
                }
                else
                {
                    token = new Token2(TokenType2.Less, "<", _position);
                }
                break;

            case '-':
                this.ReadChar();
                if (_char == '-')
                {
                    this.ReadLine(); // comment
                    token = this.ReadNext(eatWhitespace);
                }
                else
                {
                    token = new Token2(TokenType2.Minus, "-", _position);
                }
                break;

            case '+':
                token = new Token2(TokenType2.Plus, "+", _position);
                this.ReadChar();
                break;

            case '*':
                token = new Token2(TokenType2.Asterisk, "*", _position);
                this.ReadChar();
                break;

            case '/':
                token = new Token2(TokenType2.Slash, "/", _position);
                this.ReadChar();
                break;
            case '\\':
                token = new Token2(TokenType2.Backslash, @"\", _position);
                this.ReadChar();
                break;

            case '%':
                token = new Token2(TokenType2.Percent, "%", _position);
                this.ReadChar();
                break;

            case '\"':
            case '\'':
                token = new Token2(TokenType2.String, this.ReadString(_char), _position);
                break;

            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                var dbl = false;
                var number = this.ReadNumber(ref dbl);
                token = new Token2(dbl ? TokenType2.Double : TokenType2.Int, number, _position);
                break;

            case ' ':
            case '\n':
            case '\r':
            case '\t':
                var sb = new StringBuilder();
                while(char.IsWhiteSpace(_char) && !_eof)
                {
                    sb.Append(_char);
                    this.ReadChar();
                }
                token = new Token2(TokenType2.Whitespace, sb.ToString(), _position);
                break;

            default:
                // test if first char is an word 
                if (IsWordChar(_char, true))
                {
                    token = new Token2(TokenType2.Word, this.ReadWord(), _position);
                }
                else
                {
                    this.ReadChar();
                }
                break;
        }

        return token ?? new Token2(TokenType2.Unknown, _char.ToString(), _position);
    }

    /// <summary>
    /// Eat all whitespace - used before a valid token
    /// </summary>
    private void EatWhitespace()
    {
        while (char.IsWhiteSpace(_char) && !_eof)
        {
            this.ReadChar();
        }
    }

    /// <summary>
    /// Read a word (word = [\w$]+)
    /// </summary>
    private string ReadWord()
    {
        var sb = new StringBuilder();
        sb.Append(_char);

        this.ReadChar();

        while (!_eof && IsWordChar(_char, false))
        {
            sb.Append(_char);
            this.ReadChar();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Read a number - it's accepts all number char, but not validate. When run Convert, .NET will check if number is correct
    /// </summary>
    private string ReadNumber(ref bool dbl)
    {
        var sb = new StringBuilder();
        sb.Append(_char);

        var canDot = true;
        var canE = true;
        var canSign = false;

        this.ReadChar();

        while (!_eof &&
            (char.IsDigit(_char) || _char == '+' || _char == '-' || _char == '.' || _char == 'e' || _char == 'E'))
        {
            if (_char == '.')
            {
                if (canDot == false) break;
                dbl = true;
                canDot = false;
            }
            else if (_char == 'e' || _char == 'E')
            {
                if (canE == false) break;
                canE = false;
                canSign = true;
                dbl = true;
            }
            else if (_char == '-' || _char == '+')
            {
                if (canSign == false) break;
                canSign = false;
            }

            sb.Append(_char);
            this.ReadChar();
        }

        return sb.ToString();
    }
        
    /// <summary>
    /// Read a string removing open and close " or '
    /// </summary>
    private string ReadString(char quote)
    {
        var sb = new StringBuilder();
        this.ReadChar(); // remove first " or '

        while (_char != quote && !_eof)
        {
            if (_char == '\\')
            {
                this.ReadChar();

                if (_char == quote) sb.Append(quote);

                switch (_char)
                {
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        var codePoint = ParseUnicode(this.ReadChar(), this.ReadChar(), this.ReadChar(), this.ReadChar());
                        sb.Append((char)codePoint);
                        break;
                }
            }
            else
            {
                sb.Append(_char);
            }

            this.ReadChar();
        }

        this.ReadChar(); // read last " or '

        return sb.ToString();
    }

    /// <summary>
    /// Read all chars to end of LINE
    /// </summary>
    private void ReadLine()
    {
        // remove all char until new line
        while (_char != '\n' && !_eof)
        {
            this.ReadChar();
        }
        if (_char == '\n') this.ReadChar();
    }

    public static uint ParseUnicode(char c1, char c2, char c3, char c4)
    {
        uint p1 = ParseSingleChar(c1, 0x1000);
        uint p2 = ParseSingleChar(c2, 0x100);
        uint p3 = ParseSingleChar(c3, 0x10);
        uint p4 = ParseSingleChar(c4, 1);

        return p1 + p2 + p3 + p4;
    }

    public static uint ParseSingleChar(char c1, uint multiplier)
    {
        uint p1 = 0;
        if (c1 >= '0' && c1 <= '9')
            p1 = (uint)(c1 - '0') * multiplier;
        else if (c1 >= 'A' && c1 <= 'F')
            p1 = (uint)((c1 - 'A') + 10) * multiplier;
        else if (c1 >= 'a' && c1 <= 'f')
            p1 = (uint)((c1 - 'a') + 10) * multiplier;
        return p1;
    }

    public override string ToString()
    {
        return _current?.ToString() + " [ahead: " + _ahead?.ToString() + "] - position: " + _position;
    }
}
