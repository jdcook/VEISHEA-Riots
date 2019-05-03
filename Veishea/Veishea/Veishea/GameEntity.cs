using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace Veishea
{
    public enum FactionType
    {
        Players,
        Enemies,
        Neutral,
    }
    public enum EntityType
    {
        None,
        Player,
        Mob,
        Misc,
    }
    public class GameEntity
    {
        public string Name { get; private set; }
        public bool Dead { get; set; }

        private Dictionary<Type, Component> components = new Dictionary<Type, Component>();
        private Dictionary<Type, Object> sharedData = new Dictionary<Type, Object>();

        public GameEntity(string name)
        {
            this.Name = name;
            Dead = false;
        }

        public void AddSharedData(Type t, Object o)
        {
            sharedData.Add(t, o);
        }

        public Object GetSharedData(Type t)
        {
            Object retData = null;
            sharedData.TryGetValue(t, out retData);
            return retData;
        }

        public void AddComponent(Type t, Component o)
        {
            components.Add(t, o);
        }

        public void KillEntity()
        {
            foreach (KeyValuePair<Type, Component> pair in components)
            {
                pair.Value.KillComponent();
            }

            Dead = true;
        }

        public void RemoveComponent(Type t)
        {
            if (components.ContainsKey(t))
            {
                components[t].KillComponent();
                components.Remove(t);
            }
        }

        public Component GetComponent(Type t)
        {
            Component retComponent = null;
            components.TryGetValue(t, out retComponent);
            //will be null if key is not found
            return retComponent;
        }
    }
}
