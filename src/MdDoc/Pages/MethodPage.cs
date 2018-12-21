﻿using System;
using System.IO;
using MdDoc.Model;

using static Grynwald.MarkdownGenerator.FactoryMethods;

namespace MdDoc.Pages
{
    //TODO: Add documentation from XML comments
    class MethodPage : MemberPage<MethodDocumentation>
    {
        public override OutputPath OutputPath { get; }
            
        protected override MethodDocumentation Model { get; }


        public MethodPage(PageFactory pageFactory, string rootOutputPath, MethodDocumentation model) 
            : base(pageFactory, rootOutputPath)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));   
            OutputPath = new OutputPath(Path.Combine(GetTypeDir(Model.TypeDocumentation), "methods", $"{Model.TypeDocumentation.TypeId.Name}.{Model.Name}.md"));
        }


        public override void Save()
        {
            var document = Document(
                Heading($"{Model.TypeDocumentation.DisplayName}.{Model.Name} Method", 1)
            );

            AddDeclaringTypeSection(document.Root);            

            AddOverloadsSection(document.Root, Model.Overloads);

            AddDetailSections(document.Root, Model.Overloads);

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            document.Save(OutputPath);
        }
    }
}
