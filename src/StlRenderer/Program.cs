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

namespace StlRenderer;

static class Program
{
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

        STLDocument stl;

        using var inStream = File.Open(@"D:\Dropbox\Documents\Desktop\ESP32-Ethgate.stl", FileMode.Open);
        
        var tempFilePath = Path.GetTempFileName();

        using var outStream = File.Create(tempFilePath);
        
        inStream.CopyTo(outStream);
        
        outStream.Close();

        stl = STLDocument.Open(tempFilePath);
        
        inStream.Close();

        try
        {
            File.Delete(tempFilePath);
        }
        catch { /* Ignore. */ }

        RenderStl(stl);
    }

    private static void RenderStl(STLDocument stl)
    {
        // this must called for console application to enable Avalonia framework
        // and must called before any other Avalonia control usage
        InitAvalonia();

        // create standalone Avalonia window for Silk.NET opengl rendering
        var w = GLWindow.Create();

        // define the GLModel build function
        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            // clear the model
            glModel.Clear();

            // place a point light at xyz=(2,2,2)
            glModel.PointLights.Add(new GLPointLight(2, 2, 2));

            var facetCounter = 0;
            
            foreach (var facet in stl.Facets)
            {
                var vector1 = new Vector3(facet.Vertices[0].X, facet.Vertices[0].Y, facet.Vertices[0].Z);
                
                var vector2 = new Vector3(
                    facet.Vertices[1].X - facet.Vertices[0].X, 
                    facet.Vertices[1].Y - facet.Vertices[0].Y, 
                    facet.Vertices[1].Z - facet.Vertices[0].Z);

                var xLine = GLLine.PointV(vector1, vector2, Color.Green, Color.Green);
                
                
                var vector3 = new Vector3(facet.Vertices[1].X, facet.Vertices[1].Y, facet.Vertices[1].Z);
                
                var vector4 = new Vector3(
                    facet.Vertices[2].X - facet.Vertices[1].X, 
                    facet.Vertices[2].Y - facet.Vertices[1].Y, 
                    facet.Vertices[2].Z - facet.Vertices[1].Z);

                var yLine = GLLine.PointV(vector3, vector4, Color.Red, Color.Red);
                    
                
                var vector5 = new Vector3(facet.Vertices[2].X, facet.Vertices[2].Y, facet.Vertices[2].Z);
                
                var vector6 = new Vector3(
                    facet.Vertices[0].X - facet.Vertices[2].X, 
                    facet.Vertices[0].Y - facet.Vertices[2].Y, 
                    facet.Vertices[0].Z - facet.Vertices[2].Z);
                
                var zLine = GLLine.PointV(vector5, vector6, Color.Blue, Color.Blue);

                
                glModel.AddFigure(new GLLineFigure(xLine, yLine, zLine));
            }
            
            glCtl.CameraView(CameraViewType.BackTopRight);
        };

        // show the gl window
        w.ShowSync();
    }

    private static Stream GetData(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(filename);

        if (stream == null)
        {
            throw new Exception($"Failed to load resource stream: {filename}");
        }
        else
        {
            return stream;
        }
    }
    
    private static void ValidateStl(STLDocument stl, int expectedFacetCount = 12)
    {
        Console.WriteLine($"stl.Facets.Count: {stl.Facets.Count}");


        foreach (var facet in stl.Facets)
        {
            //Console.WriteLine($"facet.Vertices.Count: {facet.Vertices.Count}");
        }
            
        
        // Assert.NotNull(stl);
        // Assert.Equal(expectedFacetCount, stl.Facets.Count);
        //
        // foreach (var facet in stl.Facets)
        //     Assert.Equal(3, facet.Vertices.Count);
    }


}