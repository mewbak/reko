﻿#region License
/* 
 * Copyright (C) 1999-2014 John Källén.
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

using Decompiler.Core;
using Decompiler.Gui;
using Decompiler.Gui.Forms;
using Decompiler.Gui.Windows;
using Decompiler.Gui.Windows.Controls;
using Decompiler.Gui.Windows.Forms;
using Decompiler.UnitTests.Mocks;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Windows.Forms;
using Is = Rhino.Mocks.Constraints.Is;

namespace Decompiler.UnitTests.Gui.Windows
{
    [TestFixture]
    public class MemoryViewServiceTests
    {
        private MockRepository repository;

        [SetUp]
        public void Setup()
        {
            repository = new MockRepository();
        }

        [Test]
        public void MVS_ShowingWindowCreatesWindowFrame()
        {
            ServiceContainer sc = new ServiceContainer();
            var shellUi = repository.DynamicMock<IDecompilerShellUiService>();
            var decSvc = repository.StrictMock<IDecompilerService>();
            var windowFrame = repository.DynamicMock<IWindowFrame>();
            sc.AddService(typeof(IDecompilerShellUiService), shellUi);
            sc.AddService<IDecompilerService>(decSvc);

            var interactor = new MemoryViewInteractor();
            interactor.SetSite(sc);
            interactor.CreateControl();

            var service = repository.Stub<MemoryViewServiceImpl>(sc);
            service.Stub(x => x.CreateMemoryViewInteractor()).Return(interactor);

            var svc = (IMemoryViewService)service;
            Expect.Call(shellUi.FindWindow("memoryViewWindow")).Return(null);
            Expect.Call(shellUi.CreateWindow(
                Arg<string>.Is.Anything,
                Arg<string>.Is.Equal("Memory View"),
                Arg<IWindowPane>.Is.Anything))
                .Return(windowFrame);
            Expect.Call(windowFrame.Show);
            repository.ReplayAll();

            svc.ShowMemoryAtAddress(new Program(), new Address(0x10000));
            repository.VerifyAll();
        }

        private T AddStubService<T>(IServiceContainer sc)
        {
            var svc = repository.Stub<T>();
            sc.AddService<T>(svc);
            return svc;
        }

        [Test]
        public void MVS_ShowMemoryAtAddressShouldChangeMemoryControl()
        {
            var sc = new ServiceContainer();
            var ctrl = new LowLevelView();
            var interactor = repository.DynamicMock<MemoryViewInteractor>();
            interactor.Expect(i => i.SelectedAddress).SetPropertyWithArgument(new Address(0x4711));
            interactor.Stub(i => i.Control).Return(ctrl);
            var uiSvc = AddStubService<IDecompilerShellUiService>(sc);
            uiSvc.Stub(x => x.FindWindow(Arg<string>.Is.Anything)).Return(null);
            uiSvc.Stub(x => x.CreateWindow("", "", null))
                .IgnoreArguments()
                .Return(repository.Stub<IWindowFrame>());

            var service = repository.Stub<MemoryViewServiceImpl>(sc);
            service.Stub(x => x.CreateMemoryViewInteractor()).Return(interactor);
            repository.ReplayAll();

            service.ShowMemoryAtAddress(null, new Address(0x4711));

            repository.VerifyAll();
        }

        private class TestMainFormInteractor : MainFormInteractor
        {
            public static TestMainFormInteractor Create()
            {
                var services = new ServiceContainer();
                services.AddService(typeof(IServiceFactory), new ServiceFactory(services));
                return new TestMainFormInteractor(services);
            }

            private TestMainFormInteractor(IServiceProvider services)
                : base(services)
            {
            }
        }
    }
}
