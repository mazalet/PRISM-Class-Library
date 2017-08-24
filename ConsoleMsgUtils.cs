﻿using System;

namespace PRISM
{
    /// <summary>
    /// This class includes methods to be used when displaying messages at the console while monitoring a class that inherits clsEventNotifier
    /// </summary>
    public static class ConsoleMsgUtils
    {
        /// <summary>
        /// Debug message font color
        /// </summary>
        public static ConsoleColor DebugFontColor = ConsoleColor.DarkGray;

        /// <summary>
        /// Error message font color
        /// </summary>
        public static ConsoleColor ErrorFontColor = ConsoleColor.Red;

        /// <summary>
        /// Stack trace font color
        /// </summary>
        public static ConsoleColor StackTraceFontColor = ConsoleColor.Cyan;

        /// <summary>
        /// Warning message font color
        /// </summary>
        public static ConsoleColor WarningFontColor = ConsoleColor.Yellow;

        /// <summary>
        /// Display an error message at the console with color ErrorFontColor (defaults to Red)
        /// If an exception is included, the stack trace is shown using StackTraceFontColor
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Exception (can be null)</param>
        /// <param name="includeSeparator">When true, add a separator line before and after the error</param>
        /// <param name="writeToErrorStream">When true, also send the error to the the standard error stream</param>
        public static void ShowError(string message, Exception ex = null, bool includeSeparator = true, bool writeToErrorStream = true)
        {
            const string SEPARATOR = "------------------------------------------------------------------------------";

            Console.WriteLine();
            if (includeSeparator)
            {
                Console.WriteLine(SEPARATOR);
            }

            string formattedError;
            if (ex == null || message.EndsWith(ex.Message))
            {
                formattedError = message;
            }
            else
            {
                formattedError = message + ": " + ex.Message;
            }

            Console.ForegroundColor = ErrorFontColor;
            Console.WriteLine(formattedError);

            if (ex != null)
            {
                Console.ForegroundColor = StackTraceFontColor;
                Console.WriteLine();
                Console.WriteLine(clsStackTraceFormatter.GetExceptionStackTraceMultiLine(ex));
            }

            Console.ResetColor();

            if (includeSeparator)
            {
                Console.WriteLine(SEPARATOR);
            }
            Console.WriteLine();

            if (writeToErrorStream)
            {
                WriteToErrorStream(message);
            }
        }

        /// <summary>
        /// Display a debug message at the console with color DebugFontColor (defaults to DarkGray)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="indentChars">Characters to use to indent the message</param>
        public static void ShowDebug(string message, string indentChars = "  ")
        {
            Console.WriteLine();
            Console.ForegroundColor = DebugFontColor;
            if (string.IsNullOrEmpty(indentChars))
            {
                Console.WriteLine(indentChars + message);
            }
            else
            {
                Console.WriteLine(indentChars + message);
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Display a warning message at the console with color WarningFontColor (defaults to Yellow)
        /// </summary>
        /// <param name="message"></param>
        public static void ShowWarning(string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = WarningFontColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Write a message to the error stream
        /// </summary>
        /// <param name="strErrorMessage"></param>
        public static void WriteToErrorStream(string strErrorMessage)
        {
            try
            {
                using (var swErrorStream = new System.IO.StreamWriter(Console.OpenStandardError()))
                {
                    swErrorStream.WriteLine(strErrorMessage);
                }
            }
            catch
            {
                // Ignore errors here
            }
        }

    }
}
