using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public enum Event
    {
        MoveDirectionChanged,
        PaddleCollision
    }
    
    public static class EventEmitter
    {
        private static Dictionary<Event, List<UnityEvent<Vector2>>> events = 
            new Dictionary<Event, List<UnityEvent<Vector2>>>();

        private static Dictionary<Event, List<UnityAction<Vector2>>> listeners = 
            new Dictionary<Event, List<UnityAction<Vector2>>>();

        public static readonly UnityEvent<Vector2, Vector2, GameObject> LineCollision =
            new UnityEvent<Vector2, Vector2, GameObject>();

        public static void SubscribeOnEvent(Event eventName, UnityAction<Vector2> listener)
        {
            if (!listeners.ContainsKey(eventName)) InitializeEvent(eventName);
            listeners[eventName].Add(listener);
            foreach (var unityEvent in events[eventName])
            {
                unityEvent.AddListener(listener);
            }
        }
        
        public static void AddEvent(Event eventName, UnityEvent<Vector2> unityEvent)
        {
            if (!events.ContainsKey(eventName)) InitializeEvent(eventName);
            events[eventName].Add(unityEvent);
            foreach (var listener in listeners[eventName])
            {
                unityEvent.AddListener(listener);
            }
        }

        private static void InitializeEvent(Event eventName)
        {
            listeners[eventName] = new List<UnityAction<Vector2>>();
            events[eventName] = new List<UnityEvent<Vector2>>();
        }
    }
}