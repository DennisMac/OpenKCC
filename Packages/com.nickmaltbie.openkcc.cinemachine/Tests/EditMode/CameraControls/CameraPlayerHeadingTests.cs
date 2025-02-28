﻿// Copyright (C) 2023 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using nickmaltbie.OpenKCC.cinemachine.CameraControls;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.EditMode.CameraControls
{
    [TestFixture]
    public class CameraPlayerHeadingTests : TestBase
    {
        [Test]
        public void Validate_GetHeading()
        {
            GameObject go = CreateGameObject();
            CameraPlayerHeading heading = go.AddComponent<CameraPlayerHeading>();

            // Try without a main camera
            Assert.AreEqual(Quaternion.identity, heading.PlayerHeading);

            GameObject camera = CreateGameObject();
            camera.AddComponent<Camera>();
            camera.tag = "MainCamera";

            // Try again with camera heading
            TestUtils.AssertInBounds(heading.PlayerHeading * Vector3.forward, Vector3.forward);

            // Move camera and assert it faces the right direction
            camera.transform.rotation = Quaternion.Euler(0, 90, 0);
            TestUtils.AssertInBounds(heading.PlayerHeading * Vector3.forward, Vector3.right);
        }
    }
}
