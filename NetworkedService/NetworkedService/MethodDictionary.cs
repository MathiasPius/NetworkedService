using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace NetworkedService
{
    public class MethodDictionary
    {
        private Dictionary<Guid, Tuple<Type, MethodInfo>> _methods;
        public List<Tuple<InterfaceHash, Type>> Interfaces { get; private set; }

        public MethodDictionary()
        {
            _methods = new Dictionary<Guid, Tuple<Type, MethodInfo>>();
            Interfaces = new List<Tuple<InterfaceHash, Type>>();
        }

        public Tuple<Type, MethodInfo> FindMethod(RemoteProcedureDescriptor descriptor)
        {
            return _methods[descriptor.ToGuid()];
        }

        public RemoteProcedureDescriptor FindDescriptor(MethodInfo methodInfo)
        {
            var descriptor = new RemoteProcedureDescriptor(
                _methods.FirstOrDefault(o => o.Value.Item2 == methodInfo).Key
            );

            return descriptor;
        }

        public Type GetPrimaryInterface()
        {
            return Interfaces.Last().Item2;
        }

        public InterfaceHash GetPrimaryInterfaceHash()
        {
            return Interfaces.Last().Item1;
        }

        public IEnumerable<InterfaceHash> GetSecondaryInterfaceHashes()
        {
            return Interfaces.Take(Interfaces.Count() - 1).Select(i => i.Item1);
        }

        public InterfaceHash AddInterface(ServiceHash serviceHash, Type iface)
        {
            Console.WriteLine("Hashing Interface: " + iface.FullName);
            // Hash all methods
            var methodHashes = iface.GetMethods()
                .Select(m => new { Method = m, Hash = HashMethod(m) })
                .ToList();

            // Hash all inherited interfaces
            var interfaceHashes = iface.GetInterfaces()
                .Select(i => new { Interface = i, Hash = AddInterface(serviceHash, i) });

            // Hash this interface, based on our methods and interfaces
            var interfaceHash = HashInterface(iface,
                methodHashes.Select(m => m.Hash),
                interfaceHashes.Select(i => i.Hash)
            );

            var descriptor = new RemoteProcedureDescriptor
            {
                ServiceHash = serviceHash,
                InterfaceHash = interfaceHash
            };

            foreach (var method in methodHashes)
            {
                descriptor.MethodName = method.Hash.Name;
                descriptor.MethodSignature = method.Hash.Signature;
                
                if(_methods.ContainsKey(descriptor.ToGuid()))
                {
                    Console.WriteLine(iface.Name + ":" + method.Method.Name + " already exists");
                }
                else
                {
                    _methods.Add(descriptor.ToGuid(), Tuple.Create(iface, method.Method));
                }
            }

            Interfaces.Add(Tuple.Create(interfaceHash, iface));
            return interfaceHash;
        }

        public InterfaceHash AddInterface<TInterface>(ServiceHash serviceHash)
            => AddInterface(serviceHash, typeof(TInterface));

        private static MD5 _hasher = MD5.Create();

        internal static IEnumerable<byte> Hash(string value)
        {
            // Hash the value and return the first 4 bytes
            return _hasher.ComputeHash(Encoding.UTF8.GetBytes(value))
                .Take(4);
        }

        internal static MethodHash HashMethod(MethodInfo method)
        {
            var returnType = method.ReturnType.FullName;

            var parameters = string.Join("+", method.GetParameters()
                .Select(p => p.Position.ToString() + p.ParameterType.FullName));

            var name = Hash(method.Name);
            var signature = Hash(returnType + parameters);

            return new MethodHash
            {
                Name = new MethodNameHash(name),
                Signature = new MethodSignatureHash(signature)
            };
        }

        internal static InterfaceHash HashInterface(Type type, IEnumerable<MethodHash> methods, IEnumerable<InterfaceHash> interfaces)
        {
            if (!type.IsInterface)
            {
                throw new InvalidOperationException("Cannot compute hash for non-interface type");
            }

            return new InterfaceHash(
                Hash(type.FullName)
                .Concat(interfaces.OrderBy(i => i.GetBytes().First()).SelectMany(i => i.GetBytes()))
                .Concat(methods.OrderBy(m => m.GetBytes().First()).SelectMany(m => m.GetBytes()))
            );
        }
    }
}
