﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grynwald.MdDocs.ApiReference.Loaders;
using Grynwald.MdDocs.ApiReference.Model;
using Xunit;

namespace Grynwald.MdDocs.ApiReference.Test.Model
{
    /// <summary>
    /// Tests for <see cref="_TypeDocumentation"/>
    /// </summary>
    public class _TypeDocumentationTest
    {
        public class Constructor
        {
            [Fact]
            public void Assembly_must_not_be_null()
            {
                // ARRANGE
                var typeId = new SimpleTypeId("Namespace1", "Class1");

                var builder = new ApiReferenceBuilder();
                var @namespace = builder.GetOrAddNamespace("Namespace1");

                // ACT 
                var ex = Record.Exception(() => new _TypeDocumentation(assembly: null!, @namespace: @namespace, typeId: typeId));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("assembly", argumentNullException.ParamName);
            }

            [Fact]
            public void Namespace_must_not_be_null()
            {
                // ARRANGE
                var typeId = new SimpleTypeId("Namespace1", "Class1");

                var assemblyBuilder = new ApiReferenceBuilder();
                var assembly = assemblyBuilder.AddAssembly("Assembly1", "1.0.0");

                // ACT 
                var ex = Record.Exception(() => new _TypeDocumentation(assembly: assembly, @namespace: null!, typeId: typeId));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("namespace", argumentNullException.ParamName);
            }

            [Fact]
            public void TypeId_must_not_be_null()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly1", "1.0.0");
                var @namespace = builder.GetOrAddNamespace("Namespace1");

                // ACT 
                var ex = Record.Exception(() => new _TypeDocumentation(assembly: assembly, @namespace: @namespace, typeId: null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("typeId", argumentNullException.ParamName);
            }

            [Fact]
            public void Namespace_of_type_id_has_to_match_namespace()
            {
                // ARRANGE
                var typeId = new SimpleTypeId("Namespace1", "Class1");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly1", "1.0.0");
                var @namespace = builder.GetOrAddNamespace("Namespace2");

                // ACT 
                var ex = Record.Exception(() => new _TypeDocumentation(assembly, @namespace, typeId));

                // ASSERT
                Assert.IsType<InconsistentModelException>(ex);
                Assert.Contains("Mismatch between namespace of type 'Namespace1.Class1' and id of parent namespace 'Namespace2'", ex.Message);
            }

            [Fact]
            public void DeclaringType_must_not_be_null_when_initializing_a_nested_type()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly1", "1.0.0");
                var typeId = new SimpleTypeId("Namespace1", "Class1");

                // ACT 
                var ex = Record.Exception(() => new _TypeDocumentation(assembly: assembly, declaringType: null!, typeId: typeId));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("declaringType", argumentNullException.ParamName);
            }

            [Fact]
            public void TypeId_must_have_a_declaring_type_when_initializing_a_nested_type()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly1", "1.0.0");
                var declaringType = builder.AddType("Assembly1", new SimpleTypeId("Namespace1", "Class1"));

                var typeId = new SimpleTypeId("Namespace1", "Class2");

                // ACT 
                var ex = Record.Exception(() => new _TypeDocumentation(assembly: assembly, declaringType: declaringType, typeId: typeId));

                // ASSERT
                Assert.IsType<InconsistentModelException>(ex);
                Assert.Contains("Cannot initialize nested type for type 'Namespace1.Class2' because it has no declaring type", ex.Message);
            }

            [Fact]
            public void DeclaringType_must_match_type_id()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly1", "1.0.0");
                var declaringType = builder.AddType("Assembly1", new SimpleTypeId("Namespace1", "Class1"));

                var typeId = new SimpleTypeId(new SimpleTypeId("Namespace1", "Class2"), "Class3");

                // ACT 
                var ex = Record.Exception(() => new _TypeDocumentation(assembly: assembly, declaringType: declaringType, typeId: typeId));

                // ASSERT
                Assert.IsType<InconsistentModelException>(ex);
                Assert.Contains("Mismatch between id of type 'Namespace1.Class2.Class3' and id of declaring type 'Namespace1.Class1'", ex.Message);
            }
        }

        public class AddNestedType
        {
            [Fact]
            public void Throws_ArgumentNullException_if_nestedType_is_null()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                _ = builder.AddAssembly("Assembly", "1.0.0");
                var typeId = new SimpleTypeId("Namespace1", "Class1");
                var sut = builder.AddType("Assembly", typeId);

                // ACT 
                var ex = Record.Exception(() => sut.AddNestedType(nestedType: null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("nestedType", argumentNullException.ParamName);
            }

            [Fact]
            public void Adds_nested_type()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var nestedTypeId = new SimpleTypeId(new SimpleTypeId("Namespace1", "Class1"), "Class2");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly", "1.0.0");
                var declaringType = builder.AddType(assembly.Name, declaringTypeId);

                var nestedType = new _TypeDocumentation(
                    assembly,
                    declaringType,
                    nestedTypeId
                );

                // ACT
                declaringType.AddNestedType(nestedType);

                // ASSERT
                Assert.Collection(
                    declaringType.NestedTypes,
                    type => Assert.Same(nestedType, type)
                );
            }


            [Fact]
            public void Throws_InconsistentModelException_if_nested_type_has_a_different_declaring_type()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var nestedTypeId = new SimpleTypeId(new SimpleTypeId("Namespace1", "Class1"), "Class2");

                var builder1 = new ApiReferenceBuilder();
                var assembly1 = builder1.AddAssembly("Assembly", "1.0.0");
                var declaringType = builder1.AddType(assembly1.Name, declaringTypeId);

                var builder2 = new ApiReferenceBuilder();
                var assembly2 = builder2.AddAssembly("Assembly", "1.0.0");
                var @namespace2 = builder2.AddNamespace("Namespace1");
                var declaringType2 = builder2.AddType(assembly2.Name, declaringTypeId);

                var nestedType = new _TypeDocumentation(
                    assembly1,
                    declaringType2,
                    nestedTypeId
                );

                // ACT
                var ex = Record.Exception(() => declaringType.AddNestedType(nestedType));

                // ASSERT
                Assert.IsType<InconsistentModelException>(ex);
                Assert.Contains("Cannot add nested type with a different declaring type", ex.Message);
            }
        }

        public class Add_Field
        {
            [Fact]
            public void Throws_ArgumentNullException_if_field_is_null()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                _ = builder.AddAssembly("Assembly", "1.0.0");
                var typeId = new SimpleTypeId("Namespace1", "Class1");
                var sut = builder.AddType("Assembly", typeId);

                // ACT 
                var ex = Record.Exception(() => sut.Add(field: null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("field", argumentNullException.ParamName);
            }

            [Fact]
            public void Adds_field()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var fieldTypeId = new SimpleTypeId("System", "Int32");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly", "1.0.0");
                var sut = builder.AddType(assembly.Name, declaringTypeId);


                // ACT
                var addedField = new _FieldDocumentation(sut, "Field1", fieldTypeId);
                sut.Add(addedField);

                // ASSERT
                Assert.Collection(
                    sut.Fields,
                    field => Assert.Same(addedField, field)
                );
            }

            [Fact]
            public void Throws_InconsistentModelException_if_field_has_a_different_declaring_type()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var fieldTypeId = new SimpleTypeId("System", "Int32");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly", "1.0.0");
                var sut = builder.AddType(assembly.Name, declaringTypeId);
                var someOtherType = builder.AddType(assembly.Name, new SimpleTypeId("Namespace1", "Class2"));

                var invalidField = new _FieldDocumentation(someOtherType, "Field1", fieldTypeId);

                // ACT
                var ex = Record.Exception(() => sut.Add(invalidField));

                // ASSERT
                Assert.IsType<InconsistentModelException>(ex);
                Assert.Contains("Cannot add member with a declaring type of 'Namespace1.Class2' to type 'Namespace1.Class1'", ex.Message);
            }
        }

        public class Add_Event
        {
            [Fact]
            public void Throws_ArgumentNullException_if_event_is_null()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                _ = builder.AddAssembly("Assembly", "1.0.0");
                var typeId = new SimpleTypeId("Namespace1", "Class1");
                var sut = builder.AddType("Assembly", typeId);

                // ACT 
                var ex = Record.Exception(() => sut.Add(@event: null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("event", argumentNullException.ParamName);
            }

            [Fact]
            public void Adds_event()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var eventTypeId = new SimpleTypeId("System", "EventHandler");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly", "1.0.0");
                var sut = builder.AddType(assembly.Name, declaringTypeId);


                // ACT
                var addedEvent = new _EventDocumentation(sut, "Event1", eventTypeId);
                sut.Add(addedEvent);

                // ASSERT
                Assert.Collection(
                    sut.Events,
                    @event => Assert.Same(addedEvent, @event)
                );
            }

            [Fact]
            public void Throws_InconsistentModelException_if_event_has_a_different_declaring_type()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var eventTypeId = new SimpleTypeId("System", "EventHandler");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly", "1.0.0");
                var sut = builder.AddType(assembly.Name, declaringTypeId);
                var someOtherType = builder.AddType(assembly.Name, new SimpleTypeId("Namespace1", "Class2"));

                var invalidEvent = new _EventDocumentation(someOtherType, "Event1", eventTypeId);

                // ACT
                var ex = Record.Exception(() => sut.Add(invalidEvent));

                // ASSERT
                Assert.IsType<InconsistentModelException>(ex);
                Assert.Contains("Cannot add member with a declaring type of 'Namespace1.Class2' to type 'Namespace1.Class1'", ex.Message);
            }
        }


        public class Add_Property
        {
            [Fact]
            public void Throws_ArgumentNullException_if_property_is_null()
            {
                // ARRANGE
                var builder = new ApiReferenceBuilder();
                _ = builder.AddAssembly("Assembly", "1.0.0");
                var typeId = new SimpleTypeId("Namespace1", "Class1");
                var sut = builder.AddType("Assembly", typeId);

                // ACT 
                var ex = Record.Exception(() => sut.Add(property: null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("property", argumentNullException.ParamName);
            }

            [Fact]
            public void Adds_property()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var propertyTypeId = new SimpleTypeId("System", "String");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly", "1.0.0");
                var sut = builder.AddType(assembly.Name, declaringTypeId);


                // ACT
                var addedProperty = new _PropertyDocumentation(sut, "Property", propertyTypeId);
                sut.Add(addedProperty);

                // ASSERT
                Assert.Collection(
                    sut.Properties,
                    property => Assert.Same(addedProperty, property)
                );
            }

            [Fact]
            public void Throws_InconsistentModelException_if_property_has_a_different_declaring_type()
            {
                // ARRANGE
                var declaringTypeId = new SimpleTypeId("Namespace1", "Class1");
                var propertyTypeId = new SimpleTypeId("System", "EventHandler");

                var builder = new ApiReferenceBuilder();
                var assembly = builder.AddAssembly("Assembly", "1.0.0");
                var sut = builder.AddType(assembly.Name, declaringTypeId);
                var someOtherType = builder.AddType(assembly.Name, new SimpleTypeId("Namespace1", "Class2"));

                var invalidProperty = new _EventDocumentation(someOtherType, "Property1", propertyTypeId);

                // ACT
                var ex = Record.Exception(() => sut.Add(invalidProperty));

                // ASSERT
                Assert.IsType<InconsistentModelException>(ex);
                Assert.Contains("Cannot add member with a declaring type of 'Namespace1.Class2' to type 'Namespace1.Class1'", ex.Message);
            }
        }
    }
}
