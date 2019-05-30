﻿using System;
using System.Drawing;

namespace SoftwareRenderer
{
    /// <summary>
    /// 将三角形拆分为平底三角形+平顶三角形，通过直接扫描三角形来进行光栅化
    ///      A     
    ///      *
    ///     * *
    ///    *   *
    ///   *     *
    /// B*-------*M
    ///    *      *
    ///      *     *
    ///        *    *
    ///          *   *
    ///            *  *
    ///              * *
    ///                 *
    ///                 C
    /// 参考文献：
    /// Standard Algorithm - http://www.sunshine2k.de/coding/java/TriangleRasterization/TriangleRasterization.html#algo1
    /// </summary>
    class RasterizerStandard : Rasterizer
    {
        public override void Do(Vector pa, Vector pb, Vector pc,
                                Color  ca, Color  cb, Color  cc,
                                UV     ua, UV     ub, UV     uc)
        {
            fragments.Clear();

            Sort(ref pa, ref pb, ref pc,
                 ref ca, ref cb, ref cc,
                 ref ua, ref ub, ref uc);

            if (Math.Abs(pb.y - pc.y) <= float.Epsilon)
            {
                RasterizeBottomTriangle(pa, pb, pc,
                                        ca, cb, cc,
                                        ua, ub, uc);
            }
            else if (Math.Abs(pa.y - pb.y) <= float.Epsilon)
            {
                RasterizeTopTriangle(pa, pb, pc,
                                     ca, cb, cc,
                                     ua, ub, uc);
            }
            else
            {
                float pmz = 0;//TODO 插值得到
                Vector pm = new Vector(pa.x + (pb.y - pa.y) / (pc.y - pa.y) * (pc.x - pa.x),
                                       pb.y,
                                       pmz, 
                                       0);
                Color cm = cc;//TODO 插值得到
                UV um = uc;//TODO 插值得到

                RasterizeBottomTriangle(pa, pb, pm,
                                        ca, cb, cm,
                                        ua, ub, um);

                RasterizeTopTriangle(pb, pm, pc,
                                     cb, cm, cc,
                                     ub, um, uc);
            }
        }

        private void Sort(ref Vector pa, ref Vector pb, ref Vector pc,
                          ref Color  ca, ref Color  cb, ref Color  cc,
                          ref UV     ua, ref UV     ub, ref UV     uc)
        {
            Vector pt;
            Color  ct;
            UV     ut;

            if (pa.y > pb.y)
            {
                pt = pa;
                pa = pb;
                pb = pt;

                ct = ca;
                ca = cb;
                cb = ct;

                ut = ua;
                ua = ub;
                ub = ut;
            }

            if (pa.y > pc.y)
            {
                pt = pa;
                pa = pc;
                pc = pt;

                ct = ca;
                ca = cc;
                cc = ct;

                ut = ua;
                ua = uc;
                uc = ut;
            }

            if (pb.y > pc.y)
            {
                pt = pb;
                pb = pc;
                pc = pt;

                ct = cb;
                cb = cc;
                cc = ct;

                ut = ub;
                ub = uc;
                uc = ut;
            }
        }

        private void RasterizeTopTriangle(Vector pa, Vector pb, Vector pc,
                                          Color  ca, Color  cb, Color  cc,
                                          UV     ua, UV     ub, UV     uc)
        {
            float invslope_ca = (pc.x - pa.x) / (pc.y - pa.y);
            float invslope_cb = (pc.x - pb.x) / (pc.y - pb.y);

            float x_ca = pc.x;
            float x_cb = pc.x;

            if (invslope_ca > invslope_cb)
            {
                for (int y = (int)pc.y; y >= (int)pa.y; y--)
                {
                    for (int x = (int)x_ca; x <= (int)x_cb; x++)
                    {
                        Fragment fg = new Fragment();
                        fg.x = x;
                        fg.y = y;
                        //TODO 还需要完成depth、color和uv的插值

                        fragments.Add(fg);
                    }
                    x_ca -= invslope_ca;
                    x_cb -= invslope_cb;
                }
            }
            else
            {
                for (int y = (int)pc.y; y >= (int)pa.y; y--)
                {
                    for (int x = (int)x_cb; x <= (int)x_ca; x++)
                    {
                        Fragment fg = new Fragment();
                        fg.x = x;
                        fg.y = y;
                        //TODO 还需要完成depth、color和uv的插值

                        fragments.Add(fg);
                    }
                    x_ca -= invslope_ca;
                    x_cb -= invslope_cb;
                }
            }
        }

        private void RasterizeBottomTriangle(Vector pa, Vector pb, Vector pc,
                                             Color  ca, Color  cb, Color  cc,
                                             UV     ua, UV     ub, UV     uc)
        {
            float invslope_ab = (pa.x - pb.x) / (pa.y - pb.y);
            float invslope_ac = (pa.x - pc.x) / (pa.y - pc.y);

            float x_ab = pa.x;
            float x_ac = pa.x;

            if (invslope_ab < invslope_ac)
            {
                for (int y = (int)pa.y; y <= (int)pb.y; y++)
                {
                    for (int x = (int)x_ab; x <= (int)x_ac; x++)
                    {
                        Fragment fg = new Fragment();
                        fg.x = x;
                        fg.y = y;
                        //TODO 还需要完成depth、color和uv的插值

                        fragments.Add(fg);
                    }
                    x_ab += invslope_ab;
                    x_ac += invslope_ac;
                }
            }
            else
            {
                for (int y = (int)pa.y; y <= (int)pb.y; y++)
                {
                    for (int x = (int)x_ac; x <= (int)x_ab; x++)
                    {
                        Fragment fg = new Fragment();
                        fg.x = x;
                        fg.y = y;
                        //TODO 还需要完成depth、color和uv的插值

                        fragments.Add(fg);
                    }
                    x_ab += invslope_ab;
                    x_ac += invslope_ac;
                }
            }
        }
    }
}