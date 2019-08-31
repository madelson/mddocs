﻿using System;
using Grynwald.MarkdownGenerator;
using Grynwald.MdDocs.CommandLineHelp.Model;

namespace Grynwald.MdDocs.CommandLineHelp.Pages
{
    public class ApplicationPage : IDocument
    {
        private readonly ApplicationDocumentation m_Model;
        private readonly DocumentSet<IDocument> m_DocumentSet;
        private readonly IPathProvider m_PathProvider;

        public ApplicationPage(DocumentSet<IDocument> documentSet, IPathProvider pathProvider, ApplicationDocumentation model)
        {
            m_DocumentSet = documentSet ?? throw new ArgumentNullException(nameof(documentSet));
            m_PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
        }


        public void Save(string path) => GetDocument().Save(path);


        internal MdDocument GetDocument()
        {
            var document = new MdDocument();

            if (m_Model.Commands.Count > 0)
            {
                document.Root.Add(new MdHeading(2, "Commands"));
                document.Root.Add(GetCommandsTable());
            }

            return document;
        }

        private MdTable GetCommandsTable()
        {
            var table = new MdTable(new MdTableRow("Name", "Description"));
            foreach (var command in m_Model.Commands)
            {
                var commandPage = m_DocumentSet[m_PathProvider.GetPath(command)];

                var link = m_DocumentSet.GetLink(this, commandPage, command.Name);

                table.Add(new MdTableRow(link, command.HelpText ?? ""));
            }
            return table;
        }
    }
}
