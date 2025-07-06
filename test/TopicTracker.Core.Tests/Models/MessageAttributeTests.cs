using System;
using System.Text.Json;
using System.Threading.Tasks;
using TUnit.Assertions;
using TUnit.Core;
using TopicTracker.Core.Models;

namespace TopicTracker.Core.Tests.Models;

public class MessageAttributeTests
{
    [Test]
    public async Task MessageAttribute_Should_Have_Required_Properties()
    {
        // Act
        var attribute = new MessageAttribute
        {
            DataType = "String",
            StringValue = "TestValue"
        };
        
        // Assert
        await Assert.That(attribute.DataType).IsEqualTo("String");
        await Assert.That(attribute.StringValue).IsEqualTo("TestValue");
    }
    
    [Test]
    public async Task MessageAttribute_Should_Be_Immutable()
    {
        // Act
        var attribute = new MessageAttribute
        {
            DataType = "String",
            StringValue = "TestValue"
        };
        
        // Assert - Properties should be init-only
        await Assert.That(attribute).IsNotNull();
    }
    
    [Test]
    public async Task MessageAttribute_Should_Support_String_Type()
    {
        // Act
        var attribute = new MessageAttribute
        {
            DataType = "String",
            StringValue = "Hello World"
        };
        
        // Assert
        await Assert.That(attribute.DataType).IsEqualTo("String");
        await Assert.That(attribute.StringValue).IsEqualTo("Hello World");
        await Assert.That(attribute.BinaryValue).IsNull();
    }
    
    [Test]
    public async Task MessageAttribute_Should_Support_Number_Type()
    {
        // Act
        var attribute = new MessageAttribute
        {
            DataType = "Number",
            StringValue = "123.45"
        };
        
        // Assert
        await Assert.That(attribute.DataType).IsEqualTo("Number");
        await Assert.That(attribute.StringValue).IsEqualTo("123.45");
        await Assert.That(attribute.BinaryValue).IsNull();
    }
    
    [Test]
    public async Task MessageAttribute_Should_Support_Binary_Type()
    {
        // Arrange
        var binaryData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in bytes
        
        // Act
        var attribute = new MessageAttribute
        {
            DataType = "Binary",
            BinaryValue = binaryData
        };
        
        // Assert
        await Assert.That(attribute.DataType).IsEqualTo("Binary");
        await Assert.That(attribute.BinaryValue).IsNotNull();
        await Assert.That(attribute.BinaryValue!.Length).IsEqualTo(5);
        await Assert.That(attribute.BinaryValue[0]).IsEqualTo((byte)0x48);
        await Assert.That(attribute.StringValue).IsNull();
    }
    
    [Test]
    public async Task MessageAttribute_Should_Support_String_Array_Type()
    {
        // String.Array is a valid SNS attribute type
        var attribute = new MessageAttribute
        {
            DataType = "String.Array",
            StringValue = "[\"value1\", \"value2\", \"value3\"]"
        };
        
        // Assert
        await Assert.That(attribute.DataType).IsEqualTo("String.Array");
        await Assert.That(attribute.StringValue).Contains("value1");
    }
    
    [Test]
    public async Task MessageAttribute_Should_Support_Number_Array_Type()
    {
        // Number.Array is a valid SNS attribute type
        var attribute = new MessageAttribute
        {
            DataType = "Number.Array",
            StringValue = "[1, 2, 3, 4, 5]"
        };
        
        // Assert
        await Assert.That(attribute.DataType).IsEqualTo("Number.Array");
        await Assert.That(attribute.StringValue).IsEqualTo("[1, 2, 3, 4, 5]");
    }
    
    [Test]
    public async Task MessageAttribute_Should_Have_Value_Property_For_Compatibility()
    {
        // The Value property should return StringValue for non-binary types
        var stringAttr = new MessageAttribute
        {
            DataType = "String",
            StringValue = "test"
        };
        
        var numberAttr = new MessageAttribute
        {
            DataType = "Number",
            StringValue = "123"
        };
        
        // Assert
        await Assert.That(stringAttr.Value).IsEqualTo("test");
        await Assert.That(numberAttr.Value).IsEqualTo("123");
    }
    
    [Test]
    public async Task MessageAttribute_Should_Serialize_To_Json()
    {
        // Arrange
        var attribute = new MessageAttribute
        {
            DataType = "String",
            StringValue = "TestValue"
        };
        
        // Act
        var json = JsonSerializer.Serialize(attribute, SnsJsonContext.Default.MessageAttribute);
        
        // Assert
        await Assert.That(json).IsNotNull();
        await Assert.That(json).Contains("DataType");
        await Assert.That(json).Contains("StringValue");
        await Assert.That(json).Contains("String");
        await Assert.That(json).Contains("TestValue");
    }
    
    [Test]
    public async Task MessageAttribute_Should_Serialize_Binary_As_Base64()
    {
        // Arrange
        var binaryData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var attribute = new MessageAttribute
        {
            DataType = "Binary",
            BinaryValue = binaryData
        };
        
        // Act
        var json = JsonSerializer.Serialize(attribute, SnsJsonContext.Default.MessageAttribute);
        
        // Assert
        await Assert.That(json).IsNotNull();
        await Assert.That(json).Contains("BinaryValue");
        await Assert.That(json).Contains("SGVsbG8="); // Base64 for "Hello"
    }
    
    [Test]
    public async Task MessageAttribute_Should_Deserialize_From_Json()
    {
        // Arrange
        var json = @"{
            ""DataType"": ""String"",
            ""StringValue"": ""TestValue""
        }";
        
        // Act
        var attribute = JsonSerializer.Deserialize<MessageAttribute>(
            json, 
            SnsJsonContext.Default.MessageAttribute
        );
        
        // Assert
        await Assert.That(attribute).IsNotNull();
        await Assert.That(attribute!.DataType).IsEqualTo("String");
        await Assert.That(attribute.StringValue).IsEqualTo("TestValue");
    }
    
    [Test]
    public async Task MessageAttribute_Should_Deserialize_Binary_From_Base64()
    {
        // Arrange
        var json = @"{
            ""DataType"": ""Binary"",
            ""BinaryValue"": ""SGVsbG8=""
        }";
        
        // Act
        var attribute = JsonSerializer.Deserialize<MessageAttribute>(
            json, 
            SnsJsonContext.Default.MessageAttribute
        );
        
        // Assert
        await Assert.That(attribute).IsNotNull();
        await Assert.That(attribute!.DataType).IsEqualTo("Binary");
        await Assert.That(attribute.BinaryValue).IsNotNull();
        await Assert.That(attribute.BinaryValue!.Length).IsEqualTo(5);
        await Assert.That(attribute.BinaryValue[0]).IsEqualTo((byte)0x48); // 'H'
    }
}