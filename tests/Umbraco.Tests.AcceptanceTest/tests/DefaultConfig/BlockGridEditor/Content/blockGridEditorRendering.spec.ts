import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {ContentBuilder, DocumentTypeBuilder, PartialViewBuilder,} from "@umbraco/json-models-builders";
import {expect} from "@playwright/test";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {umbracoConfig} from "@umbraco/playwright-testhelpers/dist/umbraco.config";

test.describe('BlockGridEditorRending', () => {
  const documentName = 'DocumentTest';
  const blockGridName = 'BlockGridTest';
  const elementTitleName = 'ElementTitle';
  const contentText = "ContentTest";
  
  const documentAlias = 'documentTest';
  const blockGridAlias = 'blockGridTest';
  const elementTitleAlias = "elementTitle";

  const elementTitleLabel = 'Title';
  const elementTitleLabelAlias = AliasHelper.toAlias(elementTitleLabel);
  
  test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTitleName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
    await umbracoApi.templates.ensureNameNotExists(documentName);
    await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementTitleAlias + '.cshtml')
  });

  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTitleName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
    await umbracoApi.templates.ensureNameNotExists(documentName);
    await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementTitleAlias + '.cshtml')
  });

  async function createElementWithRTE(umbracoApi, elementName, elementAlias, label, labelAlias){
    const element = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(elementAlias)
      .AsElementType()
      .addGroup()
        .withName('TestString')
        .withAlias('testString')
        .addRichTextProperty()
          .withLabel(label)
          .withAlias(labelAlias)
        .done()
      .done()
      .build();
    return await umbracoApi.documentTypes.save(element);
  }
  
  async function createDocumentWithTemplateAndDataType(umbracoApi, dataType){
    const docType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAlias('documentTest')
      .withDefaultTemplate('documentTest')
      .withAllowAsRoot(true)
      .addGroup()
        .withName('BlockGridGroup')
        .withAlias('blockGridGroup')
        .addCustomProperty(dataType['id'])
          .withLabel(blockGridName)
          .withAlias(blockGridAlias)
        .done()
      .done()
      .build();
    return await umbracoApi.documentTypes.save(docType);
  }
  
  async function editDefaultTemplate(umbracoApi){
    await umbracoApi.templates.edit(documentName, '@using Umbraco.Cms.Web.Common.PublishedModels;\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.' + documentName + '>\n' +
      '@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;' + '\n' +
      '@await Html.GetBlockGridHtmlAsync(Model.' + blockGridName + ')');
  }
  
  async function createPartialViewWithArea(umbracoApi, elementAlias, elementLabelAlias){
    const partialViewElementTitle = new PartialViewBuilder()
      .withName(elementAlias)
      .withContent('@using Umbraco.Extensions' + '\n' +
        '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>' +
        '\n' +
        '<div>' + '\n' +
        '<h1>@Model.Content.Value("' + elementLabelAlias + '")</h1>' + '\n' +
        '<div>@await Html.GetBlockGridItemAreasHtmlAsync(Model)</div>' +
        '</div>')
      .build();
    partialViewElementTitle.virtualPath = "/Views/Partials/blockgrid/Components/";
    return await umbracoApi.partialViews.save(partialViewElementTitle);
  }
  
  async function createPartialViewWithBlock(umbracoApi,elementAlias, elementLabelAlias){
    const partialViewElementBody = new PartialViewBuilder()
      .withName(elementAlias)
      .withContent('@using Umbraco.Extensions' + '\n' +
        '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>' +
        '\n' +
        '<div>' + '\n' +
        '<h3>@Model.Content.Value("' + elementLabelAlias + '")</h3>' + '\n' +
        '</div>')
      .build();
    partialViewElementBody.virtualPath = "/Views/Partials/blockgrid/Components/";
    await umbracoApi.partialViews.save(partialViewElementBody);
  }
  
  test('can render content with a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    const element = await createElementWithRTE(umbracoApi, elementTitleName, elementTitleAlias, elementTitleLabel, elementTitleLabelAlias);

    const dataType = await umbracoApi.dataTypes.createDefaultBlockGrid(umbracoApi, blockGridName, element);

    await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

    await editDefaultTemplate(umbracoApi);

    await createPartialViewWithBlock(umbracoApi, elementTitleAlias, elementTitleLabelAlias);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(documentAlias)
      .withAction(ConstantHelper.actions.publish)
      .addVariant()
        .withName('BlockGridContent')
        .withSave(true)
        .withPublish(true)
        .addProperty()
          .withAlias(blockGridAlias)
          .addBlockGridValue()
            .addBlockGridEntry()
              .withContentTypeKey(element['key'])
              .appendContentProperties(element.groups[0].properties[0].alias, contentText)
            .done()
            .addLayout()
              .withContentUdi(element['key'])
            .done()
          .done()
        .done()
      .done()
      .withTemplateAlias(documentName)
      .build();
    await umbracoApi.content.save(rootContentNode);
    
    // Assert
    await page.goto(umbracoConfig.environment.baseUrl);
    await expect(page).toHaveScreenshot('Block-grid-editor.png');
  });

  test('can render content with a block grid editor with two elements', async ({page, umbracoApi, umbracoUi}) => {
    // ElementBody
    const elementBodyName = "ElementBody";
    const elementBodyAlias = AliasHelper.toAlias(elementBodyName);
    const elementBodyLabel = 'Body';
    const elementBodyLabelAlias = AliasHelper.toAlias(elementBodyLabel);

    await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
    await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');

    const element = await createElementWithRTE(umbracoApi, elementTitleName, elementTitleAlias, elementTitleLabel, elementTitleLabelAlias);
    const elementBody = await createElementWithRTE(umbracoApi, elementBodyName, elementBodyAlias, elementBodyLabel, elementBodyLabelAlias);
    
    const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementBody['key'])
      .done()
      .build();
    const dataType= await umbracoApi.dataTypes.save(dataTypeBlockGrid);

    await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

    await editDefaultTemplate(umbracoApi);

    // Creates partial view for the ElementTitle
    await createPartialViewWithBlock(umbracoApi, elementTitleAlias, elementTitleLabelAlias);

    // Creates partial view for the ElementBody
    await createPartialViewWithBlock(umbracoApi, elementBodyAlias, elementBodyLabelAlias);
    
    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(documentAlias)
      .withAction(ConstantHelper.actions.publish)
      .addVariant()
        .withName('BlockGridContent')
        .withSave(true)
        .withPublish(true)
        .addProperty()
          .withAlias(blockGridAlias)
          .addBlockGridValue()
            .addBlockGridEntry()
              .withContentTypeKey(element['key'])
              .appendContentProperties(element.groups[0].properties[0].alias, contentText)
            .done()
            .addBlockGridEntry()
              .withContentTypeKey(elementBody['key'])
              .appendContentProperties(elementBody.groups[0].properties[0].alias, 'Lorem Ipsum')
            .done()
            .addLayout()
              .withContentUdi(element['key'])
            .done()
            .addLayout()
              .withContentUdi(elementBody['key'])
            .done()
          .done()
        .done()
      .done()
      .withTemplateAlias(documentName)
      .build();
    await umbracoApi.content.save(rootContentNode);
    
    // Assert
    await page.goto(umbracoConfig.environment.baseUrl);
    await expect(page).toHaveScreenshot('Block-grid-editor-with-two-elements.png');

    // Clean 
    await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
    await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');
  });

  test.describe('Rendering areas', () => {
    test('can render a block grid with an area', async ({page, umbracoApi, umbracoUi}) => {
      // ElementBody
      const elementBodyName = "ElementBody";
      const elementBodyAlias = AliasHelper.toAlias(elementBodyName);
      const elementBodyLabel = 'Body';
      const elementBodyLabelAlias = AliasHelper.toAlias(elementBodyLabel);

      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');

      const element = await createElementWithRTE(umbracoApi, elementTitleName, elementTitleAlias, elementTitleLabel, elementTitleLabelAlias);
      const elementBody = await createElementWithRTE(umbracoApi, elementBodyName, elementBodyAlias, elementBodyLabel, elementBodyLabelAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias('titleArea')
          .done()
        .done()
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .build();
      const dataType= await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

      await editDefaultTemplate(umbracoApi);

      // Creates partial view for the ElementTitle
      await createPartialViewWithArea(umbracoApi, elementTitleAlias, elementTitleLabelAlias);

      // Creates partial view for the ElementBody
      await createPartialViewWithBlock(umbracoApi, elementBodyAlias, elementBodyLabelAlias);
      
      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(documentAlias)
        .withAction(ConstantHelper.actions.publish)
        .addVariant()
          .withName('BlockGridContent')
          .withSave(true)
          .withPublish(true)
          .addProperty()
            .withAlias(blockGridAlias)
            .addBlockGridValue()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, contentText)
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementBody['key'])
                .appendContentProperties(elementBody.groups[0].properties[0].alias, 'Lorem Ipsum')
              .done()
              .addLayout()
                .withContentUdi(element['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[0].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'])
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .withTemplateAlias(documentName)
        .build();
      await umbracoApi.content.save(rootContentNode);
      
      // Assert
      await page.goto(umbracoConfig.environment.baseUrl);
      await expect(page).toHaveScreenshot('Block-grid-editor-with-area.png');
      
      // Clean 
      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');
    })

    test('can render a block grid with multiple areas', async ({page, umbracoApi, umbracoUi}) => {
      // ElementBody
      const elementBodyName = "ElementBody";
      const elementBodyAlias = AliasHelper.toAlias(elementBodyName);
      const elementBodyLabel = 'Body';
      const elementBodyLabelAlias = AliasHelper.toAlias(elementBodyLabel);

      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');

      const element = await createElementWithRTE(umbracoApi, elementTitleName, elementTitleAlias, elementTitleLabel, elementTitleLabelAlias);
      const elementBody = await createElementWithRTE(umbracoApi, elementBodyName, elementBodyAlias, elementBodyLabel, elementBodyLabelAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias('titleArea')
          .done()
        .done()
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

      await editDefaultTemplate(umbracoApi);

      // Creates partial view for the ElementTitle
      await createPartialViewWithArea(umbracoApi, elementTitleAlias, elementTitleLabelAlias);

      // Creates partial view for the ElementBody
      await createPartialViewWithBlock(umbracoApi, elementBodyAlias, elementBodyLabelAlias);

      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(documentAlias)
        .withAction(ConstantHelper.actions.publish)
        .addVariant()
          .withName('BlockGridContent')
          .withSave(true)
          .withPublish(true)
          .addProperty()
            .withAlias(blockGridAlias)
            .addBlockGridValue()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, contentText)
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementBody['key'])
                .appendContentProperties(elementBody.groups[0].properties[0].alias, 'Lorem ipsum')
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementBody['key'])
                .appendContentProperties(elementBody.groups[0].properties[0].alias, 'dolor sit amet,')
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementBody['key'])
                .appendContentProperties(elementBody.groups[0].properties[0].alias, 'consectetuer adipiscing elit,')
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementBody['key'])
                .appendContentProperties(elementBody.groups[0].properties[0].alias, 'sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.')
              .done()
              .addLayout()
                .withContentUdi(element['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[0].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'],0)
                  .done()
                  .addItems()
                    .withContentUdi(elementBody['key'],1)
                  .done()
                  .addItems()
                    .withContentUdi(elementBody['key'],2)
                  .done()
                  .addItems()
                    .withContentUdi(elementBody['key'],3)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .withTemplateAlias(documentName)
        .build();
      await umbracoApi.content.save(rootContentNode);
      
      // Assert
      await page.goto(umbracoConfig.environment.baseUrl);
      await expect(page).toHaveScreenshot('Block-grid-editor-with-multiple-areas.png');

      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');
    });

    test('can render a block grid with an area that has another area', async ({page, umbracoApi, umbracoUi}) => {
      // ElementBody
      const elementBodyName = "ElementBody";
      const elementBodyAlias = AliasHelper.toAlias(elementBodyName);
      const elementBodyLabel = 'Body';
      const elementBodyLabelAlias = AliasHelper.toAlias(elementBodyLabel);

      // ElementFooter
      const elementFooterName = "ElementFooter";
      const elementFooterAlias = AliasHelper.toAlias(elementFooterName);
      const elementFooterLabel = 'Footer';
      const elementFooterLabelAlias = AliasHelper.toAlias(elementFooterLabel);

      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.documentTypes.ensureNameNotExists(elementFooterName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementFooterAlias + '.cshtml');

      const element = await createElementWithRTE(umbracoApi, elementTitleName, elementTitleAlias, elementTitleLabel, elementTitleLabelAlias);
      const elementBody = await createElementWithRTE(umbracoApi, elementBodyName, elementBodyAlias, elementBodyLabel, elementBodyLabelAlias);
      const elementFooter = await createElementWithRTE(umbracoApi, elementFooterName, elementFooterAlias, elementFooterLabel, elementFooterLabelAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias('titleArea')
          .done()
        .done()
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])  
          .addArea()
            .withAlias('bodyArea')
          .done()
        .done()
        .addBlock()
          .withContentElementTypeKey(elementFooter['key'])
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

      await editDefaultTemplate(umbracoApi);

      // Creates partial view for the ElementTitle
      await createPartialViewWithArea(umbracoApi, elementTitleAlias, elementTitleLabelAlias);

      // Creates partial view for the ElementBody
      await createPartialViewWithArea(umbracoApi, elementBodyAlias, elementBodyLabelAlias);

      // Creates partial view for the ElementFooter
      await createPartialViewWithBlock(umbracoApi, elementFooterAlias, elementFooterLabelAlias);

      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(documentAlias)
        .withAction(ConstantHelper.actions.publish)
        .addVariant()
          .withName('BlockGridContent')
          .withSave(true)
          .withPublish(true)
          .addProperty()
            .withAlias(blockGridAlias)
            .addBlockGridValue()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, contentText)
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementBody['key'])
                .appendContentProperties(elementBody.groups[0].properties[0].alias, 'Lorem ipsum')
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementFooter['key'])
                .appendContentProperties(elementFooter.groups[0].properties[0].alias, 'dolor sit amet')
              .done()
              .addLayout()
                .withContentUdi(element['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[0].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'])
                    .addAreas()
                      .withKey(dataType.preValues[0].value[1].areas[0].key)
                      .addItems()
                        .withContentUdi(elementFooter['key'])
                      .done()
                    .done()
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .withTemplateAlias(documentName)
        .build();
      await umbracoApi.content.save(rootContentNode);

      // Assert
      await page.goto(umbracoConfig.environment.baseUrl);
      await expect(page).toHaveScreenshot('Block-grid-editor-with-area-with-area.png');

      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.documentTypes.ensureNameNotExists(elementFooterName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementFooterAlias + '.cshtml');
    });
  });

  test.describe('Layout Stylesheet', () => {
    test('can render a block with a custom layout stylesheet', async ({page, umbracoApi, umbracoUi}) => {
      const stylesheetName = "Title.css";
      const stylesheetPath = "stylesheet/" + stylesheetName;
      const stylesheetMimeType = "text/css";

      await umbracoApi.media.ensureNameNotExists(stylesheetName);

      const stylesheetData = await umbracoApi.media.createFileWithFile(stylesheetName, stylesheetName, stylesheetPath, stylesheetMimeType);
      const stylesheetDataPath = stylesheetData.mediaLink;

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withStylesheet(stylesheetDataPath)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

      await editDefaultTemplate(umbracoApi);

      const partialViewElement = new PartialViewBuilder()
        .withName(elementTitleAlias)
        .withContent('@using Umbraco.Extensions' + '\n' +
          '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>' +
          '\n' +
          '<div>' + '\n' +
          '<h1 class="title">@Model.Content.Value("' + elementTitleLabelAlias + '") </h1>' + '\n' +
          '</div>' + '\n' +
          '<link rel="stylesheet" href="@Url.Content("' + stylesheetDataPath + '")" />')
        .build();
      partialViewElement.virtualPath = "/Views/Partials/blockgrid/Components/";
      await umbracoApi.partialViews.save(partialViewElement);
      
      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(documentAlias)
        .withAction(ConstantHelper.actions.publish)
        .addVariant()
          .withName('BlockGridContent')
          .withSave(true)
          .withPublish(true)
          .addProperty()
            .withAlias(blockGridAlias)
            .addBlockGridValue()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, contentText)
              .done()
              .addLayout()
                .withContentUdi(element['key'])
              .done()
            .done()
          .done()
        .done()
        .withTemplateAlias(documentName)
        .build();
      await umbracoApi.content.save(rootContentNode);

      // Assert
      await page.goto(umbracoConfig.environment.baseUrl);
      await expect(page).toHaveScreenshot('Block-grid-editor-with-custom-stylesheet.png');

      // Clean
      await umbracoApi.media.ensureNameNotExists(stylesheetName);
    });

    test('can render a block with another area where each block has a different custom layout stylesheet', async ({page, umbracoApi, umbracoUi}) => {
      // TitleStylesheet
      const stylesheetTitleName = "Title.css";
      const stylesheetTitlePath = "stylesheet/" + stylesheetTitleName;
      const stylesheetMimeType = "text/css";
      // BodyStylesheet
      const stylesheetBodyName = "Body.css";
      const stylesheetBodyPath = "stylesheet/" + stylesheetBodyName;
      // ElementBody
      const elementBodyName = "ElementBody";
      const elementBodyAlias = AliasHelper.toAlias(elementBodyName);
      
      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.media.ensureNameNotExists(stylesheetTitleName);
      await umbracoApi.media.ensureNameNotExists(stylesheetBodyName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');

      const stylesheetTitleData = await umbracoApi.media.createFileWithFile(stylesheetTitleName, stylesheetTitleName, stylesheetTitlePath, stylesheetMimeType);
      const stylesheetBodyData = await umbracoApi.media.createFileWithFile(stylesheetBodyName, stylesheetBodyName, stylesheetBodyPath, stylesheetMimeType);
      const stylesheetTitleDataPath = stylesheetTitleData.mediaLink;
      const stylesheetBodyDataPath = stylesheetBodyData.mediaLink;

      const elementTitle = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementTitle['key'])
          .withStylesheet(stylesheetTitleDataPath)
          .addArea()
            .withAlias('TitleArea')
          .done()
        .done()
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
          .withStylesheet(stylesheetBodyDataPath)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

      await editDefaultTemplate(umbracoApi);

      const partialViewElementTitle = new PartialViewBuilder()
        .withName(elementTitleAlias)
        .withContent('@using Umbraco.Extensions' + '\n' +
          '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>' +
          '\n' +
          '<div>' + '\n' +
          '<h1 class="title">@Model.Content.Value("' + elementTitleLabelAlias + '") </h1>' + '\n' +
          '</div>' + '\n' +
          '<link rel="stylesheet" href="@Url.Content("' + stylesheetTitleDataPath + '")" />' +
          '<div>' + '\n' +
          '@await Html.GetBlockGridItemAreasHtmlAsync(Model)'+ '\n' +
          '</div>')
        .build();
      partialViewElementTitle.virtualPath = "/Views/Partials/blockgrid/Components/";
      await umbracoApi.partialViews.save(partialViewElementTitle);

      const partialViewElementBody = new PartialViewBuilder()
        .withName(elementBodyAlias)
        .withContent('@using Umbraco.Extensions' + '\n' +
          '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>' +
          '\n' +
          '<div>' + '\n' +
          '<h1 class="body">@Model.Content.Value("' + elementTitleLabelAlias + '") </h1>' + '\n' +
          '</div>' + '\n' +
          '<link rel="stylesheet" href="@Url.Content("' + stylesheetBodyDataPath + '")" />')
        .build();
      partialViewElementBody.virtualPath = "/Views/Partials/blockgrid/Components/";
      await umbracoApi.partialViews.save(partialViewElementBody);

      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(documentAlias)
        .withAction(ConstantHelper.actions.publish)
        .addVariant()
          .withName('BlockGridContent')
          .withSave(true)
          .withPublish(true)
          .addProperty()
            .withAlias(blockGridAlias)
            .addBlockGridValue()
              .addBlockGridEntry()
                .withContentTypeKey(elementTitle['key'])
                .appendContentProperties(elementTitle.groups[0].properties[0].alias, contentText)
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(elementBody['key'])
                .appendContentProperties(elementBody.groups[0].properties[0].alias, 'Lorem Ipsum')
              .done()
              .addLayout()
                .withContentUdi(elementTitle['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[0].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'])
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .withTemplateAlias(documentName)
        .build();
      await umbracoApi.content.save(rootContentNode);

      // Assert
      await page.goto(umbracoConfig.environment.baseUrl);
      await expect(page).toHaveScreenshot('Block-grid-editor-with-two-custom-stylesheets.png');
      
      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
      await umbracoApi.media.ensureNameNotExists(stylesheetTitleName);
      await umbracoApi.media.ensureNameNotExists(stylesheetBodyName);
      await umbracoApi.partialViews.ensureNameNotExists('blockgrid/Components', elementBodyAlias + '.cshtml');
    });
  });
  
  test.describe('Image', () => {
    test('can render a block grid with an image', async ({page, umbracoApi, umbracoUi}) => {
      // Image
      const imageName = "Umbraco";
      const umbracoFileValue = {"src": "Umbraco.png"};
      const imageFileName = "Umbraco.png";
      const imagePath = 'mediaLibrary/' + imageFileName;
      const imageMimeType = "image/png";

      await umbracoApi.media.ensureNameNotExists(imageName);
      
      const imageData = await umbracoApi.media.createImageWithFile(imageName, umbracoFileValue, imageFileName, imagePath, imageMimeType);
      
      const element = new DocumentTypeBuilder()
        .withName(elementTitleName)
        .withAlias(elementTitleAlias)
        .AsElementType()
        .addGroup()
          .withName('ImageGroup')
          .withAlias('imageGroup')
          .addImagePickerProperty()
            .withLabel('Image')
            .withAlias('image')
          .done()
        .done()
        .build();
      await umbracoApi.documentTypes.save(element);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createDocumentWithTemplateAndDataType(umbracoApi, dataType);

      await editDefaultTemplate(umbracoApi);

      const partialViewImage = new PartialViewBuilder()
        .withName(elementTitleAlias)
        .withContent('@using Umbraco.Extensions' + '\n' +
          '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridItem>' + '\n' +
          '\n' +
          '@{' + '\n' +
          'var typedMediaPickerSingle = Model.Content.Value<Umbraco.Cms.Core.Models.MediaWithCrops>("image");' + '\n' +
          '<img src="@typedMediaPickerSingle.MediaUrl()" style="object-fit:cover; width:100%; height:100%;" alt="Umbraco"/>' + '\n' +
          '}')
        .build();
      partialViewImage.virtualPath = "/Views/Partials/blockgrid/Components/";
      await umbracoApi.partialViews.save(partialViewImage);
      
      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(documentAlias)
        .withAction(ConstantHelper.actions.publish)
        .addVariant()
          .withName(blockGridName)
          .withSave(true)
          .addProperty()
            .withAlias(blockGridAlias)
            .addBlockGridValue()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .addImage()
                  .withMediaKey(imageData.key)
                .done()
              .done() 
              .addLayout()
                .withContentUdi(element['key'])
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);
      
      // Assert
      await page.goto(umbracoConfig.environment.baseUrl);
      await expect(page).toHaveScreenshot('Block-grid-editor-with-image.png');
      
      // Clean 
      await umbracoApi.media.ensureNameNotExists(imageName);
    });
  });
});
