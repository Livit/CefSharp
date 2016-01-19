﻿// Copyright © 2010-2016 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CefSharp.Internals;

namespace CefSharp.Wpf.Internals
{
    public static class WpfExtensions
    {
        public static CefEventFlags GetModifiers(this MouseEventArgs e)
        {
            CefEventFlags modifiers = 0;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                modifiers |= CefEventFlags.LeftMouseButton;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                modifiers |= CefEventFlags.MiddleMouseButton;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                modifiers |= CefEventFlags.RightMouseButton;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                modifiers |= CefEventFlags.ControlDown | CefEventFlags.IsLeft;
            }

            if (Keyboard.IsKeyDown(Key.RightCtrl))
            {
                modifiers |= CefEventFlags.ControlDown | CefEventFlags.IsRight;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                modifiers |= CefEventFlags.ShiftDown | CefEventFlags.IsLeft;
            }

            if (Keyboard.IsKeyDown(Key.RightShift))
            {
                modifiers |= CefEventFlags.ShiftDown | CefEventFlags.IsRight;
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                modifiers |= CefEventFlags.AltDown | CefEventFlags.IsLeft;
            }

            if (Keyboard.IsKeyDown(Key.RightAlt))
            {
                modifiers |= CefEventFlags.AltDown | CefEventFlags.IsRight;
            }

            return modifiers;
        }

        public static CefEventFlags GetModifiers(this KeyEventArgs e)
        {
            CefEventFlags modifiers = 0;

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                modifiers |= CefEventFlags.ShiftDown;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                modifiers |= CefEventFlags.AltDown;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                modifiers |= CefEventFlags.ControlDown;
            }

            return modifiers;
        }

        public static CefDragDataWrapper GetDragDataWrapper(this DragEventArgs e)
        {
            // Convert Drag Data
            var dragData = CefDragDataWrapper.Create();

            // Files            
            dragData.IsFile = e.Data.GetDataPresent(DataFormats.FileDrop);
            if (dragData.IsFile)
            {
                // As per documentation, we only need to specify FileNames, not FileName, when dragging into the browser (http://magpcss.org/ceforum/apidocs3/projects/(default)/CefDragData.html)
                foreach (var filePath in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    var displayName = Path.GetFileName(filePath);

                    dragData.AddFile(filePath.Replace("\\", "/"), displayName);
                }
            }

            // Link/Url
            var link = GetLink(e.Data);
            dragData.IsLink = !string.IsNullOrEmpty(link);
            if (dragData.IsLink)
            {
                dragData.LinkUrl = link;
            }

            // Text/HTML
            dragData.IsFragment = e.Data.GetDataPresent(DataFormats.Text);
            if (dragData.IsFragment)
            {
                dragData.FragmentText = (string)e.Data.GetData(DataFormats.Text);
                dragData.FragmentHtml = (string)e.Data.GetData(DataFormats.Html);
            }

            return dragData;
        }

        private static string GetLink(IDataObject data)
        {
            const string asciiUrlDataFormatName = "UniformResourceLocator";
            const string unicodeUrlDataFormatName = "UniformResourceLocatorW";

            // Try Unicode
            if (data.GetDataPresent(unicodeUrlDataFormatName))
            {
                // Try to read a Unicode URL from the data
                var unicodeUrl = ReadUrlFromDragDropData(data, unicodeUrlDataFormatName, Encoding.Unicode);
                if (unicodeUrl != null)
                {
                    return unicodeUrl;
                }
            }

            // Try ASCII
            if (data.GetDataPresent(asciiUrlDataFormatName))
            {
                // Try to read an ASCII URL from the data
                return ReadUrlFromDragDropData(data, asciiUrlDataFormatName, Encoding.ASCII);
            }

            // Not a valid link
            return null;
        }

        /// <summary>Reads a URL using a particular text encoding from drag-and-drop data.</summary>
        /// <param name="data">The drag-and-drop data.</param>
        /// <param name="urlDataFormatName">The data format name of the URL type.</param>
        /// <param name="urlEncoding">The text encoding of the URL type.</param>
        /// <returns>A URL, or <see langword="null"/> if <paramref name="data"/> does not contain a URL
        /// of the correct type.</returns>
        private static string ReadUrlFromDragDropData(IDataObject data, string urlDataFormatName, Encoding urlEncoding)
        {
            // Read the URL from the data
            string url;
            using (var urlStream = (Stream)data.GetData(urlDataFormatName))
            {
                using (TextReader reader = new StreamReader(urlStream, urlEncoding))
                {
                    url = reader.ReadToEnd();
                }
            }

            // URLs in drag/drop data are often padded with null characters so remove these
            return url.TrimEnd('\0');
        }
    }
}
