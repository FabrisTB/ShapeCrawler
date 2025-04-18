﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using ShapeCrawler.Shapes;
using ShapeCrawler.Texts;
using P = DocumentFormat.OpenXml.Presentation;
using Picture = ShapeCrawler.Drawing.Picture;

namespace ShapeCrawler.GroupShapes;

internal sealed class GroupedShapeCollection(IEnumerable<OpenXmlCompositeElement> pGroupElements): IShapeCollection
{
    public int Count => this.GetGroupedShapes().Count;

    public IShape this[int index] => this.GetGroupedShapes()[index];

    public T GetById<T>(int id)
        where T : IShape => (T)this.GetGroupedShapes().First(shape => shape.Id == id);

    public T? TryGetById<T>(int id) 
        where T : IShape => (T?)this.GetGroupedShapes().FirstOrDefault(shape => shape.Id == id);

    T IShapeCollection.GetByName<T>(string name) => (T)this.GetGroupedShapes().First(shape => shape.Name == name);

    T? IShapeCollection.TryGetByName<T>(string name) 
        where T : default => (T?)this.GetGroupedShapes().FirstOrDefault(shape => shape.Name == name);

    public IShape GetByName(string name) => this.GetGroupedShapes().First(shape => shape.Name == name);

    public T Last<T>() 
        where T : IShape => (T)this.GetGroupedShapes().Last(shape => shape is T);

    public T GetByName<T>(string name) => (T)this.GetGroupedShapes().First(shape => shape.Name == name);

    public IEnumerator<IShape> GetEnumerator() => this.GetGroupedShapes().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    private List<IShape> GetGroupedShapes()
    {
        var groupedShapes = new List<IShape>();
        foreach (var pGroupShapeElement in pGroupElements)
        {
            IShape? shape = null;
            switch (pGroupShapeElement)
            {
                case P.GroupShape pGroupShape:
                    shape = new GroupShape(pGroupShape);
                    break;
                case P.Shape { TextBody: not null } pShape:
                    shape = new GroupedShape(
                        pShape,
                        new Shape(
                            pShape,
                            new TextBox(pShape.TextBody)));
                    break;
                case P.Shape pShape:
                    shape = new GroupedShape(
                        pShape,
                        new Shape(pShape));
                    break;
                case P.Picture pPicture:
                    {
                        var aBlip = pPicture.GetFirstChild<P.BlipFill>()?.Blip;
                        var blipEmbed = aBlip?.Embed;
                        if (blipEmbed is not null)
                        {
                            shape = new Picture(pPicture, aBlip!);
                        }

                        break;
                    } 

                default:
                    break;
            }

            if (shape != null)
            {
                groupedShapes.Add(shape);
            }
        }

        return groupedShapes;
    }
}