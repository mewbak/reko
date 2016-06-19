﻿#region License
/* 
 * Copyright (C) 1999-2016 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using Reko.Core;
using Reko.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reko.Gui.Windows.Controls
{
    /// <summary>
    /// Renders code and data side by side.
    /// </summary>
    public class MixedCodeDataControl : TextView
    {
        public MixedCodeDataControl()
        {
            this.ProgramChanged += delegate { OnProgramChanged(); };

            OnProgramChanged();

            this.Disposed += MixedCodeDataControl_Disposed;
        }

        public Program Program {
            get
            {
                return program;
            }
            set
            {
                if (program != null)
                    program.ImageMap.MapChanged -= ImageMap_MapChanged;
                program = value;
                if (program != null)
                    program.ImageMap.MapChanged += ImageMap_MapChanged;
                ProgramChanged.Fire(this);
            }
        }

        private Program program;
        public event EventHandler ProgramChanged;

        public Address TopAddress { get { return addrTop; } set { addrTop = value; OnTopAddressChanged(); } }
        private Address addrTop;


        private void OnProgramChanged()
        {
            try
            {
                if (program != null)
                {
                    Model = new MixedCodeDataModel(program);
                    addrTop = (Address)Model.CurrentPosition;
                    return;
                }
            }
            catch (Exception ex)
            {
                Services.RequireService<IDiagnosticsService>().Error(ex, "An error occurred while displaying the program.");
            }
            Model = new EmptyEditorModel();
            addrTop = null;
        }

        private void OnTopAddressChanged()
        {
            if (program != null)
            {
                Model.MoveToLine(addrTop, 0);
                RecomputeLayout();
                UpdateScrollbar();
                Invalidate();
            }
        }

        protected override void OnScroll()
        {
            addrTop = (Address)Model.CurrentPosition;
            base.OnScroll();
        }

        private void RefreshModel()
        {
            var currentAddress = (Address)Model.CurrentPosition;
            var model = new MixedCodeDataModel(program);
            model.MoveToLine(currentAddress, 0);
            this.addrTop = (Address)model.CurrentPosition;
            this.Model = model;
        }

        private void ImageMap_MapChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(RefreshModel));
            else
                RefreshModel();
        }

        private void MixedCodeDataControl_Disposed(object sender, EventArgs e)
        {
            if (program != null)
                program.ImageMap.MapChanged -= ImageMap_MapChanged;
        }

        public Address GetAnchorAddress()
        {
            var pt = GetAnchorMiddlePoint();
            var memoryTextSpan = GetTagFromPoint(pt) as MixedCodeDataModel.MemoryTextSpan;
            if (memoryTextSpan == null || memoryTextSpan.Address == null)
                return (Address)anchorPos.Line;
            return memoryTextSpan.Address;
        }
    }
}