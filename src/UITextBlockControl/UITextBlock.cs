using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using CCWin.App.Infrastructure.Controls;

namespace UITextBlockControl
{
    public class UITextBlock : TextBlock
    {
        private double? _originalFontSize;

        private static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterReadOnly(
            "IsTextTrimmed",
            typeof(bool),
            typeof(UITextBlock),
            new PropertyMetadata(false));

        private static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

        public static DependencyProperty ShrinkFontSizeToFitProperty = DependencyProperty.Register(
            "ShrinkFontSizeToFit", 
            typeof (bool), 
            typeof (UITextBlock), 
            new PropertyMetadata(ShrinkFontSizeToFitChanged));

        private static void ShrinkFontSizeToFitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uiTextBlock = (UITextBlock)d;
            uiTextBlock._originalFontSize = uiTextBlock.FontSize;
        }

        public static readonly DependencyProperty MinFontSizeProperty =
            DependencyProperty.Register("MinFontSize", typeof (double), typeof (UITextBlock), new PropertyMetadata(1d));

        public double MinFontSize
        {
            get { return (double) GetValue(MinFontSizeProperty); }
            set { SetValue(MinFontSizeProperty, value); }
        }

        public UITextBlock()
        {
            DefaultStyleKey = typeof (UITextBlock);

            SizeChanged += UITextBlockSizeChanged;
            Loaded += AddValueChangedToTextProperty;
            Unloaded += RemoveValueChangedToTextProperty;
        }

        private void RemoveValueChangedToTextProperty(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != this)
                return;

            var textDescriptor = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(UITextBlock));
            textDescriptor.RemoveValueChanged(this, TextChanged);
        }

        private void AddValueChangedToTextProperty(object sender, RoutedEventArgs e)
        {
            var textDescriptor = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(UITextBlock));
            textDescriptor.AddValueChanged(this, TextChanged);
        }

        public bool IsTextTrimmed
        {
            get { return GetIsTextTrimmed(this); }
            set { SetIsTextTrimmed(this, value); }
        }

        public bool ShrinkFontSizeToFit
        {
            get { return (bool) GetValue(ShrinkFontSizeToFitProperty); }
            set { SetValue(ShrinkFontSizeToFitProperty, value); }
        }

        public static bool GetIsTextTrimmed(DependencyObject o)
        {
            return (bool)o.GetValue(IsTextTrimmedProperty);
        }

        private static void SetIsTextTrimmed(DependencyObject target, bool value)
        {
            target.SetValue(IsTextTrimmedKey, value);
        }

        private static void TextChanged(object sender, EventArgs e)
        {
            var textBlock = sender as UITextBlock;
            if (null == textBlock)
            {
                return;
            }

            PerformShrinkIfNeeded(textBlock);

            SetIsTextTrimmed(textBlock, TextTrimming.None != textBlock.TextTrimming && CalculateIsTextTrimmed(textBlock));
        }

        private static void PerformShrinkIfNeeded(UITextBlock textBlock)
        {
            if (!textBlock.ShrinkFontSizeToFit)
                return;

            var newFontSize = textBlock.FontSize;
            var formattedText = BuildFormattedTextFrom(textBlock, newFontSize);

            var desiredWidth = textBlock.DesiredSize.Width;
            var maxWidth = BuildFormattedTextFrom(textBlock, textBlock._originalFontSize).Width;

            while (formattedText.Width > desiredWidth)
            {
                newFontSize -= 1;
                formattedText = BuildFormattedTextFrom(textBlock, newFontSize);
            }

            var width = BuildFormattedTextFrom(textBlock, newFontSize + 1).Width;
            while (width <= maxWidth && width < textBlock.ActualWidth && newFontSize < textBlock._originalFontSize)
            {
                newFontSize += 1;
                width = BuildFormattedTextFrom(textBlock, newFontSize + 1).Width;
            }

            if (Math.Abs(textBlock.FontSize - newFontSize) > 0.1 && newFontSize >= textBlock.MinFontSize)
                textBlock.FontSize = newFontSize;
        }

        static void UITextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var textBlock = sender as UITextBlock;
            if (null == textBlock)
                return;

            PerformShrinkIfNeeded(textBlock);

            SetIsTextTrimmed(textBlock, textBlock.TextTrimming != TextTrimming.None && CalculateIsTextTrimmed(textBlock));
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new UITextBlockAutomationPeer(this);
        }

        private static bool CalculateIsTextTrimmed(TextBlock textBlock)
        {
            var formattedText = BuildFormattedTextFrom(textBlock);
            formattedText.MaxTextWidth = textBlock.ActualWidth;

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            return (formattedText.Height > textBlock.ActualHeight);
        }

        private static FormattedText BuildFormattedTextFrom(TextBlock textBlock, double? fontSize = null)
        {
            var typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                fontSize ?? textBlock.FontSize,
                textBlock.Foreground);

            return formattedText;
        }
    }
}
