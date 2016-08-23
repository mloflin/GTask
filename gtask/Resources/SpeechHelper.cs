using System;
using System.Windows;

namespace gTask.Resources
{
    public class SpeechHelper
    {
        //Takes a speech and returns formatted speech
        public static string FormatSpeech(int SelectionStart, string txtbxText, int SelectionEnd, string SpeakResult, string txtbxType)
        {
            string FinalText = string.Empty;
            string message = "Huh?, There was an issue understanding what you said.. can you try again?";
            try
            {
                int SelectionLength = SelectionEnd - SelectionStart;

                if (txtbxText.Length != 0) //If nothing is in the textbox then paste speech result
                {
                    //###
                    //###Pre-processing of Speak Result###
                    //###

                    bool ToLower = false; //Create bool for lowering first letter
                    bool RemoveEndPunctuation = false;
                    //Checking to see if SelectionEnd is same as Length and it ends in punctuation, if not then we can check punctuation
                    if (SelectionEnd != txtbxText.Length && char.IsPunctuation(SpeakResult.Substring(SpeakResult.Length - 1, 1)[0]))
                    {
                        //if the next letter is a punctuation and the SpeakResult ends in a punctuation then remove it and set ToLower = true to lowercase sentence
                        if (char.IsPunctuation(txtbxText.Substring(SelectionEnd, 1)[0]))
                        {
                            RemoveEndPunctuation = true;
                            ToLower = true;
                        }

                        //if one of the next two char is a letter then remove end punctuation and neither are capital
                        if (!char.IsWhiteSpace(txtbxText.Substring(SelectionEnd, 1)[0]))
                        {
                            if (char.IsLower(txtbxText.Substring(SelectionEnd, 1)[0]))
                            {
                                RemoveEndPunctuation = true;
                                //ToLower = true;
                            }

                        }
                        else //the next letter is a space and now I need to check the 2nd character
                        {
                            if (SelectionEnd < txtbxText.Length - 1) //make sure you can check two letters 
                            {
                                if (!char.IsWhiteSpace(txtbxText.Substring(SelectionEnd, 2)[1]) && char.IsLower(txtbxText.Substring(SelectionEnd, 2)[1])) //first letter has already been check in previous logic, only check 2nd character
                                {
                                    RemoveEndPunctuation = true;
                                    //ToLower = true;
                                }
                            }
                        }

                    }

                    //if first letter of txtbx selection is lower then make first letter of SpeakResult lower
                    if (SelectionLength > 0)
                    {
                        string txtbxFirstLetter = txtbxText.Substring(SelectionStart, 1);
                        if (Char.IsLower(txtbxFirstLetter[0]))
                        {
                            ToLower = true;
                        }
                    }

                    //If letter to the left of selection is space or lower then lower first letter
                    if (SelectionStart > 0) //can't do left character if = 0
                    {
                        string txtbxLeftLetter = txtbxText.Substring(SelectionStart - 1, 1);
                        if (Char.IsLower(txtbxLeftLetter[0]) || txtbxLeftLetter == " ")
                        {
                            if (SelectionStart <= 1)
                            {
                                ToLower = true;
                            }
                            else
                            {
                                string txtbxLeft2Letter = txtbxText.Substring(SelectionStart - 2, 1);
                                if (!char.IsPunctuation(txtbxLeft2Letter[0]))
                                {
                                    ToLower = true;
                                }
                            }
                        }
                    }

                    //Update first letter to lower if "ToLower" == true
                    if (ToLower == true)
                    {
                        string SpeakResultFirstLetter = Char.ToLower(SpeakResult[0]).ToString();
                        SpeakResult = SpeakResultFirstLetter + SpeakResult.Substring(1, SpeakResult.Length - 1);
                    }

                    //Remove trailing character if "RemoveEndPunctuation" == true
                    if (RemoveEndPunctuation == true)
                    {
                        SpeakResult = SpeakResult.Substring(0, SpeakResult.Length - 1);
                    }

                    //###
                    //###Processing of FinalText###
                    //###

                    //If SelectionStart is 0 or the character before the selection is a space or the 2 or three characters before include a linebreak, no space on the left is needed
                    if (SelectionStart == 0 || (SelectionStart > 0 && txtbxText.Substring(SelectionStart - 1, 1) == " ") || (SelectionStart > 1 && txtbxText.Substring(SelectionStart - 2, 2) == "\r") || (SelectionStart > 2 && txtbxText.Substring(SelectionStart - 3, 3).Contains("\r")))
                    {
                        FinalText = SpeakResult;
                    }
                    else
                    {
                        FinalText = " " + SpeakResult;
                    }
                    if (SelectionEnd != txtbxText.Length)
                    {
                        //if text isn't the end and the right isn't a space or punctuation or linebreak then add add a space
                        if (txtbxText.Substring(SelectionEnd, 1) == " " || char.IsPunctuation(txtbxText.Substring(SelectionEnd, 1)[0]) || (SelectionEnd < txtbxText.Length - 1 && txtbxText.Substring(SelectionEnd, 1).Contains("\r")))
                        {
                            //do nothing
                        }
                        else
                        {
                            FinalText += " ";
                        }
                    }
                }
                else
                {
                    FinalText = SpeakResult;
                }
            }
            catch
            {
                if (GTaskSettings.MsgError)
                {
                    MessageBox.Show(message);
                }
            }

            return FinalText;
        }
    }
}
