using Sitecore.AdvancedMVTSuite.Sitecore.AdvancedMVTSuite.Testing;
using Sitecore.Analytics.Data.Items;
using Sitecore.Analytics.Testing;
using Sitecore.Analytics.Testing.TestingUtils;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Sitecore.AdvancedMVTSuite.Shell.Applications.Dialogs.Testing
{
  public class EditStrategy : DialogForm
  {
    protected Border Details;

    protected string ContextItemId
    {
      get
      {
        return base.ServerProperties["itemid"] as string;
      }
      set
      {
        Assert.IsNotNullOrEmpty(value, "value");
        base.ServerProperties["itemid"] = value;
      }
    }
    
    protected string DeviceId
    {
      get
      {
        return base.ServerProperties["deviceid"] as string;
      }
      set
      {
        Assert.IsNotNullOrEmpty(value, "value");
        base.ServerProperties["deviceid"] = value;
      }
    }

    protected Language ContentLanguage
    {
      get
      {
        string queryString = WebUtil.GetQueryString("contentlang");
        if (!string.IsNullOrEmpty(queryString))
        {
          return Language.Parse(queryString);
        }
        return Context.Language;
      }
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      if (Context.ClientPage.IsEvent)
      {
        return;
      }

      this.ContextItemId = WebUtil.GetQueryString("itemId");
      this.DeviceId = WebUtil.GetQueryString("device");
      Item item = Client.ContentDatabase.GetItem(this.ContextItemId, this.ContentLanguage);
      if (item == null)
      {
        return;
      }
      if (!WebEditUtil.CanDesignItem(item))
      {
        this.OK.Disabled = true;
      }
      MultivariateTestDefinitionItem testDefinition = TestingUtil.MultiVariateTesting.GetTestDefinition(item, new ID(this.DeviceId));
      if (testDefinition == null)
      {
        return;
      }

      TestSet testSet = TestingUtil.GetTestSet((TestDefinitionItem)testDefinition);
      TestSetSettings testSettings = new TestSetSettings(testSet);
      this.Render(testSet, testSettings);
    }

    protected override void OnOK(object sender, EventArgs args)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(args, "args");

      Log.Info(WebUtil.GetFormValue("result"), this);

      SheerResponse.SetDialogValue(WebUtil.GetFormValue("result"));

      base.OnOK(sender, args);
    }

    protected void Render(TestSet testSet, TestSetSettings settings)
    {
      HtmlTextWriter htmlTextWriter = new HtmlTextWriter(new StringWriter());
      foreach (var c in testSet.Variables)
      {
        htmlTextWriter.Write(this.RenderVariable(c, settings));
      }
      string text = htmlTextWriter.InnerWriter.ToString();
      if (!string.IsNullOrEmpty(text))
      {
        this.Details.InnerHtml = text;
      }
    }

    protected string RenderVariable(TestVariable c, TestSetSettings settings)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("<div style='border: 1px solid #aaa; padding:3px;'><span style='font-weight:bold;'>Variable: </span>" + c.Label);
      sb.Append("<br/>");
      foreach (var v in c.Values)
      {
        sb.Append("Variant: " + v.Label);
        sb.Append("<br/>");
        sb.Append("Weight: <input id='variant_" + new ShortID(settings.GetVariantSettingsItem(c.Id, v.Id).ID) + "' type='text' onchange='updateResultValue()' value='" + settings.GetVariantWeight(c.Id, v.Id) + "'></input>");
        sb.Append("<br/>");
        sb.Append("<br/>");
      }
      sb.Append("</div>");

      return sb.ToString();
    }
  }
}
