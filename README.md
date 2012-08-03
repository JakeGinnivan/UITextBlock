UITextBlock
===========

Wpf TextBlock that supports UI Automation and some other nice things like:

Simply use the 'Label' type if using Project white. 

## Tooltip when trimmed
When you use TextTrimming="CharacterEllipsis" or TextTrimming="WordEllipsis", if the TextBlock has been trimmed you will get a tooltip automatically

## Shrink Font Size to fit

    <UITextBlockControl:UITextBlock Text="{Binding Something} ShrinkFontSizeToFit="True" 
                                    Grid.Row="3" HorizontalAlignment="Stretch" MinFontSize="10"
                                    TextTrimming="CharacterEllipsis" />

If the text cannot fit into the space, it will drop the font size until it hits the MinFontSize (or keep dropping to 1pt).
Once it hits the min it will start using TextTrimming.