using Rin.Core.Yaml.Serialization;
using System.ComponentModel;
using Xunit;

namespace Rin.Core.Yaml.Tests.Serialization;

public class SerializationSubClassTests {
    [Fact]
    public void TestSerializeSubClassWithOverriddenDefaultValues() {
        var settings = new SerializerSettings();
        settings.RegisterAssembly(typeof(SerializationSubClassTests).Assembly);
        var serializer = new Serializer(settings);

        var value = new TestSubClass1();
        var text = serializer.Serialize(value);
        var newValue = serializer.Deserialize<TestSubClass1>(text);
        Assert.Equal(value.IsSelected, newValue.IsSelected);
        Assert.Equal(value.Size.Width, newValue.Size.Width);
        Assert.Equal(value.Size.Height, newValue.Size.Height);
    }

    [Fact]
    public void TestSerializeSubClassWithValuesSameAsBaseClassValues() {
        var settings = new SerializerSettings();
        settings.RegisterAssembly(typeof(SerializationSubClassTests).Assembly);
        var serializer = new Serializer(settings);

        var value = new TestSubClass1();
        value.IsSelected = false;
        value.Size = new() { Width = 10, Height = 10 };
        var text = serializer.Serialize(value);
        var newValue = serializer.Deserialize<TestSubClass1>(text);
        Assert.Equal(value.IsSelected, newValue.IsSelected);
        Assert.Equal(value.Size.Width, newValue.Size.Width);
        Assert.Equal(value.Size.Height, newValue.Size.Height);
    }

    public class DefaultSizeValueAttribute : DefaultValueAttribute {
        public int Width { get; }
        public int Height { get; }

        public override object Value => new Size { Width = Width, Height = Height };

        public DefaultSizeValueAttribute(int width, int height) : base(null) {
            Width = width;
            Height = height;
        }
    }

    public struct Size {
        [DefaultValue(0)]
        public int Width { get; set; }

        [DefaultValue(0)]
        public int Height { get; set; }
    }

    public class TestBaseClass {
        [DefaultValue(false)]
        public bool IsSelected { get; set; }

        [DefaultSizeValue(0, 0)]
        public Size Size { get; set; }
    }

    [DataContractMetadataType(typeof(TestSubClass1Metadata))]
    public class TestSubClass1 : TestBaseClass {
        public TestSubClass1() {
            IsSelected = true;
            Size = new() { Width = 10, Height = 10 };
        }

        class TestSubClass1Metadata {
            [DefaultValue(true)]
            public bool IsSelected { get; set; }

            [DefaultSizeValue(10, 10)]
            public Size Size { get; set; }
        }
    }
}
