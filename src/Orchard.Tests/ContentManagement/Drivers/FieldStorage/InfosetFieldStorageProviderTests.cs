﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers.FieldStorage;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Drivers.FieldStorage {
    public class InfosetFieldStorageProviderTests {
        private IContainer _container;
        private IFieldStorageProvider _provider;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<InfosetFieldStorageProvider>().As<IFieldStorageProvider>();

            _container = builder.Build();
            _provider = _container.Resolve<IFieldStorageProvider>();
        }

        private ContentPartDefinition FooPartDefinition() {
            return new ContentPartDefinitionBuilder()
                .Named("Foo")
                .WithField("Bar")
                .Build();
        }

        private ContentPart CreateContentItemPart() {
            var partDefinition = FooPartDefinition();
            var typeDefinition = new ContentTypeDefinitionBuilder()
                .WithPart(partDefinition, part => { })
                .Build();
            var contentItem = new ContentItem {
                VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord()
                }
            };
            var contentPart = new ContentPart {
                TypePartDefinition = typeDefinition.Parts.Single()
            };
            contentItem.Weld(contentPart);
            return contentPart;
        }

        [Test]
        public void BoundStorageIsNotNull() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());
            Assert.That(storage, Is.Not.Null);
        }

        [Test]
        public void GettingUnsetNamedAndUnnamedValueIsSafeAndNull() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());
            Assert.That(storage.Getter(null), Is.Null);
            Assert.That(storage.Getter("value"), Is.Null);
            Assert.That(storage.Getter("This is a test"), Is.Null);
        }

        [Test]
        public void ValueThatIsSetIsAlsoReturned() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            Assert.That(storage.Getter("alpha"), Is.Null);
            storage.Setter("alpha", "one");
            Assert.That(storage.Getter("alpha"), Is.Not.Null);
            Assert.That(storage.Getter("alpha"), Is.EqualTo("one"));
        }

        [Test]
        public void NullAndEmptyValueNamesAreTreatedTheSame() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            Assert.That(storage.Getter(null), Is.Null);
            Assert.That(storage.Getter(""), Is.Null);
            storage.Setter(null, "one");
            Assert.That(storage.Getter(null), Is.EqualTo("one"));
            Assert.That(storage.Getter(""), Is.EqualTo("one"));
            storage.Setter(null, "two");
            Assert.That(storage.Getter(null), Is.EqualTo("two"));
            Assert.That(storage.Getter(""), Is.EqualTo("two"));
        }

        [Test]
        public void RecordDataPropertyReflectsChangesToFields() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            storage.Setter(null, "one");
            storage.Setter("alpha", "two");

            Assert.That(part.ContentItem.Record.Data, Is.EqualTo("<Data><Foo><Bar alpha=\"two\">one</Bar></Foo></Data>"));
        }

        [Test]
        public void ChangingRecordDataHasImmediateEffectOnStorageAccessors() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            storage.Setter(null, "one");
            storage.Setter("alpha", "two");

            Assert.That(part.ContentItem.Record.Data, Is.EqualTo("<Data><Foo><Bar alpha=\"two\">one</Bar></Foo></Data>"));
            part.ContentItem.Record.Data = "<Data><Foo><Bar alpha=\"four\">three</Bar></Foo></Data>";

            storage.Setter(null, "three");
            storage.Setter("alpha", "four");
        }

        [Test]
        public void VersionedSettingOnInfosetField() {
            Assert.Fail("todo");
        }
    }
}
