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

    public double GetVariableTotalWeight(Guid variableId)
    {
      var variable = this.testSetSettingsRoot.Children.First(i => new Guid(i["Variable"]) == variableId);
      var variants = variable.Children.Where(v => v.TemplateID == Constants.AMVTVariantSettingsItem);

      double total = 0.0;
      foreach (var v in variants)
      {
        double weight = 0.0;
        if (double.TryParse(v["Weight"], out weight))
        {
          total += weight;
        }
      }

      return total;
    }

    public Item GetVariableSettingsItem(Guid variableId)
    {
      var variable = this.testSetSettingsRoot.Children.First(i => new Guid(i["Variable"]) == variableId);

      return variable;
    }

    public Item GetVariantSettingsItem(Guid variableId, Guid variantId)
    {
      var variable = this.testSetSettingsRoot.Children.First(i => new Guid(i["Variable"]) == variableId);
      var variant = variable.Children.First(i => new Guid(i["Variant"]) == variantId);

      return variant;
    }

    public double GetVariantWeight(Guid variableId, Guid variantId)
    {
      var variable = this.testSetSettingsRoot.Children.First(i => new Guid(i["Variable"]) == variableId);
      var variant = variable.Children.First(i => new Guid(i["Variant"]) == variantId);

      double weight = 1.0;
      double.TryParse(variant["Weight"], out weight);

      return weight;
    }

    public void SetVariantWeight(Guid variableId, Guid variantId, double value)
    {
      var variable = this.testSetSettingsRoot.Children.First(i => new Guid(i["Variable"]) == variableId);
      var variant = variable.Children.First(i => new Guid(i["Variant"]) == variantId);

      variant.Editing.BeginEdit();
      variant["Weight"] = value.ToString("F0");
      variant.Editing.EndEdit();
    }
  }
}
