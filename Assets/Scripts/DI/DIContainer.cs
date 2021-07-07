using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityDI
{
    /// <summary>
    /// При создании любого класса DI автоматически разрешает его зависимости в момент его
    /// создания. Для .Net классов это момент создания через new(), для MonoBehaviour -- момент вызова OnEnable()
    /// Constructor DI использовать не получится, т.к. MonoBehaviour нельзя создавать через new(), поэтому 
    /// все внедряемые поля будут помечаться с помощью [Injected]. Внедрение зависимости следовательно делается
    /// через рефлексию. 
    /// 
    /// Dependency types:
    /// 0) Singleton. Доступны c момента инициализации приложения и до окончания его работы. Должны быть инициализированы в начале
    /// 1) Transient. Каждому запрашивающему классу предоставляется Instance через Provider, причем этот Instance может всегда браться из одного месте в сцене,
    /// а может каждый раз создаваться новый(в зависимости от предоставленного провайдера)
    /// 2) Shared. Существуют от момента появляения хотя бы одной ссылки на них до момента когда ссылок нет, при 0 кол-ве ссылок 
    /// удаляются для MonoBehaviour через Destroy(), для .Net классов - value помечается как null и затем очищается GC автоматически
    /// 3) Filter: содержит ссылки на все созданные и активные объекты указанного типа
    /// </summary>
    public class DIContainer : DIClass
    {
        private enum DependencyType
        {
            Singleton,
            Transient,
            Shared,
        }

        public delegate object ProviderDelegade(object target);

        private class DescDependency
        {
            public DependencyType DependencyType;
            public int Count;
            public object Value;
            public ProviderDelegade Provider;
        }

        private Dictionary<Type, DescDependency> _registeredDependencies;
        private Dictionary<Type, IFilter> _registeredFilters;

        protected DIContainer parent = null;

        protected DIContainer(DIContainer parent = null)
            : base()
        {
            _registeredDependencies = new Dictionary<Type, DescDependency>();
            _registeredFilters = new Dictionary<Type, IFilter>();

            if (!ReferenceEquals(null, parent))
            {
                this.parent = parent;
                DIEventDispatcher.OnInit -= parent.Inject;
            }

            DIEventDispatcher.OnInit += Inject;
            DIEventDispatcher.OnInit += AddToFilter;

            DIEventDispatcher.OnDispose += DisposeSharedCallback;
            DIEventDispatcher.OnDispose += RemoveFromFilter;
        }

        protected T RegisterSingleton<T>(ProviderDelegade provider)
            where T : class
        {
            var type = typeof(T);
            if (_registeredDependencies.ContainsKey(type))
            {
                throw new Exception($"{type.Name} is already registered");
            }
            var value = provider?.Invoke(null);
            _registeredDependencies.Add(type, new DescDependency
            {
                DependencyType = DependencyType.Singleton,
                Value = value,
            });

            return value as T;
        }

        protected void RegisterTransient<T>(ProviderDelegade provider)
        {
            var type = typeof(T);
            if (_registeredDependencies.ContainsKey(type))
            {
                throw new Exception($"{type.Name} is already registered");
            }

            _registeredDependencies.Add(type, new DescDependency
            {
                DependencyType = DependencyType.Transient,
                Provider = provider
            });
        }

        protected void RegisterShared<T>(ProviderDelegade provider)
        {
            var type = typeof(T);
            if (_registeredDependencies.ContainsKey(type))
            {
                throw new Exception($"{type.Name} is already registered");
            }

            _registeredDependencies.Add(type, new DescDependency
            {
                DependencyType = DependencyType.Shared,
                Count = 0,
                Value = null,
                Provider = provider,
            });
        }

        protected void RegisterFilter<T>()
            where T : class
        {
            _registeredFilters.Add(typeof(T), new Filter<T>());
        }

        private void AddToFilter(object target, Type type)
        {
            if(_registeredFilters.ContainsKey(type))
            {
                _registeredFilters[type].Add(target);
            }
        }

        private void RemoveFromFilter(object target, Type type)
        {
            if(_registeredFilters.ContainsKey(type))
            {
                _registeredFilters[type].Remove(target);
            }
        }

        private void Inject(object target, Type type)
        {
            var fields = type.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
                ).Where(_ => _.IsDefined(typeof(InjectedAttribute), false));
            foreach (var field in fields)
            {
                if(typeof(IFilter).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(target);
                    if(ReferenceEquals(null, value))
                    {
                        var genericArg = field.FieldType.GetGenericArguments().FirstOrDefault();
                        if(_registeredFilters.ContainsKey(genericArg))
                        {
                            field.SetValue(target, _registeredFilters[genericArg]);
                        }
                    }

                    continue;
                }

                var current = this;
                object dependencyValue = null;
                while(current != null && !current.ResolveInternal(target, field.FieldType, out dependencyValue))
                {
                    current = current.parent;
                }
                if(ReferenceEquals(null, dependencyValue))
                {
                    Debug.LogWarning($"Injecting to {target} error. " +
                        $"Dependency ${field.FieldType} is not registered in {this}.");
                    continue;
                }
                if(!ReferenceEquals(null, target))
                {
                    field.SetValue(target, dependencyValue);
                }
            }
        }

        private bool ResolveInternal(object target, Type dependencyType, out object value)
        {
            if(!_registeredDependencies.ContainsKey(dependencyType))
            {
                value = null;
                return false;
            }

            var desc = _registeredDependencies[dependencyType];
            switch(desc.DependencyType)
            {
                case DependencyType.Singleton:
                    {
                        value = desc.Value;
                    }
                    break;
                case DependencyType.Transient:
                    {
                        value = desc.Provider?.Invoke(target);
                    }
                    break;
                case DependencyType.Shared:
                    {
                        if(desc.Count == 0)
                        {
                            value = desc.Provider?.Invoke(target);
                            desc.Value = value;
                        }
                        else
                        {
                            value = desc.Value;
                        }
                        _registeredDependencies[dependencyType].Count++;
                    }
                    break;
                default:
                    throw new Exception($"Unknown LifetimeType: {desc.DependencyType}");
            }

            return !ReferenceEquals(null, value);
        }

        private void DisposeSharedCallback(object sender, Type type)
        {
            var fields = type.GetFields().Where(_ => _.IsDefined(typeof(InjectedAttribute), false));
            var sharedDependencyFields = fields.Where(_ =>
            _registeredDependencies.ContainsKey(_.FieldType) &&
            _registeredDependencies[_.FieldType].DependencyType == DependencyType.Shared).ToList();
            for(var i = 0; i < sharedDependencyFields.Count; ++i)
            {
                var field = sharedDependencyFields[i];
                var desc = _registeredDependencies[field.FieldType];
                desc.Count--;
                if(desc.Count == 0 && desc.Value != null)
                {
                    if(desc.Value is DIMonoBehaviour cast)
                    {
                        UnityEngine.Object.Destroy(cast);
                        if (cast.GetComponents<Component>() == null || cast.GetComponents<Component>().Length == 2)
                        {
                            UnityEngine.Object.Destroy(cast.gameObject);
                        }
                        desc.Value = null;
                    }
                    else
                    {
                        desc.Value = null;
                    }
                }
            }
        }

        public void DestroyContainer()
        {
            _registeredDependencies = null;
            _registeredFilters = null;
            DIEventDispatcher.OnInit -= Inject;
            DIEventDispatcher.OnInit -= AddToFilter;

            DIEventDispatcher.OnDispose -= DisposeSharedCallback;
            DIEventDispatcher.OnDispose -= RemoveFromFilter;

            if(!ReferenceEquals(null, parent))
            {
                DIEventDispatcher.OnInit += parent.Inject;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InjectedAttribute : Attribute { }
}
