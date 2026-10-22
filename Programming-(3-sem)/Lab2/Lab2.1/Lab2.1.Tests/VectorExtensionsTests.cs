using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class VectorExtensionsTests
{
    [TestMethod]
    public void RemovePositive_RemovesPositiveNumbersOnly()
    {
        var v = new Vector(new double[] { -1, 0, 2.5, -3, 4 });
        var result = v.RemovePositive();
        var expected = new Vector(new double[] { -1, 0, -3 });

        Assert.AreEqual(expected.Length, result.Length);
        for (int i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], result[i]);
    }

    [TestMethod]
    public void RemovePositive_AllPositive_ReturnsEmptyVector()
    {
        var v = new Vector(new double[] { 1, 2, 3 });
        var result = v.RemovePositive();
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void RemovePositive_NoPositive_ReturnsSameContent()
    {
        var v = new Vector(new double[] { -1, 0, -5 });
        var result = v.RemovePositive();
        Assert.AreEqual(v.Length, result.Length);
        for (int i = 0; i < v.Length; i++)
            Assert.AreEqual(v[i], result[i]);
    }

    [TestMethod]
    public void RemovePositive_NullVector_ThrowsArgumentNullException()
    {
        Vector v = null;
        try
        {
            v.RemovePositive();
            Assert.Fail("Ожидалось исключение ArgumentNullException, но его не было");
        }
        catch (ArgumentNullException){}
    }
}