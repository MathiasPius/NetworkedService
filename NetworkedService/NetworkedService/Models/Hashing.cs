using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkedService.Models
{
    public class BasicHash : IEquatable<BasicHash>
    {
        public BasicHash() { Hash = 0; }
        public BasicHash(IEnumerable<byte> bytes) { Hash = BitConverter.ToInt32(bytes.ToArray(), 0); }

        public Int32 Hash { get; set; }

        public IEnumerable<byte> GetBytes() { return BitConverter.GetBytes(Hash); }

        public bool Equals(BasicHash other) { return Hash == other.Hash; }
        public override string ToString() { return Convert.ToBase64String(BitConverter.GetBytes(Hash)).TrimEnd('='); }
        public override Int32 GetHashCode() { return Hash; }
    }

    public class MethodNameHash : BasicHash, IEquatable<MethodNameHash>
    {
        public MethodNameHash() { }
        public MethodNameHash(IEnumerable<byte> b) : base(b) { }
        public bool Equals(MethodNameHash other) { return base.Equals(other); }
    }

    public class MethodSignatureHash : BasicHash, IEquatable<MethodSignatureHash>
    {
        public MethodSignatureHash() { }
        public MethodSignatureHash(IEnumerable<byte> b) : base(b) { }
        public bool Equals(MethodSignatureHash other) { return base.Equals(other); }
    }

    public class InterfaceHash : BasicHash, IEquatable<InterfaceHash>
    {
        public InterfaceHash() { }
        public InterfaceHash(IEnumerable<byte> b) : base(b) { }
        public bool Equals(InterfaceHash other) { return base.Equals(other); }
    }

    public class ServiceHash : InterfaceHash, IEquatable<ServiceHash>
    {
        public ServiceHash() { }
        public ServiceHash(IEnumerable<byte> bytes) : base(bytes) { }
        public ServiceHash(InterfaceHash interfaceHash) : base(interfaceHash.GetBytes()) { }
        public bool Equals(ServiceHash other) { return base.Equals(other); }
    }

    public class MethodHash : IEquatable<MethodHash>
    {
        public MethodNameHash Name { get; set; }
        public MethodSignatureHash Signature { get; set; }

        public IEnumerable<byte> GetBytes() { return Name.GetBytes().Concat(Signature.GetBytes()); }
        public bool Equals(MethodHash other) { return Name.Equals(other.Name) && Signature.Equals(other.Signature); }
    }

    public class InterfaceMethodHash : IEquatable<InterfaceMethodHash>
    {
        public InterfaceHash Interface { get; set; }
        public MethodHash Method { get; set; }

        public IEnumerable<byte> GetBytes() { return Interface.GetBytes().Concat(Method.GetBytes()); }
        public bool Equals(InterfaceMethodHash other) { return Interface.Equals(other.Interface) && Method.Equals(other.Method); }
    }

    public class ServiceInterfaceHash : IEquatable<ServiceInterfaceHash>
    {
        public ServiceHash Service { get; set; }
        public InterfaceHash Interface { get; set; }

        public IEnumerable<byte> GetBytes() { return Service.GetBytes().Concat(Interface.GetBytes()); }
        public bool Equals(ServiceInterfaceHash other) { return Service.Equals(other.Service) && Interface.Equals(other.Interface); }
    }

    public class InterfaceDeclaration
    {
        public InterfaceHash InterfaceHash { get; set; }
        public List<MethodHash> Methods { get; set; }
    }

    public class ServiceDeclaration
    {
        public ServiceHash ServiceHash { get; set; }
        public List<InterfaceDeclaration> Interfaces { get; set; }
    }

    public class RemoteProcedureDescriptor : IEquatable<RemoteProcedureDescriptor>
    {
        public ServiceHash ServiceHash { get; set; }
        public InterfaceHash InterfaceHash { get; set; }
        public MethodNameHash MethodName { get; set; }
        public MethodSignatureHash MethodSignature { get; set; }

        public RemoteProcedureDescriptor() { }

        public RemoteProcedureDescriptor(Guid guid)
        {
            var bytes = guid.ToByteArray();
            ServiceHash = new ServiceHash(bytes);
            InterfaceHash = new InterfaceHash(bytes.Skip(4));
            MethodName = new MethodNameHash(bytes.Skip(8));
            MethodSignature = new MethodSignatureHash(bytes.Skip(12));
        }

        public Guid ToGuid()
        {
            return new Guid(ServiceHash.GetBytes()
                .Concat(InterfaceHash.GetBytes())
                .Concat(MethodName.GetBytes())
                .Concat(MethodSignature.GetBytes())
                .ToArray());
        }

        public bool Equals(RemoteProcedureDescriptor other)
        {
            return ServiceHash.Equals(other.ServiceHash)
                && InterfaceHash.Equals(other.InterfaceHash)
                && MethodName.Equals(other.MethodName)
                && MethodSignature.Equals(other.MethodSignature);
        }
    }

    public class RemoteProcedureMethodComparer : IEqualityComparer<RemoteProcedureDescriptor>
    {
        public bool Equals(RemoteProcedureDescriptor x, RemoteProcedureDescriptor y)
        {
            return x.ServiceHash.Equals(y.ServiceHash)
                && x.MethodName.Equals(y.MethodName)
                && x.MethodSignature.Equals(y.MethodSignature);
        }

        public int GetHashCode(RemoteProcedureDescriptor obj)
        {
            return obj.ServiceHash.GetHashCode() 
                 + obj.MethodName.GetHashCode() 
                 + obj.MethodSignature.GetHashCode();
        }
    }
}
