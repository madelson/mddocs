﻿using Grynwald.MarkdownGenerator;
using Grynwald.MdDocs.ApiReference.Model;

using static Grynwald.MarkdownGenerator.FactoryMethods;

namespace Grynwald.MdDocs.ApiReference.Pages
{
    class FieldPage : SimpleMemberPage<FieldDocumentation>
    {
        public override OutputPath OutputPath { get; }


        public FieldPage(PageFactory pageFactory, string rootOutputPath, FieldDocumentation model)
            : base(pageFactory, rootOutputPath, model)
        {
            OutputPath = new OutputPath(GetTypeDir(Model.TypeDocumentation), "Fields", $"{Model.Name}.md");
        }



        protected override MdHeading GetHeading() =>
            Heading($"{Model.TypeDocumentation.DisplayName}.{Model.Name} Field", 1);

        protected override void AddValueSection(MdContainerBlock block)
        {
            block.Add(Heading("Field Value", 2));
            block.Add(
                GetMdParagraph(Model.Type)
            );

            if (Model.Value != null)
            {
                block.Add(ConvertToBlock(Model.Value));
            }
        }
    }
}
