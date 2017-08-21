using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Mvc.Server.Options
{
    /// <summary>
    /// Static helper class that allows binding strongly typed objects to configuration values.
    /// </summary>
    public static class ConfigurationBinder
    {
        /// <summary>
        /// Attempts to bind the configuration instance to a new instance of type T.
        /// If this configuration section has a value, that will be used.
        /// Otherwise binding by matching property names against configuration keys recursively.
        /// </summary>
        /// <typeparam name="T">The type of the new instance to bind.</typeparam>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <returns>The new instance of T if successful, default(T) otherwise.</returns>
        public static T Get<T>(this IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            var obj = configuration.Get(typeof(T));
            if (obj == null)
                return default(T);
            return (T)obj;
        }

        /// <summary>
        /// Attempts to bind the configuration instance to a new instance of type T.
        /// If this configuration section has a value, that will be used.
        /// Otherwise binding by matching property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="type">The type of the new instance to bind.</param>
        /// <returns>The new instance if successful, null otherwise.</returns>
        public static object Get(this IConfiguration configuration, Type type)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            return BindInstance(type, null, configuration);
        }

        /// <summary>
        /// Attempts to bind the given object instance to configuration values by matching property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="instance">The object to bind.</param>
        public static void Bind(this IConfiguration configuration, object instance)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (instance == null)
                return;
            BindInstance(instance.GetType(), instance, configuration);
        }

        /// <summary>
        /// Extracts the value with the specified key and converts it to type T.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <param name="key">The configuration key for the value to convert.</param>
        /// <returns>The converted value.</returns>
        public static T GetValue<T>(this IConfiguration configuration, string key)
        {
            return configuration.GetValue(key, default(T));
        }

        /// <summary>
        /// Extracts the value with the specified key and converts it to type T.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <param name="key">The configuration key for the value to convert.</param>
        /// <param name="defaultValue">The default value to use if no value is found.</param>
        /// <returns>The converted value.</returns>
        public static T GetValue<T>(this IConfiguration configuration, string key, T defaultValue)
        {
            return (T)configuration.GetValue(typeof(T), key, defaultValue);
        }

        /// <summary>
        /// Extracts the value with the specified key and converts it to the specified type.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="type">The type to convert the value to.</param>
        /// <param name="key">The configuration key for the value to convert.</param>
        /// <returns>The converted value.</returns>
        public static object GetValue(this IConfiguration configuration, Type type, string key)
        {
            return configuration.GetValue(type, key, null);
        }

        /// <summary>
        /// Extracts the value with the specified key and converts it to the specified type.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="type">The type to convert the value to.</param>
        /// <param name="key">The configuration key for the value to convert.</param>
        /// <param name="defaultValue">The default value to use if no value is found.</param>
        /// <returns>The converted value.</returns>
        public static object GetValue(this IConfiguration configuration, Type type, string key, object defaultValue)
        {
            var str = configuration.GetSection(key).Value;
            if (str != null)
                return ConvertValue(type, str);
            return defaultValue;
        }

        private static void BindNonScalar(this IConfiguration configuration, object instance)
        {
            if (instance == null)
                return;
            foreach (var allProperty in GetAllProperties(instance.GetType().GetTypeInfo()))
                BindProperty(allProperty, instance, configuration);
        }

        private static void BindProperty(PropertyInfo property, object instance, IConfiguration config)
        {
            if ((object)property.GetMethod == null || !property.GetMethod.IsPublic || property.GetMethod.GetParameters().Length != 0)
                return;
            var instance1 = property.GetValue(instance);
            var flag = (object)property.SetMethod != null && property.SetMethod.IsPublic;
            if (instance1 == null && !flag)
                return;
            var obj = BindInstance(property.PropertyType, instance1, config.GetSection(property.Name));
            if (!(obj != null & flag))
                return;
            property.SetValue(instance, obj);
        }

        private static object BindToCollection(TypeInfo typeInfo, IConfiguration config)
        {
            var type = typeof(List<>).MakeGenericType(typeInfo.GenericTypeArguments[0]);
            var instance = Activator.CreateInstance(type);
            var collectionType = type;
            var config1 = config;
            BindCollection(instance, collectionType, config1);
            return instance;
        }

        private static object AttemptBindToCollectionInterfaces(Type type, IConfiguration config)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsInterface)
                return null;
            if ((object)FindOpenGenericInterface(typeof(IReadOnlyList<>), type) != null)
                return BindToCollection(typeInfo, config);
            if ((object)FindOpenGenericInterface(typeof(IReadOnlyDictionary<,>), type) != null)
            {
                var type1 = typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0], typeInfo.GenericTypeArguments[1]);
                var instance = Activator.CreateInstance(type1);
                var dictionaryType = type1;
                var config1 = config;
                BindDictionary(instance, dictionaryType, config1);
                return instance;
            }
            var genericInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
            if ((object)genericInterface != null)
            {
                var instance = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0], typeInfo.GenericTypeArguments[1]));
                var dictionaryType = genericInterface;
                var config1 = config;
                BindDictionary(instance, dictionaryType, config1);
                return instance;
            }
            if ((object)FindOpenGenericInterface(typeof(IReadOnlyCollection<>), type) != null || (object)FindOpenGenericInterface(typeof(ICollection<>), type) != null || (object)FindOpenGenericInterface(typeof(IEnumerable<>), type) != null)
                return BindToCollection(typeInfo, config);
            return null;
        }

        internal static object BindInstance(Type type, object instance, IConfiguration config)
        {
            if (ReferenceEquals(type, typeof(IConfigurationSection)))
                return config;
            var configurationSection = config as IConfigurationSection;
            var str = configurationSection?.Value;
            if (str != null)
                return ConvertValue(type, str);
            if (config != null && config.GetChildren().Any())
            {
                if (instance == null)
                {
                    instance = AttemptBindToCollectionInterfaces(type, config);
                    if (instance != null)
                        return instance;
                    instance = CreateInstance(type);
                }
                var genericInterface1 = FindOpenGenericInterface(typeof(IDictionary<,>), type);
                if ((object)genericInterface1 != null)
                    BindDictionary(instance, genericInterface1, config);
                else if (type.IsArray)
                {
                    instance = BindArray((Array)instance, config);
                }
                else
                {
                    var genericInterface2 = FindOpenGenericInterface(typeof(ICollection<>), type);
                    if ((object)genericInterface2 != null)
                        BindCollection(instance, genericInterface2, config);
                    else
                        config.BindNonScalar(instance);
                }
            }
            return instance;
        }

        private static object CreateInstance(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsInterface || typeInfo.IsAbstract)
                throw new InvalidOperationException("CannotActivateAbstractOrInterface");
            if (type.IsArray)
            {
                if (typeInfo.GetArrayRank() > 1)
                    throw new InvalidOperationException("UnsupportedMultidimensionalArray");
                return Array.CreateInstance(typeInfo.GetElementType(), new int[1]);
            }
            var declaredConstructors = typeInfo.DeclaredConstructors;

            bool Func(ConstructorInfo ctor)
            {
                if (ctor.IsPublic)
                    return ctor.GetParameters().Length == 0;
                return false;
            }

            Func<ConstructorInfo, bool> predicate = Func;
            if (!declaredConstructors.Any(predicate))
                throw new InvalidOperationException("MissingParameterlessConstructor");
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to activate", ex);
            }
        }

        private static void BindDictionary(object dictionary, Type dictionaryType, IConfiguration config)
        {
            var typeInfo = dictionaryType.GetTypeInfo();
            var genericTypeArgument1 = typeInfo.GenericTypeArguments[0];
            var genericTypeArgument2 = typeInfo.GenericTypeArguments[1];
            if (!ReferenceEquals(genericTypeArgument1, typeof(string)))
                return;
            var declaredMethod = typeInfo.GetDeclaredMethod("Add");
            foreach (var child in config.GetChildren())
            {
                var obj = BindInstance(genericTypeArgument2, null, child);
                if (obj != null)
                {
                    var key = child.Key;
                    declaredMethod.Invoke(dictionary, new[]
                    {
                        key,
                        obj
                    });
                }
            }
        }

        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
        private static void BindCollection(object collection, Type collectionType, IConfiguration config)
        {
            var typeInfo = collectionType.GetTypeInfo();
            var genericTypeArgument = typeInfo.GenericTypeArguments[0];
            var name = "Add";
            var declaredMethod = typeInfo.GetDeclaredMethod(name);
            foreach (var child in config.GetChildren())
            {
                try
                {
                    var obj = BindInstance(genericTypeArgument, null, child);
                    if (obj != null)
                        declaredMethod.Invoke(collection, new[]
                        {
                            obj
                        });
                }
                catch
                {
                }
            }
        }

        [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
        private static Array BindArray(Array source, IConfiguration config)
        {
            var array = config.GetChildren().ToArray();
            var length = source.Length;
            var elementType = source.GetType().GetElementType();
            var instance = Array.CreateInstance(elementType, new[]
            {
                length + array.Length
            });
            if (length > 0)
                Array.Copy(source, instance, length);
            for (var index = 0; index < array.Length; ++index)
            {
                try
                {
                    var obj = BindInstance(elementType, null, array[index]);
                    if (obj != null)
                        instance.SetValue(obj, new[]
                        {
                            length + index
                        });
                }
                catch
                {
                }
            }
            return instance;
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private static object ConvertValue(Type type, string value)
        {
            while (true)
            {
                if (ReferenceEquals(type, typeof(object)))
                    return value;
                if (type.GetTypeInfo().IsGenericType && ReferenceEquals(type?.GetGenericTypeDefinition(), typeof(Nullable<>)))
                {
                    if (string.IsNullOrEmpty(value))
                        return null;
                    type = Nullable.GetUnderlyingType(type);
                    continue;
                }
                try
                {
                    return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Configuration binding failed", ex);
                }
            }
        }

        private static Type FindOpenGenericInterface(Type expected, Type actual)
        {
            var typeInfo = actual.GetTypeInfo();
            if (typeInfo.IsGenericType && ReferenceEquals(actual.GetGenericTypeDefinition(), expected))
                return actual;
            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                if (implementedInterface.GetTypeInfo().IsGenericType && ReferenceEquals(implementedInterface.GetGenericTypeDefinition(), expected))
                    return implementedInterface;
            }
            return null;
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(TypeInfo type)
        {
            var propertyInfoList = new List<PropertyInfo>();
            do
            {
                propertyInfoList.AddRange(type.DeclaredProperties);
                type = type.BaseType.GetTypeInfo();
            }
            while (type != typeof(object).GetTypeInfo());
            return propertyInfoList;
        }
    }
}