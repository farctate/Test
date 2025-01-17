﻿using System;
using UnityEngine;

namespace UnityDI.Extensions
{
    public static class ExtensionsEngine
    {
        public static bool TrySearchComponent<T>(this Transform root, out T value)
            where T : Component
        {
            value = SearchComponentInternal<T>(root);
            return value == null;
        }

        public static bool TrySearchComponent<T>(this Transform root, Func<GameObject, bool> predicate, out T value)
            where T : Component
        {
            value = SearchComponentInternal<T>(root, predicate);
            return value == null;
        }

        public static T SearchComponent<T>(this Transform root)
            where T : Component
        {
            return SearchComponentInternal<T>(root) ?? throw new Exception($"Can't find component of type {typeof(T)}");
        }

        public static T SearchComponent<T>(this Transform root, Func<GameObject, bool> predicate)
            where T : Component
        {
            return SearchComponentInternal<T>(root, predicate) ?? throw new Exception($"Can't find component of type {typeof(T)}");
        }

        private static T SearchComponentInternal<T>(Transform current)
            where T : Component
        {
            var type = typeof(T);
            if (current.TryGetComponent(type, out var comp))
            {
                return comp as T;
            }

            foreach (Transform child in current)
            {
                comp = SearchComponentInternal<T>(child);
                if (!ReferenceEquals(null, comp))
                {
                    return comp as T;
                }
            }

            return null;
        }

        private static T SearchComponentInternal<T>(Transform current, Func<GameObject, bool> predicate)
            where T : Component
        {
            if(ReferenceEquals(null, predicate))
            {
                return SearchComponentInternal<T>(current);
            }

            var type = typeof(T);
            if (predicate.Invoke(current.gameObject) && current.TryGetComponent(type, out var comp))
            {
                return comp as T;
            }

            foreach (Transform child in current)
            {
                comp = SearchComponentInternal<T>(child, predicate);
                if (!ReferenceEquals(null, comp))
                {
                    return comp as T;
                }
            }

            return null;
        }
    }
}
