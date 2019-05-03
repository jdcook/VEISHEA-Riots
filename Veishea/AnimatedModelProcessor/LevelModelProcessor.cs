using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SkinnedModelLib;

namespace AnimatedModelProcessor
{
    [ContentProcessor(DisplayName = "Level Model Processor")]
    public class LevelModelProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            //uncomment this to debug the content processor
            //System.Diagnostics.Debugger.Launch();

            LevelTagData tag = new LevelTagData();

            AddPointsTo(GetDataHolder(input, "light_poles"), tag.lightPoleLocations);
            AddPointsTo(GetDataHolder(input, "cars"), tag.cars);
            AddPointsTo(GetDataHolder(input, "mailbox1"), tag.mailbox1);
            AddPointsTo(GetDataHolder(input, "mailbox2"), tag.mailbox2);
            AddPointsTo(GetDataHolder(input, "beer"), tag.beer);

            ModelContent retModel = base.Process(input, context);
            retModel.Tag = tag;

            return retModel;
        }

        void AddPointsTo(NodeContent dataHolder, List<Vector3> list)
        {
            //iterate through and get positions
            if (dataHolder != null)
            {
                for (int i = dataHolder.Children.Count - 1; i >= 0; --i)
                {
                    MeshContent spherePoint = dataHolder.Children[i] as MeshContent;
                    if (spherePoint != null)
                    {
                        BoundingSphere sphere = BoundingSphere.CreateFromPoints(spherePoint.Positions);
                        list.Add(Vector3.Transform(sphere.Center, spherePoint.AbsoluteTransform));
                    }
                }
                dataHolder.Parent.Children.Remove(dataHolder);
            }
        }

        NodeContent GetDataHolder(NodeContent input, string name)
        {
            NodeContentCollection children = input.Children;
            for (int i = children.Count - 1; i >= 0; --i)
            {
                NodeContent n = children[i];

                if (n.Name.ToLower() == name.ToLower())
                {
                    return n;
                }
            }
            return null;
        }
    }
}
