﻿using System;
using Mono.Cecil;

namespace MdDoc.Model
{
    public class EventDocumentation : MemberDocumentation
    {
        public string Name => Definition.Name;

        public MemberId MemberId { get; }

        internal EventDefinition Definition { get; }


        public EventDocumentation(TypeDocumentation typeDocumentation, EventDefinition definition) : base(typeDocumentation)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            MemberId = definition.ToMemberId();
        }

        public override IDocumentation TryGetDocumentation(MemberId id) => 
            MemberId.Equals(id) ? this : TypeDocumentation.TryGetDocumentation(id);
    }
}
