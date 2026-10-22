using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void CutStart_ValidCount_ReturnsSubstring()
    {
        string input = "Hello World";
        string result = input.CutStart(6);
        Assert.AreEqual("World", result);
    }

    [TestMethod]
    public void CutStart_CountEqualsLength_ReturnsEmpty()
    {
        Assert.AreEqual("", "abc".CutStart(3));
    }

    [TestMethod]
    public void CutStart_CountGreaterThanLength_ReturnsEmpty()
    {
        Assert.AreEqual("", "Hi".CutStart(10));
    }

    [TestMethod]
    public void CutStart_NegativeCount_ThrowsArgumentException()
    {
        try
        {
            "test".CutStart(-1);
            Assert.Fail("Ожидалось исключение ArgumentException, но его не было");
        }
        catch (ArgumentException){}
    }

    [TestMethod]
    public void CutStart_NullString_ThrowsArgumentNullException()
    {
        try
        {
            ((string)null).CutStart(2);
            Assert.Fail("Ожидалось исключение ArgumentNullException, но его не было");
        }
        catch (ArgumentNullException){}
    }
}