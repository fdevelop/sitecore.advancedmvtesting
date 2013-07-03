using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitecore.AdvancedMVTSuite.Shell.Applications.Dialogs.Testing;
using Sitecore.Analytics.Data.Items;
using Sitecore.Analytics.Testing.TestingUtils;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.Dialogs.Testing;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.StringExtensions;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Sitecore.Data.Managers;

namespace Sitecore.AdvancedMVTSuite.Shell.Applications.WebEdit.Commands.Testing
{
  public class EditStrategy : WebEditCommand
  {
    /// <summary>The session handle.</summary>
    public readonly string SessionHandle = "PETesting";
    /// <summary>
    /// The execute.
    /// </summary>
    /// <param name="context">The context.</param>
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      Log.Info("COMMAND EXECUTED", this);

      Item item = (context.Items.Length > 0) ? context.Items[0] : null;
      if (item == null)
      {
        return;
      }
      ID clientDeviceId = WebEditUtil.GetClientDeviceId();
      Item item2 = TestingUtil.MultiVariateTesting.GetTestDefinition(item, clientDeviceId);
      if (item2 == null)
      {
        SheerResponse.Alert("Item not found.", new string[0]);
        return;
      }

      NameValueCollection nameValueCollection = new NameValueCollection();
      nameValueCollection["testDefinitionUri"] = item2.Uri.ToString();
      nameValueCollection["itemUri"] = item.Uri.ToString();
      Context.ClientPage.Start(this, "Run", nameValueCollection);
    }

    [UsedImplicitly]
    protected void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (!SheerResponse.CheckModified())
      {
        return;
      }
      ID clientDeviceId = WebEditUtil.GetClientDeviceId();
      if (ID.IsNullOrEmpty(clientDeviceId))
      {
        return;
      }
      if (!args.IsPostBack)
      {
        UrlString urlString = new UrlString(Context.Site.XmlControlPage);
        ItemUri itemUri = ItemUri.ParseQueryString();
        Assert.IsNotNull(itemUri, "uri");

        urlString["xmlcontrol"] = "Sitecore.AdvancedMVTSuite.Shell.Applications.Dialogs.Testing.EditStrategy";
        urlString["itemId"] = ((itemUri != null) ? itemUri.ItemID.ToString() : null);
        urlString["device"] = clientDeviceId.ToString();
        urlString["contentlang"] = ((itemUri != null) ? itemUri.Language.ToString() : null);

        SheerResponse.ShowModalDialog(urlString.ToString(), "520px", "520px", string.Empty, true);
        args.WaitForPostBack();
        return;
      }
      
      if (!args.HasResult)
      {
        return;
      }

      ItemUri itemUri2 = ItemUri.Parse(args.Parameters["testDefinitionUri"]);
      if (itemUri2 == null)
      {
        SheerResponse.Alert("Item not found.", new string[0]);
        return;
      }
      TestDefinitionItem testDefinitionItem = null;
      if (!TestDefinitionItem.TryParse(Client.GetItemNotNull(itemUri2), ref testDefinitionItem))
      {
        SheerResponse.Alert("Item not found.", new string[0]);
        return;
      }
      itemUri2 = ItemUri.Parse(args.Parameters["itemUri"]);
      if (itemUri2 == null)
      {
        SheerResponse.Alert("Item not found.", new string[0]);
        return;
      }
      Item itemNotNull = Client.GetItemNotNull(itemUri2);
      
      string fieldValue = LayoutField.GetFieldValue(itemNotNull.Fields[FieldIDs.LayoutField]);
      LayoutDefinition layoutDefinition = LayoutDefinition.Parse(fieldValue);
      DeviceDefinition device = layoutDefinition.GetDevice(clientDeviceId.ToString());

      // apply result
      testDefinitionItem.InnerItem.Editing.BeginEdit();
      testDefinitionItem.TestStrategy.InnerField.SetValue(Sitecore.AdvancedMVTSuite.Constants.WeightedStrategyItem.ToString(), false);
      testDefinitionItem.InnerItem.Editing.EndEdit();

      Log.Info("Result: " + args.Result, this);

      JObject jObject = JObject.Parse(args.Result);
      foreach (JProperty current2 in jObject.Properties())
      {
        Log.Info("Result: start", this);
        ID variantSettingsId = ShortID.DecodeID(current2.Name.Replace("variant_",string.Empty));
        double value = double.Parse(jObject[current2.Name].Value<string>());
        Log.Info("Result: go " + value + ", " + variantSettingsId, this);

        Item i = testDefinitionItem.InnerItem.Database.GetItem(variantSettingsId);
        i.Editing.BeginEdit();
        i["Weight"] = value.ToString("F0");
        i.Editing.EndEdit();
      }
            
      UrlString url = WebEditCommand.GetUrl();
      WebEditCommand.Reload(url);
    }

    /// <summary>
    /// Queries the state of the command.
    /// </summary>
    /// <param name="context">
    /// The context.
    /// </param>
    /// <returns>
    /// The state of the command.
    /// </returns>
    public override CommandState QueryState(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      if (!Settings.Analytics.Enabled)
      {
        return CommandState.Hidden;
      }
      return base.QueryState(context);
    }
  }
}
