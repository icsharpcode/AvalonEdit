// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

using AcAvalonEdit.Utils;

namespace AcAvalonEdit.Highlighting
{
   /// <summary>
   /// A highlighting color is a set of font properties and foreground and background color.
   /// </summary>
   [Serializable]
   public class HighlightingColor : ISerializable, IFreezable, ICloneable, IEquatable<HighlightingColor>
   {
      internal static readonly HighlightingColor Empty = FreezableHelper.FreezeAndReturn(new HighlightingColor());

      string name;
      FontFamily fontFamily = null;
      int? fontSize;
      FontWeight? fontWeight;
      FontStyle? fontStyle;
      bool? underline;
      HighlightingBrush? underlineBrush;
      bool? strikethrough;
      bool? overline;
      TextAlignment? textAlignment;
      HighlightingBrush? overlineBrush;
      HighlightingBrush foreground;
      HighlightingBrush background;
      FontStretch? fontStretch;
      int? eolMarker;
      bool frozen;


      /// <summary>
      /// 
      /// </summary>
      public TextAlignment? TextAlignment
      {
         get
         {
            return textAlignment;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            textAlignment = value;
         }
      }

      /// <summary>
      /// Gets/Sets the EOLMarker flag
      /// </summary>
      public int? EOLMarker
      {
         get { return eolMarker; }
         set {
            if (frozen)
               throw new InvalidOperationException();
            eolMarker = value;
         }
      }
      /// <summary>
      /// Gets/sets the Overline flag
      /// </summary>
      public bool? Overline
      {
         get
         {
            return overline;
         }
         set
         {
            if (frozen)
            {
               throw new InvalidOperationException();
            }
            overline = value;
         }
      }

      /// <summary>
      /// Gets/sets the OverlineBrush, only used when Overline is to be a in a different color from Line
      /// </summary>
      public HighlightingBrush? OverlineBrush
      {
         get
         {
            return overlineBrush;
         }
         set
         {
            if (frozen)
            {
               throw new InvalidOperationException();
            }
            overlineBrush = value;
         }
      }


      /// <summary>
      /// Gets/Sets the name of the color.
      /// </summary>
      public string Name
      {
         get
         {
            return name;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            name = value;
         }
      }

      /// <summary>
      /// Gets/sets the font family. Null if the highlighting color does not change the font style.
      /// </summary>
      public FontFamily FontFamily
      {
         get
         {
            return fontFamily;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            fontFamily = value;
         }
      }

      /// <summary>
      /// Gets/sets the font size. Null if the highlighting color does not change the font style.
      /// </summary>
      public int? FontSize
      {
         get
         {
            return fontSize;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            fontSize = value;
         }
      }

      /// <summary>
      /// Gets/sets the font weight. Null if the highlighting color does not change the font weight.
      /// </summary>
      public FontWeight? FontWeight
      {
         get
         {
            return fontWeight;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            fontWeight = value;
         }
      }

      /// <summary>
      /// Gets/sets the font style. Null if the highlighting color does not change the font style.
      /// </summary>
      public FontStyle? FontStyle
      {
         get
         {
            return fontStyle;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            fontStyle = value;
         }
      }

      /// <summary>
      ///  Gets/sets the underline flag. Null if the underline status does not change the font style.
      /// </summary>
      public bool? Underline
      {
         get
         {
            return underline;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            underline = value;
         }
      }

      /// <summary>
      /// Gets/sets the UnderlineBrush, only used if Underline is to be a different color from the rest of the text
      /// </summary>
      public HighlightingBrush? UnderlineBrush
      {
         get
         {
            return underlineBrush;
         }
         set
         {
            if (frozen)
            {
               throw new InvalidOperationException();
            }
            underlineBrush = value;
         }
      }

      /// <summary>
      ///  Gets/sets the strikethrough flag. Null if the strikethrough status does not change the font style.
      /// </summary>
      public bool? Strikethrough
      {
         get
         {
            return strikethrough;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            strikethrough = value;
         }
      }

      /// <summary>
      /// Gets/sets the foreground color applied by the highlighting.
      /// </summary>
      public HighlightingBrush Foreground
      {
         get
         {
            return foreground;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            foreground = value;
         }
      }

      /// <summary>
      /// Gets/sets the background color applied by the highlighting.
      /// </summary>
      public HighlightingBrush Background
      {
         get
         {
            return background;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            background = value;
         }
      }

      /// <summary>
      /// Gets/sets the font strech applied by the highlighting
      /// </summary>
      public FontStretch? FontStretch
      {
         get
         {
            return fontStretch;
         }
         set
         {
            if (frozen)
               throw new InvalidOperationException();
            if (value == FontStretches.Medium)
               fontStretch = FontStretches.Normal;
            else
               fontStretch = value;
         }
      }

      /// <summary>
      /// Creates a new HighlightingColor instance.
      /// </summary>
      public HighlightingColor()
      {
      }

      /// <summary>
      /// Deserializes a HighlightingColor.
      /// </summary>
      protected HighlightingColor(SerializationInfo info, StreamingContext context)
      {
         if (info == null)
            throw new ArgumentNullException("info");
         this.Name = info.GetString("Name");
         if (info.GetBoolean("HasWeight"))
            this.FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(info.GetInt32("Weight"));
         if (info.GetBoolean("HasStyle"))
            this.FontStyle = (FontStyle?)new FontStyleConverter().ConvertFromInvariantString(info.GetString("Style"));
         if (info.GetBoolean("HasUnderline"))
            this.Underline = info.GetBoolean("Underline");
         if (info.GetBoolean("HasStrikethrough"))
            this.Strikethrough = info.GetBoolean("Strikethrough");
         this.Foreground = (HighlightingBrush)info.GetValue("Foreground", typeof(SimpleHighlightingBrush));
         this.Background = (HighlightingBrush)info.GetValue("Background", typeof(SimpleHighlightingBrush));
         if (info.GetBoolean("HasFamily"))
            this.FontFamily = new FontFamily(info.GetString("Family"));
         if (info.GetBoolean("HasSize"))
            this.FontSize = info.GetInt32("Size");
         if (info.GetBoolean("HasStretch"))
            this.FontStretch = (FontStretch?)new FontStretchConverter().ConvertFromInvariantString(info.GetString("Stretch"));
         if (info.GetBoolean("Overline"))
         {
            this.Overline = info.GetBoolean("Overline");
            this.OverlineBrush = (HighlightingBrush)info.GetValue("OverlineBrush", typeof(SimpleHighlightingBrush));
         }
         if (info.GetBoolean("HasEOLMarker"))
         {
            this.EOLMarker = info.GetInt32("EOLMarker");
         }
         if (info.GetBoolean("HasTextAlignment"))
         {
            this.TextAlignment = (System.Windows.TextAlignment)info.GetInt32("TextAlignment");
         }

      }

      /// <summary>
      /// Serializes this HighlightingColor instance.
      /// </summary>
      [System.Security.SecurityCritical]
      public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
      {
         if (info == null)
            throw new ArgumentNullException("info");
         info.AddValue("Name", this.Name);
         info.AddValue("HasWeight", this.FontWeight.HasValue);
         if (this.FontWeight.HasValue)
            info.AddValue("Weight", this.FontWeight.Value.ToOpenTypeWeight());
         info.AddValue("HasStyle", this.FontStyle.HasValue);
         if (this.FontStyle.HasValue)
            info.AddValue("Style", this.FontStyle.Value.ToString());
         info.AddValue("HasUnderline", this.Underline.HasValue);
         if (this.Underline.HasValue)
            info.AddValue("Underline", this.Underline.Value);
         info.AddValue("HasStrikethrough", this.Strikethrough.HasValue);
         if (this.Strikethrough.HasValue)
            info.AddValue("Strikethrough", this.Strikethrough.Value);
         info.AddValue("Foreground", this.Foreground);
         info.AddValue("Background", this.Background);
         info.AddValue("HasFamily", this.FontFamily != null);
         if (this.FontFamily != null)
            info.AddValue("Family", this.FontFamily.FamilyNames.FirstOrDefault());
         info.AddValue("HasSize", this.FontSize.HasValue);
         if (this.FontSize.HasValue)
            info.AddValue("Size", this.FontSize.Value.ToString());
         info.AddValue("HasStretch", this.FontStretch.HasValue);
         if (this.FontStretch.HasValue)
            info.AddValue("Stretch", this.FontStretch.Value.ToString());
         info.AddValue("Overline", overline.HasValue);
         if (this.Overline.HasValue)
         {
            info.AddValue("OverlineBrush", this.OverlineBrush);
         }
         info.AddValue("HasEOLMarker",this.EOLMarker.HasValue);
         if (this.EOLMarker.HasValue)
         {
            info.AddValue("EOLMarker",this.EOLMarker);
         }
         info.AddValue("HasTextAlignment", this.TextAlignment.HasValue);
         if (this.TextAlignment.HasValue)
         {
            info.AddValue("TextAlignment", this.TextAlignment);
         }
      }

      /// <summary>
      /// Gets CSS code for the color.
      /// </summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "CSS usually uses lowercase, and all possible values are English-only")]
      public virtual string ToCss()
      {
         StringBuilder b = new StringBuilder();
         if (Foreground != null)
         {
            Color? c = Foreground.GetColor(null);
            if (c != null)
            {
               b.AppendFormat(CultureInfo.InvariantCulture, "color: #{0:x2}{1:x2}{2:x2}; ", c.Value.R, c.Value.G, c.Value.B);
            }
         }
         if (Background != null)
         {
            Color? c = Background.GetColor(null);
            if (c != null)
            {
               b.AppendFormat(CultureInfo.InvariantCulture, "background-color: #{0:x2}{1:x2}{2:x2}; ", c.Value.R, c.Value.G, c.Value.B);
            }
         }
         if (FontWeight != null)
         {
            b.Append("font-weight: ");
            b.Append(FontWeight.Value.ToString().ToLowerInvariant());
            b.Append("; ");
         }
         if (FontStyle != null)
         {
            b.Append("font-style: ");
            b.Append(FontStyle.Value.ToString().ToLowerInvariant());
            b.Append("; ");
         }
         if (Underline != null)
         {
            b.Append("text-decoration: ");
            b.Append(Underline.Value ? "underline" : "none");
            b.Append("; ");
         }
         if (Strikethrough != null)
         {
            if (Underline == null)
               b.Append("text-decoration:  ");

            b.Remove(b.Length - 1, 1);
            b.Append(Strikethrough.Value ? " line-through" : " none");
            b.Append("; ");
         }
         if (Overline != null)
         {
            if (Underline == null && Strikethrough == null)
               b.Append("text-decoration:  ");

            b.Remove(b.Length - 1, 1);
            b.Append(Strikethrough.Value ? "overline" : " none");
            b.Append("; ");
         }
         if (FontStretch != null)
         {
            b.Append("font-stretch: ");
            b.Append(FontStretch.Value.ToString().ToLowerInvariant());
            b.Append("; ");
         }
         if (TextAlignment.HasValue)
         {
            b.Append("text-align: ");
            b.Append(TextAlignment.Value.ToString().ToLowerInvariant());
            b.Append("; ");
         }
         return b.ToString();
      }

      /// <inheritdoc/>
      public override string ToString()
      {
         return "[" + GetType().Name + " " + (string.IsNullOrEmpty(this.Name) ? ToCss() : this.Name) + "]";
      }

      /// <summary>
      /// Prevent further changes to this highlighting color.
      /// </summary>
      public virtual void Freeze()
      {
         frozen = true;
      }

      /// <summary>
      /// Gets whether this HighlightingColor instance is frozen.
      /// </summary>
      public bool IsFrozen
      {
         get { return frozen; }
      }

      /// <summary>
      /// Clones this highlighting color.
      /// If this color is frozen, the clone will be unfrozen.
      /// </summary>
      public virtual HighlightingColor Clone()
      {
         HighlightingColor c = (HighlightingColor)MemberwiseClone();
         c.frozen = false;
         return c;
      }

      object ICloneable.Clone()
      {
         return Clone();
      }

      /// <inheritdoc/>
      public override sealed bool Equals(object obj)
      {
         return Equals(obj as HighlightingColor);
      }

      /// <inheritdoc/>
      public virtual bool Equals(HighlightingColor other)
      {
         if (other == null)
            return false;
         return this.name == other.name && this.fontWeight == other.fontWeight
            && this.fontStyle == other.fontStyle && this.underline == other.underline && this.strikethrough == other.strikethrough && this.overline == other.overline
            && object.Equals(this.foreground, other.foreground) && object.Equals(this.background, other.background)
            && object.Equals(this.fontFamily, other.fontFamily) && object.Equals(this.FontSize, other.FontSize)
            && object.Equals(this.fontStretch, other.fontStretch) && object.Equals(this.OverlineBrush, other.OverlineBrush) && object.Equals(this.EOLMarker,other.EOLMarker)
            && object.Equals(this.TextAlignment,other.TextAlignment);
      }

      /// <inheritdoc/>
      public override int GetHashCode()
      {
         int hashCode = 0;
         unchecked
         {
            if (name != null)
               hashCode += 1000000007 * name.GetHashCode();
            hashCode += 1000000009 * fontWeight.GetHashCode();
            hashCode += 1000000021 * fontStyle.GetHashCode();
            if (foreground != null)
               hashCode += 1000000033 * foreground.GetHashCode();
            if (background != null)
               hashCode += 1000000087 * background.GetHashCode();
            if (fontFamily != null)
               hashCode += 1000000123 * fontFamily.GetHashCode();
            if (fontSize != null)
               hashCode += 1000000181 * fontSize.GetHashCode();
            if (fontStretch != null)
               hashCode += 1000000207 * fontStretch.GetHashCode();
            if (EOLMarker.HasValue)
               hashCode += 1000000409 * EOLMarker.GetHashCode();
            if (TextAlignment.HasValue)
               hashCode += 1000000411 * TextAlignment.GetHashCode();
            //1000000427	1000000433	1000000439	1000000447	1000000453
         }
         return hashCode;
      }

      /// <summary>
      /// Overwrites the properties in this HighlightingColor with those from the given color;
      /// but maintains the current values where the properties of the given color return <c>null</c>.
      /// </summary>
      public void MergeWith(HighlightingColor color)
      {
         FreezableHelper.ThrowIfFrozen(this);
         if (color.fontWeight != null)
            this.fontWeight = color.fontWeight;
         if (color.fontStyle != null)
            this.fontStyle = color.fontStyle;
         if (color.foreground != null)
            this.foreground = color.foreground;
         if (color.background != null)
            this.background = color.background;
         if (color.underline != null)
            this.underline = color.underline;
         if (color.strikethrough != null)
            this.strikethrough = color.strikethrough;
         if (color.fontFamily != null)
            this.fontFamily = color.fontFamily;
         if (color.fontSize != null)
            this.fontSize = color.fontSize;
         if (color.fontStretch != null)
            this.fontStretch = color.fontStretch;
         if (color.Overline != null)
            this.Overline = color.Overline;
         if (color.OverlineBrush != null)
            this.OverlineBrush = color.OverlineBrush;
         if (color.EOLMarker != null)
            this.EOLMarker = color.EOLMarker;
         if(TextAlignment != null)
            this.TextAlignment = color.TextAlignment;
      }

      internal bool IsEmptyForMerge
      {
         get
         {
            return fontWeight == null && fontStyle == null && underline == null
                  && strikethrough == null && foreground == null && background == null
                  && fontFamily == null && fontSize == null && fontStretch == null && overline == null
                  && overlineBrush == null && EOLMarker == null && TextAlignment == null
                  ;
         }
      }
   }
}
