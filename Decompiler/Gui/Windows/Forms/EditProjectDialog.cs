﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Decompiler.Gui.Windows.Forms
{
    public partial class EditProjectDialog : Form
    {
        public EditProjectDialog()
        {
            InitializeComponent();
        }


        public TextBox BinaryFilename
        {
            get { return txtInputFile; }
        }

        public TextBox BaseAddress
        {
            get { return txtLoadAddress; }
            
        }

        public TextBox Disassembly
        {
            get { return txtAssemblerFile; }
        }

        public TextBox IntermediateFilename
        {
            get { return txtIntermediateFile; }
            
        }

        public TextBox TypesFilename
        {
            get { return txtHeaderFile; }
        }

        public TextBox OutputFilename
        {
            get { return txtSourceFile; }
        }

        public Button BrowseBinaryFileButton
        {
            get { return btnBrowseInputFile; }
        }
        public Button BrowseAssemblerFileButton
        {
            get { return btnBrowseAssemblerFile; }
        }
        public Button BrowseIntermediateFileButton
        {
            get { return btnBrowseIntermediateFile; }
        }
        public Button BrowseTypesFileButton
        {
            get { return btnBrowseHeaderFile; }
        }
        public Button BrowseOutputFileButton
        {
            get { return btnBrowseSourceFile; }
        }

        public OpenFileDialog OpenFileDialog
        {
            get { return openFileDialog; }
        }

        public SaveFileDialog SaveFileDialog
        {
            get { return saveFileDialog; }
        }
    }
}