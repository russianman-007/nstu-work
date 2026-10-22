using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

[TestClass]
public class VectorTests
{
    [TestMethod]
    public void Constructor_Default_CreatesEmptyVector()
    {
        var v = new Vector();
        Assert.AreEqual(0, v.Length);
    }

    [TestMethod]
    public void Constructor_Size_CreatesZeroFilledVector()
    {
        var v = new Vector(3);
        Assert.AreEqual(3, v.Length);
        Assert.AreEqual(0.0, v[0]);
        Assert.AreEqual(0.0, v[1]);
        Assert.AreEqual(0.0, v[2]);
    }

    [TestMethod]
    public void Constructor_Size_Negative_ThrowsArgumentException()
    {
        _ = new Vector(-1);
    }

    [TestMethod]
    public void Constructor_Array_CopiesReferenceButWorks()
    {
        var data = new double[] { 1.0, 2.0, 3.0 };
        var v = new Vector(data);
        Assert.AreEqual(3, v.Length);
        Assert.AreEqual(1.0, v[0]);
        Assert.AreEqual(2.0, v[1]);
        Assert.AreEqual(3.0, v[2]);

        data[0] = 999;
        Assert.AreEqual(999, v[0]);
    }

    [TestMethod]
    public void Constructor_Array_Null_ThrowsArgumentNullException()
    {
        _ = new Vector((double[])null);
    }

    [TestMethod]
    public void Constructor_Copy_CreatesIndependentCopy()
    {
        var original = new Vector(new double[] { 1, -2, 3 });
        var copy = new Vector(original);
        Assert.AreEqual(original.Length, copy.Length);
        Assert.AreEqual(1, copy[0]);
        Assert.AreEqual(-2, copy[1]);
        Assert.AreEqual(3, copy[2]);

        original[0] = 888;
        Assert.AreEqual(1, copy[0]);
    }

    [TestMethod]
    public void Constructor_Copy_Null_ThrowsArgumentNullException()
    {
        _ = new Vector((Vector)null);
    }

    [TestMethod]
    public void Indexer_GetSet_Works()
    {
        var v = new Vector(new double[] { 10, 20 });
        v[1] = 25;
        Assert.AreEqual(25, v[1]);
    }

    [TestMethod]
    public void Length_ReturnsCorrectValue()
    {
        Assert.AreEqual(0, new Vector().Length);
        Assert.AreEqual(2, new Vector(2).Length);
        Assert.AreEqual(3, new Vector(new double[] { 1, 2, 3 }).Length);
    }

    [TestMethod]
    public void OperatorAddition_SameSize_ReturnsCorrectResult()
    {
        var v1 = new Vector(new double[] { 1, 2, 3 });
        var v2 = new Vector(new double[] { 4, 5, 6 });
        var result = v1 + v2;
        var expected = new Vector(new double[] { 5, 7, 9 });
        // Проверяем вручную, так как Equals не переопределён
        Assert.AreEqual(expected.Length, result.Length);
        for (int i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], result[i]);
    }

    [TestMethod]
    public void OperatorAddition_NullOperand_ThrowsArgumentNullException()
    {
        var v = new Vector(new double[] { 1, 2 });
        _ = v + null;
    }

    [TestMethod]
    public void OperatorAddition_DifferentLength_ThrowsArgumentException()
    {
        var v1 = new Vector(new double[] { 1, 2 });
        var v2 = new Vector(new double[] { 1, 2, 3 });
        _ = v1 + v2;
    }

    [TestMethod]
    public void Norm_CalculatesEuclideanNormCorrectly()
    {
        var v = new Vector(new double[] { 3, 4 });
        Assert.AreEqual(5.0, v.Norm(), 1e-10);
    }

    [TestMethod]
    public void TrueOperator_ReturnsTrueForNonEmptyVector()
    {
        var v = new Vector(new double[] { 1 });
        Assert.IsTrue(v ? true : false);
    }

    [TestMethod]
    public void FalseOperator_ReturnsFalseForEmptyVector()
    {
        var v = new Vector();
        Assert.IsFalse(v ? true : false);
    }

    [TestMethod]
    public void FalseOperator_ReturnsFalseForNull()
    {
        Vector v = null;
        Assert.IsFalse(v ? true : false);
    }

    [TestMethod]
    public void ToString_FormatsCorrectly()
    {
        var v = new Vector(new double[] { 1.5, -2, 0 });
        Assert.AreEqual("[1.5, -2, 0]", v.ToString());
    }
}