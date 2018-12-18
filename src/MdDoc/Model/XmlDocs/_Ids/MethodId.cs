﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MdDoc.Model.XmlDocs
{
    public sealed class MethodId : MemberId, IEquatable<MethodId>
    {
        public TypeId DefiningType { get; }

        public string Name { get; }

        public int Arity { get; }

        public IReadOnlyList<TypeId> Parameters { get; }

        public TypeId ReturnType { get; }


        public MethodId(TypeId definingType, string name) : this(definingType, name, 0, Array.Empty<TypeId>(), null)
        { }

        public MethodId(TypeId definingType, string name, IReadOnlyList<TypeId> parameters) : this(definingType, name, 0, parameters, null)
        { }

        public MethodId(TypeId definingType, string name, int arity, IReadOnlyList<TypeId> parameters) : this(definingType, name, arity, parameters, null)
        { }

        public MethodId(TypeId definingType, string name, int arity, IReadOnlyList<TypeId> parameters, TypeId returnType)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Value must not be null or empty", nameof(name));

            DefiningType = definingType ?? throw new ArgumentNullException(nameof(definingType));
            Name = name;
            Arity = arity;
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ReturnType = returnType;
        }


        public override bool Equals(object obj) => Equals(obj as MethodId);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = DefiningType.GetHashCode() * 397;
                hash ^= StringComparer.Ordinal.GetHashCode(Name);
                hash ^= Arity.GetHashCode();

                foreach(var parameter in Parameters)
                {
                    hash ^= parameter.GetHashCode();
                }

                if(ReturnType != null)
                    hash ^= ReturnType.GetHashCode();

                return hash;
            }
        }

        public bool Equals(MethodId other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other == null)
                return false;

            return DefiningType.Equals(other.DefiningType) &&
                StringComparer.Ordinal.Equals(Name, other.Name) &&
                Arity == other.Arity &&
                Parameters.SequenceEqual(other.Parameters) &&
                (
                    (ReturnType == null && other.ReturnType == null) || 
                    (ReturnType != null && ReturnType.Equals(other.ReturnType))
                );
        }
    }
}
