// FitNesse.NET
// Copyright � 2007 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System.Text;

namespace fit.Parser {
    public class HtmlString {
        private readonly bool isStandard;

        public HtmlString(string theHtml, bool isStandard) {
            myHtml = theHtml;
            this.isStandard = isStandard;
        }
        
        public string ToPlainText() {
            string result = myHtml != null ? UnEscape(UnFormat(myHtml)): string.Empty;
            foreach (char c in result) {
                if (c != ' ')
                    return result;
            }
            return string.Empty;
        }
        
        private string UnFormat(string theInput) {
            var result = new TextOutput(isStandard);
            var scan = new Scanner(theInput);
            while (true) {
                scan.FindTokenPair("<", ">", ourValidTagFilter);
                result.Append(scan.Leader);
                if (scan.Body.Length == 0) break;
                if (isStandard) result.AppendTag(GetTag(scan.Body));
            }
            return result.ToString();
        }
	    
        private static bool IsValidTag(Substring theBody) {
            return theBody[0] == '/' || char.IsLetter(theBody[0]);
        }
	    
        private static readonly TokenBodyFilter ourValidTagFilter = IsValidTag;
	    
        private static string GetTag(Substring theInput) {
            var tag = new StringBuilder();
            int i = 0;
            if (theInput[0] == '/') tag.Append(theInput[i++]);
            while (i < theInput.Length && char.IsLetter(theInput[i])) {
                tag.Append(theInput[i++]);
            }
            return tag.ToString().ToLower();
        }
	    
        private static string UnEscape(string theInput) {
            var scan = new Scanner(theInput);
            var result = new StringBuilder();
            while (true) {
                scan.FindTokenPair("&", ";");
                result.Append(scan.Leader);
                if (scan.Body.Length == 0) break;
                if (scan.Body.Equals("lt")) result.Append('<');
                else if (scan.Body.Equals("gt")) result.Append('>');
                else if (scan.Body.Equals("amp")) result.Append('&');
                else if (scan.Body.Equals("nbsp")) result.Append(' ');
                else if (scan.Body.Equals("quot")) result.Append('"');
                else {
                    result.Append('&');
                    result.Append(scan.Body);
                    result.Append(';');
                }
            }
            return result.ToString();
        }

        private readonly string myHtml;
	    
        private class TextOutput {

            private readonly bool isStandard;
	        
            public TextOutput(bool isStandard) {
                this.isStandard = isStandard;
                myText = new StringBuilder();
                myLastTag = string.Empty;
                myWhitespace = false;
            }
	        
            public void Append(Substring theInput) {
                for (int i = 0; i < theInput.Length; i++) {
                    char input = theInput[i];
                    if (isStandard && input != '\u00a0' && char.IsWhiteSpace(input)) {
                        if (!myWhitespace) {
                            myText.Append(' ');
                            myLastTag = myLastTag + " ";
                        }
                        myWhitespace = true;
                    }
                    else {
                        switch (input) {
                            case '\u201c':
                                input = '"'; break;
                            case '\u201d':
                                input = '"'; break;
                            case '\u2018':
                                input = '\''; break;
                            case '\u2019':
                                input = '\''; break;
                            case '\u00a0':
                                input = ' '; break;
                            case '&':
                                if (theInput.Contains(i + 1, "nbsp;")) {
                                    input = ' ';
                                    i += 5;
                                }
                                break;
                        }
                        myText.Append(input);
                        myWhitespace = false;
                        myLastTag = string.Empty;
                    }
                }
            }
	        
            public void AppendTag(string theInput) {
                if (theInput == "br") {
                    myText.Append("<br />");
                    myWhitespace = false;
                }
                else if (myLastTag.StartsWith("/p") && theInput == "p") {
                    if (myLastTag == "/p ") myText.Remove(myText.Length - 1, 1);
                    myWhitespace = false;
                    myText.Append("<br />");
                }
                myLastTag = theInput;
            }
	        
            public override string ToString() {
                return isStandard ? myText.ToString().Trim().Replace("<br>", "\n").Replace("<br />", "\n") : myText.ToString();
            }
	        
            private readonly StringBuilder myText;
            private string myLastTag;
            bool myWhitespace;
        }
    }
}