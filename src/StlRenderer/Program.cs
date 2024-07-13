using System.Drawing;
using System.Numerics;
using System.Reflection;
using QuantumConcepts.Formats.StereoLithography;
using SearchAThing.OpenGL.Core;
using SearchAThing.OpenGL.Shapes;

using Vector3 = System.Numerics.Vector3;
using Color = System.Drawing.Color;
using SearchAThing.OpenGL.Core;
using static SearchAThing.OpenGL.Core.Constants;
using SearchAThing.OpenGL.GUI;
using static SearchAThing.OpenGL.GUI.Toolkit;
using SearchAThing.OpenGL.Shapes;
using Silk.NET.OpenGL;

namespace StlRenderer;

static class Program
{
    private static STLDocument _stl;
    private static GLModel _glModel;

    static void Main(string[] args)
    {
        // Debug print args
        // for (var i = 0; i < args.Length; i++)
        // {
        //     if (!string.IsNullOrWhiteSpace(args[i]))
        //         Console.WriteLine($"Arg {i}: '{args[i]}'");    
        // }

        var stlPath = "";
        var saveFilename = "";

        // try
        // {
        //     stlPath = args[0];
        //     saveFilename = args[1];
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine("You MUST specify the full path to the STL as the first argument when calling this binary");
        //     Console.WriteLine("You MUST specify the full path to the jpg to output (including '.jpg' on the end) as the second argument when calling this binary");
        //     Console.WriteLine();
        // }

        using var inStream = File.Open(@"D:\Dropbox\Documents\Desktop\ESP32-Ethgate.stl", FileMode.Open);
        
        var tempFilePath = Path.GetTempFileName();

        using var outStream = File.Create(tempFilePath);
        
        inStream.CopyTo(outStream);
        
        outStream.Close();

        _stl = STLDocument.Open(tempFilePath);
        
        inStream.Close();

        try
        {
            File.Delete(tempFilePath);
        }
        catch { /* Ignore. */ }

        RenderStl();
    }

    private static void RenderStl()
    {
        // this must called for console application to enable Avalonia framework
        // and must called before any other Avalonia control usage
        InitAvalonia();

        // create standalone Avalonia window for Silk.NET opengl rendering
        var w = GLWindow.Create();

        // define the GLModel build function
        w.GLModel.BuildModel = async (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            _glModel = glCtl.GLModel;

            // clear the model
            _glModel.Clear();

            // place a point light at xyz=(2,2,2)
            _glModel.PointLights.Add(new GLPointLight(2, 2, 2));
            
            RenderSurfaces();   
            
            // Uncommenting this will draw facet outlines as lines over the surfaces
            //RenderWireframe();
            
            glCtl.CameraView(CameraViewType.BackTopRight);


            
            var renderDevice = new OffscreenRenderDevice(@"D:\Dropbox\Documents\Desktop\Test.bmp", new Size(100, 90));

            //renderDevice.TransferGLPixels(GL );
            
            
            
            
            
        };

        w.ShowSync();
        // show the gl window
        
        
        Console.WriteLine();
    }

    private static void RenderSurfaces()
    {
        foreach (var facet in _stl.Facets)
        {
            var a = new Vector3(facet.Vertices[0].X, facet.Vertices[0].Y, facet.Vertices[0].Z);
            var b = new Vector3(facet.Vertices[1].X, facet.Vertices[1].Y, facet.Vertices[1].Z);
            var c = new Vector3(facet.Vertices[2].X, facet.Vertices[2].Y, facet.Vertices[2].Z);

            var va = new GLVertex(a, Color.Green);
            var vb = new GLVertex(b, Color.Green);
            var vc = new GLVertex(c, Color.Green);

            var tri = new GLTriangle(va, vb, vc);

            // add triangle to the model
            _glModel.AddFigure(new GLTriangleFigure(tri));
        }
    }

    private static void RenderWireframe()
    {
        var colorToUse = Color.Black;
        
        foreach (var facet in _stl.Facets)
        {
            var vector1 = new Vector3(facet.Vertices[0].X, facet.Vertices[0].Y, facet.Vertices[0].Z);

            var vector2 = new Vector3(
                facet.Vertices[1].X - facet.Vertices[0].X,
                facet.Vertices[1].Y - facet.Vertices[0].Y,
                facet.Vertices[1].Z - facet.Vertices[0].Z);

            var xLine = GLLine.PointV(vector1, vector2, colorToUse, colorToUse);


            var vector3 = new Vector3(facet.Vertices[1].X, facet.Vertices[1].Y, facet.Vertices[1].Z);

            var vector4 = new Vector3(
                facet.Vertices[2].X - facet.Vertices[1].X,
                facet.Vertices[2].Y - facet.Vertices[1].Y,
                facet.Vertices[2].Z - facet.Vertices[1].Z);

            var yLine = GLLine.PointV(vector3, vector4, colorToUse, colorToUse);


            var vector5 = new Vector3(facet.Vertices[2].X, facet.Vertices[2].Y, facet.Vertices[2].Z);

            var vector6 = new Vector3(
                facet.Vertices[0].X - facet.Vertices[2].X,
                facet.Vertices[0].Y - facet.Vertices[2].Y,
                facet.Vertices[0].Z - facet.Vertices[2].Z);

            var zLine = GLLine.PointV(vector5, vector6, colorToUse, colorToUse);


            _glModel.AddFigure(new GLLineFigure(xLine, yLine, zLine));
        }
    }

    private static void FindFacetsWithMatchingVertexes()
    {
        var facetsSharingAVertex = new List<(Facet, Facet)>();

        foreach (var facet1 in _stl.Facets)
        {
            foreach (var facet2 in _stl.Facets)
            {
                if (!HasCoplanarVertex(facet1, facet2)) continue;
                
                facetsSharingAVertex.Add((facet1, facet2));
            }   
        }

        Console.WriteLine(facetsSharingAVertex.Count);
    }

    private static bool HasCoplanarVertex(Facet facet1, Facet facet2)
    {
        if (facet1 == facet2) return false;   // We don't want to report on checks of the same two facets
        
        var matchingPointsCounter = 0;
        
        // For each of the points in facet1
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator because it doesn't matter as these are not calculated, they are set
                if (facet1.Vertices[i].X == facet2.Vertices[j].X &&
                    facet1.Vertices[i].Y == facet2.Vertices[j].Y &&
                    facet1.Vertices[i].Z == facet2.Vertices[j].Z)
                {
                    matchingPointsCounter++;
                }
                
                if (matchingPointsCounter > 1)
                    return true;
            }   
        }

        return false;
    }
}
