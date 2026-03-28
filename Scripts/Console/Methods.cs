using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Assets.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CallableAttribute : Attribute
    {
        public string Name { get; set; }
        public CallableAttribute(string name = null) => Name = name;
    }

    public class Method_Info
    {
        public object Instance { get; set; }
        public string Name { get; set; }
        public MethodBase Method { get; set; }
        public Type DeclaringType { get; set; }
        public bool IsStatic { get; set; }
    }

    public class MethodFinder
    {
        private Dictionary<string, List<Method_Info>> _methods = new Dictionary<string, List<Method_Info>>();

        public MethodFinder()
        {
            ScanAllAssemblies();
        }

        private void ScanAllAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("System") ||
                    assembly.FullName.StartsWith("Microsoft") ||
                    assembly.FullName.StartsWith("netstandard"))
                    continue;

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsClass || type.IsAbstract) continue;

                        foreach (var method in type.GetMethods(BindingFlags.Public |
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.Instance |
                                                                BindingFlags.Static))
                        {
                            var attr = method.GetCustomAttribute<CallableAttribute>();
                            if (attr != null)
                            {
                                object instance = null;
                                bool isStatic = method.IsStatic;

                                if (!isStatic)
                                {
                                    try
                                    {
                                        instance = Activator.CreateInstance(type);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"Не удалось создать экземпляр {type.Name}: {ex.Message}");
                                        continue;
                                    }
                                }

                                var callName = attr.Name ?? method.Name;

                                var info = new Method_Info
                                {
                                    Instance = instance,
                                    Name = method.Name,
                                    Method = method,
                                    DeclaringType = type,
                                    IsStatic = isStatic
                                };

                                if (!_methods.ContainsKey(callName))
                                    _methods[callName] = new List<Method_Info>();

                                _methods[callName].Add(info);

                                Debug.Log($"Зарегистрирован метод: {callName} в {type.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Ошибка при сканировании сборки {assembly.FullName}: {ex.Message}");
                }
            }
        }

        public List<Method_Info> Find(string name)
        {
            return _methods.ContainsKey(name) ? _methods[name] : new List<Method_Info>();
        }

        public object Call(string name, params object[] args)
        {
            var methods = Find(name);
            if (methods.Count == 0)
            {
                Debug.LogError($"Метод '{name}' не найден!");
                return null;
            }

            Method_Info selectedMethod = null;

            if (methods.Count > 1)
            {
                foreach (var methodInfo in methods)
                {
                    var parameters = methodInfo.Method.GetParameters();
                    if (parameters.Length == args.Length)
                    {
                        bool match = true;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (args[i] != null && !parameters[i].ParameterType.IsAssignableFrom(args[i].GetType()))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            selectedMethod = methodInfo;
                            break;
                        }
                    }
                }

                if (selectedMethod == null)
                    selectedMethod = methods[0];
            }
            else
            {
                selectedMethod = methods[0];
            }

            try
            {
                return selectedMethod.Method.Invoke(selectedMethod.Instance, args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка при вызове метода '{name}': {ex.InnerException?.Message ?? ex.Message}");
                return null;
            }
        }

        public object[] ParseTypedValues(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new object[0];

            if (!input.Contains('(') || !input.Contains(')'))
            {
                Debug.LogError("Некорректный формат ввода. Ожидается: MethodName(arg1, arg2, ...)");
                return new object[0];
            }

            try
            {
                string methodName = input.Split('(')[0];

                string bracketContent = input.Split('(', ')')[1];

                if (string.IsNullOrWhiteSpace(bracketContent))
                {
                    Call(methodName);
                    return new object[0];
                }

                string[] parts = bracketContent.Split(',');
                List<object> result = new List<object>();

                foreach (string part in parts)
                {
                    string trimmed = part.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    string[] typeAndValue = trimmed.Split(' ', 2);

                    if (typeAndValue.Length < 2)
                    {
                        Debug.LogWarning($"Некорректный аргумент: {trimmed}");
                        continue;
                    }

                    string type = typeAndValue[0].ToLower();
                    string value = typeAndValue[1];

                    try
                    {
                        switch (type)
                        {
                            case "string":
                                result.Add(value);
                                break;
                            case "int":
                                result.Add(int.Parse(value));
                                break;
                            case "float":
                                result.Add(float.Parse(value));
                                break;
                            case "double":
                                result.Add(double.Parse(value));
                                break;
                            case "bool":
                                result.Add(bool.Parse(value));
                                break;
                            case "object":
                                GameObject obj = GameObject.Find(value);
                                if (obj == null)
                                    Debug.LogWarning($"Объект '{value}' не найден в сцене");
                                result.Add(obj);
                                break;
                            default:
                                Debug.LogWarning($"Неизвестный тип: {type}, обрабатывается как строка");
                                result.Add(value);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Ошибка парсинга аргумента '{trimmed}': {ex.Message}");
                        result.Add(value);
                    }
                }

                Call(methodName, result.ToArray());
                return result.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка при парсинге строки '{input}': {ex.Message}");
                return new object[0];
            }
        }

        public T Call<T>(string name, params object[] args)
        {
            var result = Call(name, args);
            if (result is T typedResult)
                return typedResult;

            Debug.LogWarning($"Не удалось привести результат к типу {typeof(T)}");
            return default(T);
        }

        public Dictionary<string, List<Method_Info>> GetAllMethods()
        {
            return _methods;
        }
    }
}