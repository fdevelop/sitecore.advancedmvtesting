using Sitecore.Analytics;
using Sitecore.Analytics.Testing;
using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.AdvancedMVTSuite.Sitecore.AdvancedMVTSuite.Testing
{
  public class TestSetSettings
  {
    private TestSet testSet;
    private Item testSetSettingsRoot;
    private Database database;

    public TestSetSettings(TestSet testSet)
    {
      this.testSet = testSet;
      this.database = Tracker.DefinitionDatabase;

      this.testSetSettingsRoot = this.database.GetItem(Constants.AMVTTestsRoot).Children.First(c => new Guid(c["Test"]) == testSet.Id);
    }

    public double GetVariantWeight(Guid variableId, Guid variantId)
    {
      var variable = this.testSetSettingsRoot.Children.First(i => new Guid(i["Variable"]) == variableId);
      var variant = variable.Children.First(i => new Guid(i["Variant"]) == variantId);

      double weight = 100.0;
      double.TryParse(variant["Weight"], out weight);

      return weight / 100.0;
    }
  }
}
