using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.AdvancedMVTSuite
{
  public class ItemEventHandler
  {
    private bool IsMVTestRelated(Item item)
    {
      return item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestDefinition || item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestVariable || item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestVariant;
    }

    private void AddMVSettings(Item item)
    {
      if (item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestDefinition)
      {
        Item testSettings = ItemManager.CreateItem(item.Name, item.Database.GetItem(Sitecore.AdvancedMVTSuite.Constants.AMVTTestsRoot), Sitecore.AdvancedMVTSuite.Constants.AMVTTestSettingsItem);
        testSettings.Editing.BeginEdit();
        testSettings["Test"] = item.ID.ToString();
        testSettings.Editing.EndEdit();
        return;
      }

      if (item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestVariable)
      {
        var testSetSettings = item.Database.GetItem(Sitecore.AdvancedMVTSuite.Constants.AMVTTestsRoot).Children.First(c => new Guid(c["Test"]) == item.Parent.ID.ToGuid());
        Item testVariableSettings = ItemManager.CreateItem(item.Name, testSetSettings, Sitecore.AdvancedMVTSuite.Constants.AMVTTestVariableSettingsItem);
        testVariableSettings.Editing.BeginEdit();
        testVariableSettings["Variable"] = item.ID.ToString();
        testVariableSettings.Editing.EndEdit();
        return;
      }

      if (item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestVariant)
      {
        var testSetSettings = item.Database.GetItem(Sitecore.AdvancedMVTSuite.Constants.AMVTTestsRoot).Children.First(c => new Guid(c["Test"]) == item.Parent.Parent.ID.ToGuid());
        var testVariableSettings = testSetSettings.Children.First(c => new Guid(c["Variable"]) == item.Parent.ID.ToGuid());

        Item testVariantSettings = ItemManager.CreateItem(item.Name, testVariableSettings, Sitecore.AdvancedMVTSuite.Constants.AMVTVariantSettingsItem);
        testVariantSettings.Editing.BeginEdit();
        testVariantSettings["Variant"] = item.ID.ToString();
        testVariantSettings["Weight"] = "1";
        testVariantSettings.Editing.EndEdit();
        return;
      }
    }

    private void DeleteMVSettings(Item item, Item parent)
    {
      if (item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestDefinition)
      {
        var testSetSettings = item.Database.GetItem(Sitecore.AdvancedMVTSuite.Constants.AMVTTestsRoot).Children.First(c => new Guid(c["Test"]) == item.ID.ToGuid());
        
        ItemManager.DeleteItem(testSetSettings);
        return;
      }

      if (item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestVariable)
      {
        var testSetSettings = item.Database.GetItem(Sitecore.AdvancedMVTSuite.Constants.AMVTTestsRoot).Children.First(c => new Guid(c["Test"]) == parent.ID.ToGuid());
        var testVariableSettings = testSetSettings.Children.First(c => new Guid(c["Variable"]) == item.ID.ToGuid());
        
        ItemManager.DeleteItem(testVariableSettings);
        return;
      }

      if (item.TemplateID == Sitecore.AdvancedMVTSuite.Constants.MVTestVariant)
      {
        var testSetId = parent.ParentID;

        var testSetSettings = item.Database.GetItem(Sitecore.AdvancedMVTSuite.Constants.AMVTTestsRoot).Children.First(c => new Guid(c["Test"]) == testSetId.ToGuid());
        var testVariableSettings = testSetSettings.Children.First(c => new Guid(c["Variable"]) == parent.ID.ToGuid());
        var testVariantSettings = testVariableSettings.Children.First(c => new Guid(c["Variant"]) == item.ID.ToGuid());

        ItemManager.DeleteItem(testVariantSettings);
        return;
      }
    }

    protected void OnItemAdded(object sender, EventArgs args)
    {
      if (args == null)
      {
        return;
      }

      Item item = Event.ExtractParameter(args, 0) as Item;
      if (item == null || !IsMVTestRelated(item))
      {
        return;
      }

      this.AddMVSettings(item);
    }
    
    protected void OnItemAddedRemote(object sender, EventArgs args)
    {
      AddedFromTemplateRemoteEventArgs addedFromTemplateRemoteEventArgs = args as AddedFromTemplateRemoteEventArgs;
      if (addedFromTemplateRemoteEventArgs == null)
      {
        return;
      }
      
      this.AddMVSettings(addedFromTemplateRemoteEventArgs.Item);
    }
    
    protected void OnItemDeleted(object sender, EventArgs args)
    {
      if (args == null)
      {
        return;
      }
      Item item = Event.ExtractParameter(args, 0) as Item;
      if (item == null)
      {
        return;
      }
      Item parent = null;
      ID iD = Event.ExtractParameter(args, 1) as ID;
      if ((ID)null != iD)
      {
        parent = item.Database.GetItem(iD);
      }
      
      this.DeleteMVSettings(item, parent);
    }

    protected void OnItemDeletedRemote(object sender, EventArgs args)
    {
      ItemDeletedRemoteEventArgs itemDeletedRemoteEventArgs = args as ItemDeletedRemoteEventArgs;
      if (itemDeletedRemoteEventArgs == null)
      {
        return;
      }

      this.DeleteMVSettings(itemDeletedRemoteEventArgs.Item, itemDeletedRemoteEventArgs.Item.Database.GetItem(itemDeletedRemoteEventArgs.ParentId));
    }
  }
}
