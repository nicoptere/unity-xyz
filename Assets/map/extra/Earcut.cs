using UnityEngine;
using System.Collections.Generic;



/*-------------------------------------------------------------------*/
/*  BruteForceEarCut.java                                            */
/*      This file contains the class needed for the implementation   */
/*      of ear cutting for a simple polygon.                         */
/*                                                                   */
/*- Modification History---------------------------------------------*/
/*  When:       Who:          Comments:                              */
/*                                                                   */
/*  97.12.09    Ian Garton    Final Implementation                   */
/*-------------------------------------------------------------------*/
//http://cgm.cs.mcgill.ca/~godfried/teaching/cg-projects/97/Ian//algorithm2.html
namespace Assets.map.extra
{

    public class Earcut{

        private int npoints;
        private int earCounter;
        private int concaveCount;
        private float[] xpoints, ypoints;
        private float[] xpointsTemp, ypointsTemp;
        private int[] ptType;
        private List<Vector2> transformedPoly;
        private List<Vector2>[] earPoly;
        static private Earcut instance;
        public Earcut()
        {
        }

        public int[] compute( List<Vector2> vertices )
        {
            classifyPoints(vertices);
            init(vertices);
            return doCutEar();
        }
        
        public void init( List<Vector2> vertices )
        {
            npoints = vertices.Count;
            earCounter = 0;
            xpoints = new float[npoints];
            ypoints = new float[npoints];
            earPoly = new List<Vector2>[npoints - 2];
            for (int i = 0; i < npoints - 2; i++)
            {
                earPoly[i] = new List<Vector2>();
            }
            for (int i = 0; i < npoints; i++)
            {
                xpoints[i] = vertices[i].x;
                ypoints[i] = vertices[i].y;
            }
            for (int i = 0; i < npoints - 2; i++)
            {
                earPoly[i] = null;
            }
            transformedPoly = null;
        }

        /* polygonClockwise:  Returns true if user inputted polygon in 
         *                    clockwise order, false if counterclockwise.  
         *                    The Law of Cosines is used to determine the
         *                    angle.
         */
        bool polygonClockwise()
        {
            float aa, bb, cc, b, c, theta;
            float convex_turn;
            float convex_sum = 0;

            for (int i = 0; i < npoints - 2; i++)
            {
                aa = ((xpoints[i + 2] - xpoints[i]) * (xpoints[i + 2] - xpoints[i])) +
                     ((-ypoints[i + 2] + ypoints[i]) * (-ypoints[i + 2] + ypoints[i]));

                bb = ((xpoints[i + 1] - xpoints[i]) * (xpoints[i + 1] - xpoints[i])) +
                     ((-ypoints[i + 1] + ypoints[i]) * (-ypoints[i + 1] + ypoints[i]));

                cc = ((xpoints[i + 2] - xpoints[i + 1]) *
                  (xpoints[i + 2] - xpoints[i + 1])) +
                 ((-ypoints[i + 2] + ypoints[i + 1]) *
                  (-ypoints[i + 2] + ypoints[i + 1]));

                b = Mathf.Sqrt(bb);
                c = Mathf.Sqrt(cc);
                theta = Mathf.Acos((bb + cc - aa) / (2 * b * c));

                if (convex(xpoints[i], ypoints[i],
                               xpoints[i + 1], ypoints[i + 1],
                               xpoints[i + 2], ypoints[i + 2]))
                {
                    convex_turn = Mathf.PI - theta;
                    convex_sum += convex_turn;
                }
                else
                {
                    convex_sum -= Mathf.PI - theta;
                }
            }
            aa = ((xpoints[1] - xpoints[npoints - 2]) *
                  (xpoints[1] - xpoints[npoints - 2])) +
                 ((-ypoints[1] + ypoints[npoints - 2]) *
                  (-ypoints[1] + ypoints[npoints - 2]));

            bb = ((xpoints[0] - xpoints[npoints - 2]) *
                  (xpoints[0] - xpoints[npoints - 2])) +
                 ((-ypoints[0] + ypoints[npoints - 2]) *
                  (-ypoints[0] + ypoints[npoints - 2]));

            cc = ((xpoints[1] - xpoints[0]) * (xpoints[1] - xpoints[0])) +
                 ((-ypoints[1] + ypoints[0]) * (-ypoints[1] + ypoints[0]));

            b = Mathf.Sqrt(bb);
            c = Mathf.Sqrt(cc);
            theta = Mathf.Acos((bb + cc - aa) / (2 * b * c));

            if (convex(xpoints[npoints - 2], ypoints[npoints - 2],
                           xpoints[0], ypoints[0],
                           xpoints[1], ypoints[1]))
            {
                convex_turn = Mathf.PI - theta;
                convex_sum += convex_turn;
            }
            else
            {
                convex_sum -= Mathf.PI - theta;
            }

            if (convex_sum >= (2 * 3.14159))
                return true;
            else
                return false;
        }


        /* classifyPoints:  Classifies points as "convex" or "concave".  
         *                  Convex points are represented as a "1" in the
         *                  ptType array; concave points are represented as a
         *                  "-1" in the array.
         */
        private void classifyPoints(List<Vector2> vertices )
        {
            npoints = vertices.Count;

            ptType = new int[npoints];
            xpointsTemp = new float[npoints];
            ypointsTemp = new float[npoints];
            concaveCount = 0;


            /* Before cutting any ears, we must determine if the polygon was
                 * inputted in clockwise order or not, since the algorithm for
                 * cutting ears assumes that the polygon's points are in clockwise
                 * order.  If the points are in counterclockwise order, they are
                 * simply reversed in the array.
                 */
            if (earCounter == 0)
            {
                if (!polygonClockwise())
                {
                    for (int i = 0; i < npoints; i++)
                    {
                        xpointsTemp[i] = xpoints[npoints - 1 - i];
                        ypointsTemp[i] = ypoints[npoints - 1 - i];
                    }
                    for (int i = 0; i < npoints - 1; i++)
                    {
                        xpoints[i] = xpointsTemp[i];
                        ypoints[i] = ypointsTemp[i];
                    }
                }
            }

            for (int i = 0; i < npoints - 1; i++)
            {
                if (i == 0)
                {
                    if (convex(xpoints[npoints - 2], ypoints[npoints - 2],
                                       xpoints[i], ypoints[i],
                                       xpoints[i + 1], ypoints[i + 1]))
                    {
                        ptType[i] = 1;  /* point is convex */
                    }
                    else
                    {
                        ptType[i] = -1; /* point is concave */
                        concaveCount++;
                    }
                }
                else
                {    /* i > 0 */
                    if (convex(xpoints[i - 1], ypoints[i - 1],
                                       xpoints[i], ypoints[i],
                                       xpoints[i + 1], ypoints[i + 1]))
                    {
                        ptType[i] = 1;  /* point is convex */
                    }
                    else
                    {
                        ptType[i] = -1; /* point is concave */
                        concaveCount++;
                    }
                }
            }
        }


        /* convex:  returns true if point (x2, y2) is convex
         */
        bool convex(float x1, float y1, float x2, float y2,float x3, float y3)
        {
            if (area(x1, y1, x2, y2, x3, y3) < 0)
                return true;
            else
                return false;
        }


        /* area:  determines area of triangle formed by three points
         */
        float area(float x1, float y1, float x2, float y2,float x3, float y3)
        {
            float areaSum = 0;

            areaSum += x1 * (y3 - y2);
            areaSum += x2 * (y1 - y3);
            areaSum += x3 * (y2 - y1);

            /* for actual area, we need to multiple areaSum * 0.5, but we are
                 * only interested in the sign of the area (+/-)
                 */

            return areaSum;
        }


        /* triangleContainsPoints:  returns true if the triangle formed by
         *                          three points contains another point
         */
        bool triangleContainsPoint(float x1, float y1, float x2,float y2, float x3, float y3)
        {
            int i = 0;
            float area1, area2, area3;
            bool noPointInTriangle = true;

            while ((i < npoints - 1) && (noPointInTriangle))
            {
                if ((ptType[i] == -1)   /* point is concave */  &&
                        (((xpoints[i] != x1) && (ypoints[i] != y1)) ||
                 ((xpoints[i] != x2) && (ypoints[i] != y2)) ||
                 ((xpoints[i] != x3) && (ypoints[i] != y3))))
                {

                    area1 = area(x1, y1, x2, y2, xpoints[i], ypoints[i]);
                    area2 = area(x2, y2, x3, y3, xpoints[i], ypoints[i]);
                    area3 = area(x3, y3, x1, y1, xpoints[i], ypoints[i]);

                    if (area1 > 0)
                        if ((area2 > 0) && (area3 > 0))
                            noPointInTriangle = false;
                    if (area1 < 0)
                        if ((area2 < 0) && (area3 < 0))
                            noPointInTriangle = false;
                }
                i++;
            }
            return !noPointInTriangle;
        }


        /* ear:  returns true if the point (x2, y2) is an ear, false
         *       otherwise
         */
        bool ear(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            if (concaveCount != 0)
                if (triangleContainsPoint(x1, y1, x2, y2, x3, y3))
                    return false;
                else
                    return true;
            else
                return true;
        }


        /* cutEar:  creates triangle that represents ear for graphics purposes
         */
        void cutEar(int index)
        {
            List<Vector2> p = new List<Vector2>();
            int i = 0;

            float[] ex, ey;
            ex = new float[4];
            ey = new float[4];

            if (index == 0)
            {
                ex[0] = xpoints[npoints - 2];
                ey[0] = ypoints[npoints - 2];
                ex[1] = xpoints[index];
                ey[1] = ypoints[index];
                ex[2] = xpoints[index + 1];
                ey[2] = ypoints[index + 1];
            }
            else if ((index > 0) && (index < npoints - 2))
            {
                ex[0] = xpoints[index - 1];
                ey[0] = ypoints[index - 1];
                ex[1] = xpoints[index];
                ey[1] = ypoints[index];
                ex[2] = xpoints[index + 1];
                ey[2] = ypoints[index + 1];
            }
            else if (index == npoints - 2)
            {
                ex[0] = xpoints[index - 1];
                ey[0] = ypoints[index - 1];
                ex[1] = xpoints[index];
                ey[1] = ypoints[index];
                ex[2] = xpoints[0];
                ey[2] = ypoints[0];
            }
            for (i = 0; i < 3; ++i)
                p.Add(new Vector2( ex[i], ey[i] ) );
            ex[3] = ex[0];
            ey[3] = ey[0];
            p.Add( new Vector2( ex[0], ey[0] ) );
            earPoly[earCounter] = p;
            earCounter++;
        }


        /* updatePolygon:  creates new polygon without the ear that was
         *                 cut
         */
        void updatePolygon(int index)
        {
            List<Vector2> p = new List<Vector2>();
            int new_i = 0;
            int i = 0;

            if (index == 0)
                i++;
            while (i < npoints - 1)
            {
                if (i == index)
                    i++;
                if (i < npoints - 1)
                {
                    xpoints[new_i] = xpoints[i];
                    ypoints[new_i] = ypoints[i];
                    p.Add(new Vector2( xpoints[new_i], ypoints[new_i] ) );
                    new_i++;
                    i++;
                }
            }
            xpoints[npoints - 2] = xpoints[0];
            ypoints[npoints - 2] = ypoints[0];
            p.Add(new Vector2( xpoints[0], ypoints[0]));
            npoints--;
            transformedPoly = p;

        }


        /* doCutEar:  Performs all the functions needed to find and cut an
         *            ear.
         */
        int[] doCutEar()
        {
            bool earHasBeenCut = false;
            int i = 0;

            while ((i < npoints - 1) && (!earHasBeenCut))
            {
                if (ptType[i] == 1)
                {  /* point is convex */
                    if (i == 0)
                    {
                        if (ear(xpoints[npoints - 2], ypoints[npoints - 2],
                            xpoints[i], ypoints[i],
                                        xpoints[i + 1], ypoints[i + 1]))
                        {
                            cutEar(i);
                            updatePolygon(i);
                            earHasBeenCut = true;
                        }
                    }
                    else
                    {    /* i > 0 */
                        if (ear(xpoints[i - 1], ypoints[i - 1],
                            xpoints[i], ypoints[i],
                                        xpoints[i + 1], ypoints[i + 1]))
                        {
                            cutEar(i);
                            updatePolygon(i);
                            earHasBeenCut = true;
                        }
                    }
                }
                i++;
            }

            for (int j = 0; j < earPoly.Length; j++)
            {
                Debug.Log(earPoly[j]);
            }

            return new int[] { 0, 1, 2 };
        }

    }

}
