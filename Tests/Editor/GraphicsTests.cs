using UnityEngine;
using NUnit.Framework;

namespace Voxell.Graphics
{
  using Mathx;

  public class GraphicsTests
  {
    private const int ARRAY_COUNT = 100000;

    [Test]
    public void BlellochSumScanTest()
    {
      int[] array = new int[ARRAY_COUNT];
      int[] scannedArray = new int[ARRAY_COUNT];
      for (int i=0; i < ARRAY_COUNT; i++) array[i] = GenerateRandomInt();

      ComputeBuffer cb_in = new ComputeBuffer(ARRAY_COUNT, StrideSize.s_int);
      ComputeBuffer cb_out = new ComputeBuffer(ARRAY_COUNT, StrideSize.s_int);

      ComputeShaderUtil.Init();
      BlellochSumScan.Init();

      cb_in.SetData(array);
      ComputeShaderUtil.ZeroOut(ref cb_out, ARRAY_COUNT);

      BlellochSumScan blellochSumScan = new BlellochSumScan(ARRAY_COUNT);
      blellochSumScan.Scan(ref cb_in, ref cb_out, ARRAY_COUNT);

      cb_out.GetData(scannedArray);

      // using serial min scan method to make sure that the parallel method works
      int sum = 0;
      for (int i=0; i < ARRAY_COUNT; i++)
      {
        Assert.AreEqual(sum, scannedArray[i]);
        sum += array[i];
      }

      cb_in.Dispose();
      cb_out.Dispose();
      blellochSumScan.Dispose();
    }

    [Test]
    public void RadixSortTest()
    {
      uint[] array = new uint[ARRAY_COUNT];
      uint[] sortedArray = new uint[ARRAY_COUNT];
      int[] indices = MathUtil.GenerateSeqArray(ARRAY_COUNT);
      for (uint i=0; i < ARRAY_COUNT; i++) array[i] = GenerateRandomUInt();

      ComputeBuffer cb_sort = new ComputeBuffer(ARRAY_COUNT, StrideSize.s_uint);
      ComputeBuffer cb_indices = new ComputeBuffer(ARRAY_COUNT, StrideSize.s_int);

      ComputeShaderUtil.Init();
      RadixSort.Init();

      cb_sort.SetData(array);
      cb_indices.SetData(indices);

      RadixSort radixSort = new RadixSort(ARRAY_COUNT);
      radixSort.Setup(ref cb_sort, ref cb_indices);
      radixSort.Sort();

      cb_sort.GetData(sortedArray);
      cb_indices.GetData(indices);

      // check if sorting works
      for (int i=0; i < ARRAY_COUNT-1; i++)
        Assert.GreaterOrEqual(sortedArray[i+1], sortedArray[i]);
      // check if indices are sorted properly
      for (int i=0; i < ARRAY_COUNT; i++)
        Assert.AreEqual(array[indices[i]], sortedArray[i]);

      cb_sort.Dispose();
      cb_indices.Dispose();
      radixSort.Dispose();
    }

    private int GenerateRandomInt() => Random.Range(0, ARRAY_COUNT);
    private uint GenerateRandomUInt() => (uint)Random.Range(0, ARRAY_COUNT);
  }
}