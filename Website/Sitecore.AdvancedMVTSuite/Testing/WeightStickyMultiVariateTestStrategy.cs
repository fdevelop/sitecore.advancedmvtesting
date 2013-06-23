using Sitecore.Analytics;
using Sitecore.Analytics.Testing;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.AdvancedMVTSuite.Sitecore.AdvancedMVTSuite.Testing;

namespace Sitecore.AdvancedMVTSuite.Testing
{
  public class WeightStickyMultiVariateTestStrategy : MultiVariateTestStrategyBase, IMultiVariateTestStrategy
  {
    private int hashCode;

    public virtual int HashCode
    {
      get
      {
        if (this.hashCode == 0)
        {
          return Tracker.CurrentVisit.VisitorId.GetHashCode();
        }
        return this.hashCode;
      }
      set
      {
        this.hashCode = value;
      }
    }

    protected TestCombination GenerateCombinationByWeight(TestSet testset, Random rng)
    {
      Assert.ArgumentNotNull(testset, "testset");
      Assert.ArgumentNotNull(rng, "rng");

      TestSetSettings settings = new TestSetSettings(testset);

      byte[] array = new byte[testset.Variables.Count];
      for (int i = 0; i < array.Length; i++)
      {
        double genRnd = rng.NextDouble();

        byte variant = 0;
        for (byte k = 0; k < testset.Variables[i].Values.Count; k++)
        {
          var weight = settings.GetVariantWeight(testset.Variables[i].Id, testset.Variables[i].Values[k].Id);

          if (genRnd <= weight)
          {
            variant = k;
            break;
          }
          else
          {
            genRnd -= weight;
          }
        }

        array[i] = variant;
      }

      return new TestCombination(array, testset);
    }

    public TestCombination GetTestCombination(TestSet testset)
    {
      Random rng = new Random(this.HashCode ^ testset.Id.GetHashCode());
      return this.GenerateCombinationByWeight(testset, rng);
    }
  }
}
