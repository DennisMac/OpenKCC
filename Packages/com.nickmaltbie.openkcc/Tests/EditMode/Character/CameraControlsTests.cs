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

using System.Collections;
using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.CameraControls.CameraController"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class CameraControlsTests : TestBase
    {
        private MockUnityService unityServiceMock;
        private CameraController cameraController;
        private Gamepad gamepad;
        private InputAction lookInputAction;
        private InputAction zoomInputAction;
        private StickControl lookAction;
        private StickControl zoomAction;
        private GameObject box;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            GameObject go = CreateGameObject();
            cameraController = go.AddComponent<CameraController>();
            unityServiceMock = new MockUnityService();
            cameraController.config.cameraTransform = cameraController.transform;
            cameraController.unityService = unityServiceMock;
            unityServiceMock.deltaTime = 0.1f;

            InputActionMap actionMap;
            (gamepad, _, actionMap) = base.SetupInputDevice<Gamepad>();
            zoomAction = gamepad.rightStick;
            lookAction = gamepad.leftStick;
            zoomInputAction = actionMap.AddAction("zoomAction", InputActionType.Value, zoomAction.path);
            lookInputAction = actionMap.AddAction("lookAction", InputActionType.Value, lookAction.path);

            cameraController.config.ZoomAction = InputActionReference.Create(zoomInputAction);
            cameraController.config.LookAction = InputActionReference.Create(lookInputAction);

            lookInputAction.Enable();
            zoomInputAction.Enable();
            cameraController.Start();

            PlayerInputUtils.playerMovementState = PlayerInputState.Allow;

            cameraController.config.minCameraDistance = 0.0f;
            cameraController.config.maxCameraDistance = 10.0f;
        }

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            box = CreateGameObject();
            box.AddComponent<BoxCollider>();
            yield return null;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            PlayerInputUtils.playerMovementState = PlayerInputState.Allow;
        }

        [Test]
        public void Validate_CaemraController_CameraBase()
        {
            Assert.AreEqual(cameraController.config.cameraTransform, cameraController.CameraBase);
        }

        [Test]
        public void Validate_CameraController_DisallowMovement()
        {
            PlayerInputUtils.playerMovementState = PlayerInputState.Deny;

            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);
            Assert.AreEqual(0, cameraController.PlayerHeading.eulerAngles.y);

            Set(lookAction, Vector3.up);
            cameraController.Update();

            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);
            Assert.AreEqual(0, cameraController.PlayerHeading.eulerAngles.y);

            Set(lookAction, Vector3.left);
            cameraController.Update();

            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);
            Assert.AreEqual(0, cameraController.PlayerHeading.eulerAngles.y);
        }

        [Test]
        public void Validate_CameraController_BasicMovement([Values] bool rotatePlayer)
        {
            cameraController.config.rotatePlayer = rotatePlayer;

            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);
            Assert.AreEqual(0, cameraController.PlayerHeading.eulerAngles.y);

            Set(lookAction, Vector3.up);
            cameraController.Update();

            Assert.AreEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);
            Assert.AreEqual(0, cameraController.PlayerHeading.eulerAngles.y);

            Set(lookAction, Vector3.left);
            cameraController.Update();

            Assert.AreEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
            Assert.AreEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Yaw);
            Assert.AreEqual((-cameraController.config.rotationRate * 0.1f * 0.25f + 360) % 360, (cameraController.PlayerHeading.eulerAngles.y + 360) % 360);
        }

        [Test]
        public void Validate_CameraController_IgnoreObjects(
            [NUnit.Framework.Range(2, 10, 4)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * dist;
            cameraController.config.currentDistance = dist;
            cameraController.config.AddIgnoreObject(box);
            cameraController.Update();

            TestUtils.AssertInBounds(cameraController.config.CameraDistance, dist);
            cameraController.config.RemoveIgnoreObject(box);
        }

        [Test]
        public void Validate_CameraController_SphereCast(
            [NUnit.Framework.Range(2, 10, 4)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * dist;
            cameraController.config.currentDistance = dist;
            Debug.DrawRay(cameraController.transform.position, -cameraController.transform.forward * cameraController.config.currentDistance);
            cameraController.Update();

            TestUtils.AssertInBounds(cameraController.config.CameraDistance, dist - 0.5f, 0.25f);
        }

        [Test]
        public void Validate_CameraController_ThirdPerson(
            [NUnit.Framework.Range(2, 10, 4)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * dist;
            cameraController.config.currentDistance = dist;
            cameraController.config.thirdPersonCharacterBase = cameraController.gameObject;

            cameraController.Update();
        }

        [Test]
        public void Validate_CameraController_ThirdPerson_Dither(
            [NUnit.Framework.Range(0, 1, 0.1f)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * (0.5f + dist);
            cameraController.config.currentDistance = 2.0f;
            cameraController.config.thirdPersonCharacterBase = cameraController.gameObject;

            cameraController.Update();
        }
    }
}
