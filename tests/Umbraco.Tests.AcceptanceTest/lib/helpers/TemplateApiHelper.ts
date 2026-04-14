import {ApiHelpers} from "./ApiHelpers";
import {AliasHelper} from "./AliasHelper";

export class TemplateApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/template/' + id);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/template/' + id);
    return response.status() === 200;
  }

  async create(name: string, alias: string, content: string) {
    const templateData = {
      "name": name,
      "alias": alias,
      "content": content
    };
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/template', templateData);
    // Returns the id of the created template
    return response.headers().location.split("/").pop();
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/template/' + id);
  }

  async update(id: string, template: object) {
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/template/' + id, template);
  }

  async getChildren(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/template/children?parentId=${id}&skip=0&take=10000`);
    const items = await response.json();
    return items.items;
  }

  async getItems(ids: string[]) {
    let idArray = 'id=' + ids[0];
    let i: number;

    for (i = 1; i < ids.length; ++i) {
      idArray += '&id=' + ids[i];
    }

    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/template/item?' + idArray);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/template/root?skip=0&take=10000');
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  private async recurseDeleteChildren(id: string) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.isContainer || child.hasChildren) {
        await this.recurseDeleteChildren(child.id);
      } else {
        await this.delete(child.id);
      }
    }
    return await this.delete(id);
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.name === name) {
        if (!toDelete) {
          return await this.get(child.id);
        }
        if (child.isContainer || child.hasChildren) {
          return await this.recurseDeleteChildren(child.id);
        } else {
          return await this.delete(child.id);
        }
      } else if (child.isContainer || child.hasChildren) {
        await this.recurseChildren(name, child.id, toDelete);
      }
    }
    return false;
  }

  async getByName(name: string) {
    const rootTemplates = await this.getAllAtRoot();
    const jsonTemplates = await rootTemplates.json();

    for (const template of jsonTemplates.items) {
      if (template.name === name) {
        return this.get(template.id);
      } else if (template.isContainer || template.hasChildren) {
        const result = await this.recurseChildren(name, template.id, false);
        if (result) {
          return result;
        }
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootTemplates = await this.getAllAtRoot();
    const jsonTemplates = await rootTemplates.json();

    for (const template of jsonTemplates.items) {
      if (template.name === name) {
        if (template.isContainer || template.hasChildren) {
          await this.recurseDeleteChildren(template.id);
        }
        await this.delete(template.id);
      } else {
        if (template.isContainer || template.hasChildren) {
          await this.recurseChildren(name, template.id, true);
        }
      }
    }
  }

  async createDefaultTemplate(name: string) {
    await this.ensureNameNotExists(name);
    const alias = AliasHelper.toAlias(name);
    return await this.create(name, alias, '');
  }

  async createTemplateWithDisplayingValue(name: string, templateContent: string) {
    await this.ensureNameNotExists(name);
    const alias = AliasHelper.toAlias(name);
    const content =
      '@using Umbraco.Cms.Web.Common.PublishedModels;' +
      '\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage;' +
      '\n@{' +
      '\n\tLayout = null;' +
      '\n}' +
      '\n<div data-mark="content-render-value">' +
      templateContent +
      '\n</div>\n';

    const templateId = await this.create(name, alias, content);
    return templateId === undefined ? '' : templateId;
  }

  async createTemplateWithDisplayingStringValue(name: string, valueAlias: string) {
    const templateContent =
      '\n@{' +
      '\n\tif (Model.HasValue("' + valueAlias + '")){' +
      '\n\t\t<p>@(Model.Value<string>("' + valueAlias + '"))</p>' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingMulitpleStringValue(name: string, valueAlias: string) {
    const templateContent =
      '\n@if(Model.HasValue("' + valueAlias + '"))' +
      '\n{' +
      '\nvar items = Model.Value<IEnumerable<string>>("' + valueAlias + '");' +
      '\n\t<ul>' +
      '\n\t\t@foreach(var item in items)' +
      '\n\t\t{' +
      '\n\t\t\t<li>@item</li>' +
      '\n\t\t}' +
      '\n\t</ul>' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingApprovedColorValue(name: string, valueAlias: string, useLabel: boolean = true) {
    let templateContent = '';
    if (useLabel) {
      templateContent =
        '\n@using Umbraco.Cms.Core.PropertyEditors.ValueConverters' +
        '\n@{' +
        '\n\tvar hexColor = Model.Value("' + valueAlias + '");' +
        '\n\tvar colorLabel = Model.Value<ColorPickerValueConverter.PickedColor>("' + valueAlias + '").Label;' +
        '\n\tif (hexColor != null)' +
        '\n\t{' +
        '\n\t\t<div style="background-color: #@hexColor">@colorLabel</div>' +
        '\n\t}' +
        '\n}';
    } else {
      templateContent =
        '\n@using Umbraco.Cms.Core.PropertyEditors.ValueConverters' +
        '\n@{' +
        '\n\tvar hexColor = Model.Value("' + valueAlias + '");' +
        '\n\tif (hexColor != null)' +
        '\n\t{' +
        '\n\t\t<div style="background-color: #@hexColor">@hexColor</div>' +
        '\n\t}' +
        '\n}';
    }
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingImageCropperValue(name: string, valueAlias: string, cropName: string) {
    const templateContent =
      '\n@using Umbraco.Cms.Core.PropertyEditors.ValueConverters' +
      '\n\t<img src="@Url.GetCropUrl(Model.Value<ImageCropperValue>("' + valueAlias + '"), "' + cropName + '")" />';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingContentPickerValue(name: string, valueAlias: string) {
    const templateContent =
      '\n@{' +
      '\n\tIPublishedContent typedContentPicker = Model.Value<IPublishedContent>("' + valueAlias + '");' +
      '\n\tif (typedContentPicker != null)' +
      '\n\t{' +
      '\n\t\t<p>@typedContentPicker.Name</p>' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingUploadedFileValue(name: string, valueAlias: string) {
    const templateContent =
      '\n@using System.IO;' +
      '\n@{' +
      '\n\tif (Model.HasValue("' + valueAlias + '"))' +
      '\n\t{' +
      '\n\t\tvar myFile = Model.Value<string>("' + valueAlias + '");' +
      '\n\t\t<a href="@myFile">@System.IO.Path.GetFileName(myFile)</a>' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingMemberPickerValue(name: string, valueAlias: string) {
    const templateContent =
      '\n@{' +
      '\n\tif (Model.HasValue("' + valueAlias + '"))' +
      '\n\t{' +
      '\n\t\tvar member = Model.Value<IPublishedContent>("' + valueAlias + '");' +
      '\n\t\t@member.Name' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingMultiURLPickerValue(name: string, valueAlias: string) {
    const templateContent =
      '\n@using Umbraco.Cms.Core.Models' +
      '\n@{' +
      '\n\tvar links = Model.Value<IEnumerable<Link>>("' + valueAlias + '");' +
      '\n\tif (links.Any())' +
      '\n\t{' +
      '\n\t\t<ul>' +
      '\n\t\t\t@foreach (var link in links)' +
      '\n\t\t\t{' +
      '\n\t\t\t\t<li><a href="@link.Url" target="@link.Target">@link.Name</a></li>' +
      '\n\t\t\t}' +
      '\n\t\t</ul>' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingMultipleMediaPickerValue(name: string, valueAlias: string) {
    const templateContent =
      '\n@using Umbraco.Cms.Core.Models;' +
      '\n@{' +
      '\n\tvar medias = Model.Value<IEnumerable<MediaWithCrops>>("' + valueAlias + '");' +
      '\n\tif (medias.Any())' +
      '\n\t{' +
      '\n\t\t<ul>' +
      '\n\t\t\t@foreach (var media in medias)' +
      '\n\t\t\t{' +
      '\n\t\t\t\t<li><a href="@media.MediaUrl()">@media.Name</a></li>' +
      '\n\t\t\t}' +
      '\n\t\t</ul>' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateUsingSiblingOfTypeMethod(name: string, documentTypeName: string) {
    const templateContent =
      '\n<ul>' +
      '\n\t@foreach(var item in Model.SiblingsOfType("' + AliasHelper.toAlias(documentTypeName) + '"))' +
      '\n\t{' +
      '\n\t\t<li><a href="@item.Url()">@item.Name</a></li>' +
      '\n\t}' +
      '\n<ul>';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithContent(name: string, templateContent: string) {
    await this.ensureNameNotExists(name);
    const alias = AliasHelper.toAlias(name);
    return await this.create(name, alias, templateContent);
  }

  private readonly memberAuthTemplateHeader =
    '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n' +
    '@using Umbraco.Cms.Web.Common.Models\n' +
    '@using Umbraco.Cms.Web.Website.Controllers\n' +
    '@using Umbraco.Cms.Web.Website.Models\n' +
    '@using Umbraco.Extensions\n' +
    '@inject MemberModelBuilderFactory memberModelBuilderFactory;\n' +
    '@{\n' +
    '    Layout = null;\n' +
    '    var memberIdentity = Context.User.GetMemberIdentity();\n' +
    '    var isMemberAuthenticated = memberIdentity?.IsAuthenticated ?? false;\n' +
    '}\n' +
    '<!DOCTYPE html>\n' +
    '<html><head><title>Member Auth Test</title></head><body>\n' +
    '<span data-mark="member-auth-state">@(isMemberAuthenticated ? "authenticated" : "anonymous")</span>\n' +
    '<span data-mark="member-auth-name">@memberIdentity?.Name</span>\n';

  private readonly memberLoginFormBody =
    '\n@{ var loginModel = new LoginModel(); loginModel.RedirectUrl = null; }' +
    '\n<div class="login-form">' +
    '\n@using (Html.BeginUmbracoForm<UmbLoginController>("HandleLogin", new { RedirectUrl = loginModel.RedirectUrl }))' +
    '\n{' +
    '\n    <h4>Log in with a local account.</h4>' +
    '\n    <div asp-validation-summary="All" class="text-danger"></div>' +
    '\n    <div class="mb-3">' +
    '\n        <label asp-for="@loginModel.Username" class="form-label"></label>' +
    '\n        <input asp-for="@loginModel.Username" class="form-control" />' +
    '\n    </div>' +
    '\n    <div class="mb-3">' +
    '\n        <label asp-for="@loginModel.Password" class="form-label"></label>' +
    '\n        <input asp-for="@loginModel.Password" class="form-control" />' +
    '\n    </div>' +
    '\n    <div class="mb-3 form-check">' +
    '\n        <input asp-for="@loginModel.RememberMe" class="form-check-input" />' +
    '\n        <label asp-for="@loginModel.RememberMe" class="form-check-label">Remember me</label>' +
    '\n    </div>' +
    '\n    <button type="submit" class="btn btn-primary">Log in</button>' +
    '\n}' +
    '\n</div>\n';

  private readonly memberRegisterFormBody =
    '\n@{' +
    '\n    var registerModel = memberModelBuilderFactory' +
    '\n        .CreateRegisterModel()' +
    '\n        .WithMemberTypeAlias("Member")' +
    '\n        .WithRedirectUrl(null)' +
    '\n        .WithCustomProperties(false)' +
    '\n        .WithAutomaticLogIn(true)' +
    '\n        .Build();' +
    '\n    var success = TempData["FormSuccess"] != null;' +
    '\n}' +
    '\n@if (success)' +
    '\n{' +
    '\n    <p class="text-success">Registration succeeded.</p>' +
    '\n}' +
    '\nelse' +
    '\n{' +
    '\n    using (Html.BeginUmbracoForm<UmbRegisterController>("HandleRegisterMember",' +
    '\n        new { MemberTypeAlias = registerModel.MemberTypeAlias, UsernameIsEmail = registerModel.UsernameIsEmail, RedirectUrl = registerModel.RedirectUrl, AutomaticLogIn = registerModel.AutomaticLogIn }))' +
    '\n    {' +
    '\n        <h2>Create a new account.</h2>' +
    '\n        <div asp-validation-summary="All" class="text-danger"></div>' +
    '\n        <div class="mb-3">' +
    '\n            <label asp-for="@registerModel.Name" class="form-label"></label>' +
    '\n            <input asp-for="@registerModel.Name" class="form-control" />' +
    '\n        </div>' +
    '\n        <div class="mb-3">' +
    '\n            <label asp-for="@registerModel.Email" class="form-label"></label>' +
    '\n            <input asp-for="@registerModel.Email" class="form-control" />' +
    '\n        </div>' +
    '\n        <div class="mb-3">' +
    '\n            <label asp-for="@registerModel.Password" class="form-label"></label>' +
    '\n            <input asp-for="@registerModel.Password" class="form-control" />' +
    '\n        </div>' +
    '\n        <div class="mb-3">' +
    '\n            <label asp-for="@registerModel.ConfirmPassword" class="form-label"></label>' +
    '\n            <input asp-for="@registerModel.ConfirmPassword" class="form-control" />' +
    '\n        </div>' +
    '\n        <button type="submit" class="btn btn-primary">Register</button>' +
    '\n    }' +
    '\n}\n';

  private readonly memberProfileFormBody =
    '\n@{' +
    '\n    var profileModel = await memberModelBuilderFactory' +
    '\n        .CreateProfileModel()' +
    '\n        .WithRedirectUrl(null)' +
    '\n        .WithCustomProperties(false)' +
    '\n        .BuildForCurrentMemberAsync();' +
    '\n    var success = TempData["FormSuccess"] != null;' +
    '\n}' +
    '\n@if (profileModel != null)' +
    '\n{' +
    '\n    if (success)' +
    '\n    {' +
    '\n        <p class="text-success">Profile updated</p>' +
    '\n    }' +
    '\n    using (Html.BeginUmbracoForm<UmbProfileController>("HandleUpdateProfile", new { RedirectUrl = profileModel.RedirectUrl }))' +
    '\n    {' +
    '\n        <h2>Update your account.</h2>' +
    '\n        <div asp-validation-summary="All" class="text-danger"></div>' +
    '\n        <div class="mb-3">' +
    '\n            <label asp-for="@profileModel.Name" class="form-label"></label>' +
    '\n            <input asp-for="@profileModel.Name" class="form-control" />' +
    '\n        </div>' +
    '\n        <div class="mb-3">' +
    '\n            <label asp-for="@profileModel.Email" class="form-label"></label>' +
    '\n            <input asp-for="@profileModel.Email" class="form-control" />' +
    '\n        </div>' +
    '\n        <input asp-for="@profileModel.UserName" type="hidden" />' +
    '\n        <button type="submit" class="btn btn-primary">Update</button>' +
    '\n    }' +
    '\n}\n';

  private readonly memberLoginStatusBody =
    '\n@{ var logoutModel = new PostRedirectModel(); logoutModel.RedirectUrl = null; }' +
    '\n@if (isMemberAuthenticated)' +
    '\n{' +
    '\n    <div class="login-status">' +
    '\n        <p>Welcome back <strong>@memberIdentity?.Name</strong>!</p>' +
    '\n        @using (Html.BeginUmbracoForm<UmbLoginStatusController>("HandleLogout", new { RedirectUrl = logoutModel.RedirectUrl }))' +
    '\n        {' +
    '\n            <button type="submit" class="btn btn-primary">Log out</button>' +
    '\n        }' +
    '\n    </div>' +
    '\n}\n';

  // Covers login AND logout flows — the login-status partial renders the logout button once
  // the member is authenticated.
  async createMemberLoginTemplate(name: string) {
    return await this.createTemplateWithContent(
      name,
      this.memberAuthTemplateHeader + this.memberLoginFormBody + this.memberLoginStatusBody + '</body></html>\n',
    );
  }

  async createMemberRegistrationTemplate(name: string) {
    return await this.createTemplateWithContent(
      name,
      this.memberAuthTemplateHeader + this.memberRegisterFormBody + this.memberLoginStatusBody + '</body></html>\n',
    );
  }

  // Bundles the login form so the test can authenticate before exercising the profile form
  async createMemberProfileTemplate(name: string) {
    return await this.createTemplateWithContent(
      name,
      this.memberAuthTemplateHeader + this.memberLoginFormBody + this.memberProfileFormBody + this.memberLoginStatusBody + '</body></html>\n',
    );
  }

  async createTemplateWithEntityDataPickerValue(templateName: string, propertyName: string) {
    const templateContent =
      '@using Umbraco.Cms.Core.Models;\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage;\n' +
      '@{\n' +
      '\tLayout = null;\n' +
      '\tvar entityDataPickerValue = Model.Value<EntityDataPickerValue>("' + AliasHelper.toAlias(propertyName) + '");\n' +
      '}\n' +
      '\n' +
      '@if (entityDataPickerValue is null)\n' +
      '{ }\n' +
      'else\n' +
      '{\n' +
      '\t<div data-mark="data-source-render-value">\n' +
      '\t\t<p>@entityDataPickerValue.DataSource</p>\n' +
      '\t</div>\n' +
      '\n' +
      '\t<div data-mark="content-render-value">\n' +
      '\t\t<ul>\n' +
      '\n' +
      '\t\t@foreach (var id in @entityDataPickerValue.Ids)\n' +
      '\t\t{\n' +
      '\t\t\t<li>@id</li>\n' +
      '\t\t}\n' +
      '\t\t</ul>\n' +
      '\t</div>\n' +
      '}';
    return this.createTemplateWithContent(templateName, templateContent);
  }

  async createTemplateWithDisplayingDefaultValue(name: string, valueAlias: string, defaultValue: string) {
    const templateContent =
      '\n\t@Model.Value("' + valueAlias + '", fallback: Fallback.ToDefaultValue, defaultValue: (object)"' + defaultValue + '")';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingBlockListItems(name: string, blockListPropertyName: string, elementPropertyAlias: string, noBlocksMessage: string = 'No blocks available') {
    const templateContent =
      '\n@using Umbraco.Cms.Core.Models.Blocks' +
      '\n@{' +
      '\n\tvar blocks = Model.Value<IEnumerable<BlockListItem>>("' + AliasHelper.toAlias(blockListPropertyName) + '");' +
      '\n\tif (blocks != null && blocks.Any())' +
      '\n\t{' +
      '\n\t\tforeach (var block in blocks)' +
      '\n\t\t{' +
      '\n\t\t\t<p>@block.Content.Value("' + AliasHelper.toAlias(elementPropertyAlias) + '")</p>' +
      '\n\t\t}' +
      '\n\t}' +
      '\n\telse' +
      '\n\t{' +
      '\n\t\t<p>' + noBlocksMessage + '</p>' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }

  async createTemplateWithDisplayingBlockGridItems(name: string, blockGridPropertyName: string, elementPropertyAlias: string, noBlocksMessage: string = 'No blocks available') {
    const templateContent =
      '\n@using Umbraco.Cms.Core.Models.Blocks' +
      '\n@{' +
      '\n\tvar blocks = Model.Value<BlockGridModel>("' + AliasHelper.toAlias(blockGridPropertyName) + '");' +
      '\n\tif (blocks != null && blocks.Any())' +
      '\n\t{' +
      '\n\t\tforeach (var block in blocks)' +
      '\n\t\t{' +
      '\n\t\t\t<p>@block.Content.Value("' + AliasHelper.toAlias(elementPropertyAlias) + '")</p>' +
      '\n\t\t}' +
      '\n\t}' +
      '\n\telse' +
      '\n\t{' +
      '\n\t\t<p>' + noBlocksMessage + '</p>' +
      '\n\t}' +
      '\n}';
    return this.createTemplateWithDisplayingValue(name, templateContent);
  }
}
