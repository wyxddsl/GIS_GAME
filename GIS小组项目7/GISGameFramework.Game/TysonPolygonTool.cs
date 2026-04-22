//2352025 杨麟烨
using System;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Geometry;
using GISGameFramework.Game;

public class TysonPolygonTool
{
    public List<IPolygon> CreateTysonAreas(List<IPoint> points)
    {
        List<IPolygon> result = new List<IPolygon>();
        if (points == null || points.Count < 3)
            return result;

        List<Point2D> sites = new List<Point2D>();
        foreach (IPoint p in points)
        {
            sites.Add(new Point2D(p.X, p.Y));
        }

        double minX = sites.Min(p => p.X) - 0.005;
        double maxX = sites.Max(p => p.X) + 0.005;
        double minY = sites.Min(p => p.Y) - 0.005;
        double maxY = sites.Max(p => p.Y) + 0.005;

        foreach (var site in sites)
        {
            List<HalfEdge> edges = new List<HalfEdge>();
            edges.Add(new HalfEdge(new Point2D(minX, minY), new Point2D(maxX, minY)));
            edges.Add(new HalfEdge(new Point2D(maxX, minY), new Point2D(maxX, maxY)));
            edges.Add(new HalfEdge(new Point2D(maxX, maxY), new Point2D(minX, maxY)));
            edges.Add(new HalfEdge(new Point2D(minX, maxY), new Point2D(minX, minY)));

            foreach (var other in sites)
            {
                if (Math.Abs(site.X - other.X) < 1e-8 && Math.Abs(site.Y - other.Y) < 1e-8)
                    continue;

                Point2D mid = new Point2D((site.X + other.X) / 2, (site.Y + other.Y) / 2);
                Point2D dir = new Point2D(-(other.Y - site.Y), other.X - site.X);

                List<HalfEdge> newEdges = new List<HalfEdge>();
                List<Point2D> intersects = new List<Point2D>();

                foreach (var edge in edges)
                {
                    bool startIn = IsInside(edge.Start, site, mid, dir);
                    bool endIn = IsInside(edge.End, site, mid, dir);

                    if (startIn && endIn)
                    {
                        newEdges.Add(edge);
                    }
                    else if (startIn || endIn)
                    {
                        Point2D cross = GetIntersection(edge, mid, dir);
                        if (double.IsNaN(cross.X) || double.IsNaN(cross.Y))
                            continue;
                        intersects.Add(cross);
                        if (startIn)
                            newEdges.Add(new HalfEdge(edge.Start, cross));
                        else
                            newEdges.Add(new HalfEdge(cross, edge.End));
                    }
                }

                if (intersects.Count == 2)
                {
                    newEdges.Add(new HalfEdge(intersects[0], intersects[1]));
                }

                edges = newEdges;
                if (edges.Count == 0)
                    break;
            }

            if (edges.Count >= 3)
            {
                List<Point2D> polygonPoints = SortEdges(edges);
                if (polygonPoints.Count >= 3)
                {
                    IPolygon poly = new PolygonClass();
                    IPointCollection pc = poly as IPointCollection;
                    foreach (var p in polygonPoints)
                    {
                        pc.AddPoint(new PointClass() { X = p.X, Y = p.Y });
                    }
                    pc.AddPoint(new PointClass() { X = polygonPoints[0].X, Y = polygonPoints[0].Y });
                    result.Add(poly);
                }
            }
        }

        return result;
    }

    public List<IPoint> GetYour10CenterPoints()
    {
        List<IPoint> points = new List<IPoint>();
        foreach (var area in AreaManager.AllAreas)
        {
            points.Add(new PointClass() { X = area.CenterLon, Y = area.CenterLat });
        }
        return points;
    }

    private static bool IsInside(Point2D p, Point2D site, Point2D mid, Point2D dir)
    {
        double crossP = (p.X - mid.X) * dir.Y - (p.Y - mid.Y) * dir.X;
        double crossSite = (site.X - mid.X) * dir.Y - (site.Y - mid.Y) * dir.X;
        return crossP * crossSite >= 0;
    }

    private static Point2D GetIntersection(HalfEdge edge, Point2D lineMid, Point2D lineDir)
    {
        double x1 = edge.Start.X, y1 = edge.Start.Y;
        double x2 = edge.End.X, y2 = edge.End.Y;
        double x3 = lineMid.X, y3 = lineMid.Y;
        double x4 = lineMid.X + lineDir.X, y4 = lineMid.Y + lineDir.Y;

        double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (Math.Abs(denom) < 1e-8)
            return new Point2D(double.NaN, double.NaN);

        double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        if (t < 0 || t > 1)
            return new Point2D(double.NaN, double.NaN);

        return new Point2D(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
    }

    private static List<Point2D> SortEdges(List<HalfEdge> edges)
    {
        List<Point2D> result = new List<Point2D>();
        if (edges.Count == 0)
            return result;

        HalfEdge current = edges[0];
        edges.RemoveAt(0);
        result.Add(current.Start);
        result.Add(current.End);

        while (edges.Count > 0)
        {
            Point2D last = result[result.Count - 1];
            HalfEdge next = edges.Find(e => Math.Abs(e.Start.X - last.X) < 1e-8 && Math.Abs(e.Start.Y - last.Y) < 1e-8);
            if (next == null)
                next = edges.Find(e => Math.Abs(e.End.X - last.X) < 1e-8 && Math.Abs(e.End.Y - last.Y) < 1e-8);
            if (next == null)
                break;

            edges.Remove(next);
            if (Math.Abs(next.Start.X - last.X) < 1e-8 && Math.Abs(next.Start.Y - last.Y) < 1e-8)
                result.Add(next.End);
            else
                result.Add(next.Start);
        }

        return result;
    }
}

internal struct Point2D
{
    public double X;
    public double Y;
    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }
}

internal class HalfEdge
{
    public Point2D Start;
    public Point2D End;
    public HalfEdge(Point2D s, Point2D e)
    {
        Start = s;
        End = e;
    }
}