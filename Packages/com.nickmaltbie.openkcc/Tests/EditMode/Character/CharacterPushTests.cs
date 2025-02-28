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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Environment.Pushable;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.KCCMovementEngineWithPush"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class CharacterPushTests : TestBase
    {
        private KCCMovementEngineWithPush character;
        private MockColliderCast mockColliderCast;
        private GameObject pushable;

        [SetUp]
        public override void Setup()
        {
            GameObject go = CreateGameObject();
            mockColliderCast = go.AddComponent<MockColliderCast>();
            character = go.AddComponent<KCCMovementEngineWithPush>();

            pushable = CreateGameObject();
            pushable.AddComponent<BoxCollider>();
            pushable.AddComponent<Rigidbody>();
            pushable.AddComponent<Pushable>().Awake();
        }

        [Test]
        public void Verify_CharacterPush_CanPushObject()
        {
            GameObject noRigidbody = CreateGameObject();
            GameObject kinematic = CreateGameObject();
            GameObject noPushable = CreateGameObject();

            kinematic.AddComponent<Rigidbody>().isKinematic = true;
            noPushable.AddComponent<Rigidbody>();

            Assert.IsFalse(character.CanPushObject(noRigidbody.AddComponent<BoxCollider>()));
            Assert.IsFalse(character.CanPushObject(kinematic.AddComponent<BoxCollider>()));
            Assert.IsFalse(character.CanPushObject(noPushable.AddComponent<BoxCollider>()));
            Assert.IsTrue(character.CanPushObject(pushable.GetComponent<BoxCollider>()));
        }

        [Test]
        public void Verify_CharacterPush_NonPushable_PushObject()
        {
            GameObject noRigidbody = CreateGameObject();
            GameObject kinematic = CreateGameObject();
            GameObject noPushable = CreateGameObject();

            kinematic.AddComponent<Rigidbody>().isKinematic = true;
            noPushable.AddComponent<Rigidbody>();

            character.PushObject(new KinematicCharacterControllerHit(default, default, noRigidbody, default, default, default, default, default));
            character.PushObject(new KinematicCharacterControllerHit(default, kinematic.GetComponent<Rigidbody>(), kinematic, default, default, default, default, default));
            character.PushObject(new KinematicCharacterControllerHit(default, noPushable.GetComponent<Rigidbody>(), noPushable, default, default, default, default, default));
        }

        [Test]
        public void Verify_CharacterPush_PushObject()
        {
            var hit = new KinematicCharacterControllerHit(default, pushable.GetComponent<Rigidbody>(), pushable, default, default, default, default, default);
            character.PushObject(hit);
        }

        [Test]
        public void Verify_CharacterPush_PushObject_ViaMovement()
        {
            int hits = 0;
            IRaycastHit noHit = KCCTestUtils.SetupRaycastHitMock(default(Collider), Vector3.zero, Vector3.zero, Mathf.Infinity);
            IRaycastHit pushableHit = KCCTestUtils.SetupRaycastHitMock(pushable.GetComponent<Collider>(), Vector3.zero, Vector3.up, 0.1f);
            mockColliderCast.OnCastSelf = (Vector3 position, Quaternion rotation, Vector3 direction, float distance, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction, float skinWidth) =>
            {
                // Only return hit for first hit
                if (hits < 1)
                {
                    hit = pushableHit;
                    hits++;
                    return true;
                }

                hit = noHit;
                return false;
            };

            character.MovePlayer(Vector3.forward);
            Assert.IsTrue(hits >= 1);
        }
    }
}
