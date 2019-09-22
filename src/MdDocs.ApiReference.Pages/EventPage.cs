﻿using System.IO;
using Grynwald.MarkdownGenerator;
using Grynwald.MdDocs.ApiReference.Model;
using Microsoft.Extensions.Logging;

namespace Grynwald.MdDocs.ApiReference.Pages
{
    public sealed class EventPage : SimpleMemberPage<EventDocumentation>
    {
        internal EventPage(ILinkProvider linkProvider, PageFactory pageFactory, EventDocumentation model, ILogger logger)
            : base(linkProvider, pageFactory, model, logger)
        {
        }


        protected override MdHeading GetHeading() =>
            new MdHeading($"{Model.TypeDocumentation.DisplayName}.{Model.Name} Event", 1);

        // No "Value"section for events
        protected override void AddValueSection(MdContainerBlock block)
        { }
    }
}
