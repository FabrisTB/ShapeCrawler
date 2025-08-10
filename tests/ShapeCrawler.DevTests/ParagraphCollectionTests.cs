﻿using FluentAssertions;
using NUnit.Framework;

namespace ShapeCrawler.DevTests;

public class ParagraphCollectionTests
{
    [Test]
    public void Add_adds_paragraph_in_the_middle()
    {
        // Arrange
        var pres = new Presentation(pres =>
        {
            pres.Slide(slide =>
            {
                slide.TextBox(textBox =>
                {
                    textBox.Name("TextBox 1");
                    textBox.X(100);
                    textBox.Y(100);
                    textBox.Width(200);
                    textBox.Height(200);
                    textBox.Paragraph("Paragraph 1");
                    textBox.Paragraph("Paragraph 2");
                });
            });
        });
        var slide = pres.Slide(1);
        var addedShape = slide.Shapes.Last();
        var paragraphs = addedShape.TextBox!.Paragraphs;

        // Act
        paragraphs.Add("New Paragraph 2", 1);

        // Assert
        addedShape.TextBox.Text.Should().Be($"Paragraph 1{Environment.NewLine}New Paragraph 2{Environment.NewLine}Paragraph 2");
        pres.Validate();
    }
    
    [Test]
    public void Add_adds_paragraph_at_the_beginning()
    {
        // Arrange
        var pres = new Presentation(pres =>
        {
            pres.Slide(slide =>
            {
                slide.TextBox("TextBox 1", 100, 100, 200, 200, "Paragraph 1");
            });
        });
        var slide = pres.Slide(1);
        var addedShape = slide.Shapes.Last();
        var paragraphs = addedShape.TextBox!.Paragraphs;

        // Act
        paragraphs.Add("New Paragraph 1", 0);

        // Assert
        addedShape.TextBox.Text.Should().Be($"New Paragraph 1{Environment.NewLine}Paragraph 1");
        pres.Validate();
    }
}