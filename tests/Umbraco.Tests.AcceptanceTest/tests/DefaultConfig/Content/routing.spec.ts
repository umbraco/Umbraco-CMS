import {ApiHelpers, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {ContentBuilder, DocumentTypeBuilder} from "@umbraco/json-models-builders";

test.describe('Routing', () => {
  let swedishLanguageId = 0;
  const swedishCulture = "sv";
  const danishCulture = "da"
  const nodeName = "Root";
  const childNodeName = "Child";
  const grandChildNodeName = "Grandchild";
  const rootDocTypeName = "Test document type";

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.languages.ensureCultureNotExists(danishCulture);
    await umbracoApi.languages.ensureCultureNotExists(swedishCulture);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.login();
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.languages.ensureCultureNotExists(danishCulture);
    await umbracoApi.languages.ensureCultureNotExists(swedishCulture);
  });

  async function saveNewLanguages(umbracoApi: ApiHelpers) {
    // Save Danish
    const url = process.env.URL + "/umbraco/backoffice/umbracoapi/language/SaveLanguage";
    const danishRequestBody = {
      culture: danishCulture
    }

    await umbracoApi.post(url, danishRequestBody);

    // Save Swedish
    const swedishRequestBody = {
      culture: swedishCulture
    }

    await umbracoApi.post(url, swedishRequestBody).then((response) => {
      swedishLanguageId = response["id"];
    });
  }

  async function configureDomain(id, name, lang, umbracoApi: ApiHelpers) {
    //Save domain for child node
    const url = process.env.URL + "/umbraco/backoffice/umbracoapi/content/PostSaveLanguageAndDomains"
    const body = {
      nodeId: id,
      domains: [
        {
          name: name,
          lang: lang
        }],
      language: 0
    }

    await umbracoApi.post(url, body);
  }

  test('Root node published in language A, Child node published in language A', async ({page, umbracoApi, umbracoUi}) => {
    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .withAllowCultureVariation(true)
      .build();

    await saveNewLanguages(umbracoApi);

    await umbracoApi.documentTypes.save(rootDocType).then(async (generatedRootDocType) => {
      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(generatedRootDocType["alias"])
        .withAction("publishNew")
        .addVariant()
          .withCulture('en-US')
          .withName(nodeName)
          .withSave(true)
          .withPublish(true)
        .done()
        .build();

      await umbracoApi.content.save(rootContentNode).then(async (generatedRootContent) => {
        const childContentNode = new ContentBuilder()
          .withContentTypeAlias(generatedRootDocType["alias"])
          .withAction("saveNew")
          .withParent(generatedRootContent["id"])
          .addVariant()
            .withCulture('en-US')
            .withName(childNodeName)
            .withSave(true)
          .done()
          .build();

        await umbracoApi.content.save(childContentNode);
      });
    });

    // Refresh to update the tree
    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [nodeName, childNodeName]));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Pop-up with what cultures you want to publish shows, click it
    await page.locator('.btn-success').last().click()

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
  });

  test(    'Root node published in language A, Child node published in language B', async ({page, umbracoApi, umbracoUi}) => {
    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .withAllowCultureVariation(true)
      .build();

    await saveNewLanguages(umbracoApi);

    await umbracoApi.documentTypes.save(rootDocType).then(async (generatedRootDocType) => {
      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(generatedRootDocType["alias"])
        .withAction("publishNew")
        .addVariant()
          .withCulture('en-US')
          .withName(nodeName)
          .withSave(true)
          .withPublish(true)
        .done()
        .build();

      await umbracoApi.content.save(rootContentNode).then(async (generatedRootContent) => {
        const childContentNode = new ContentBuilder()
          .withContentTypeAlias(generatedRootDocType["alias"])
          .withAction("saveNew")
          .withParent(generatedRootContent["id"])
          .addVariant()
            .withCulture('en-US')
            .withName(childNodeName)
            .withSave(true)
          .done()
          .addVariant()
            .withCulture(swedishCulture)
            .withName("Bärn")
            .withSave(true)
          .done()
          .build();

        await umbracoApi.content.save(childContentNode);
      });
    });

    // Refresh to update the tree
    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [nodeName, childNodeName]));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    await expect(await page.locator('.umb-list')).toBeVisible();
    await page.locator('.checkbox').last().click();
    // Pop-up with what cultures you want to publish shows, click it
    await page.locator('.btn-success').last().click()

    // Assert
    await expect(await umbracoUi.getSuccessNotification()).toHaveCount(2);
    await expect(await page.locator('.alert-warning')).toBeVisible();
  });
  
  test('Root node published in language A, Child node published in language A + B, Grandchild published in A + B', async ({page, umbracoApi, umbracoUi}) => {
    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .withAllowCultureVariation(true)
      .build();

    await saveNewLanguages(umbracoApi);

    await umbracoApi.documentTypes.save(rootDocType).then(async (generatedRootDocType) => {
      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(generatedRootDocType["alias"])
        .withAction("publishNew")
        .addVariant()
          .withCulture('en-US')
          .withName(nodeName)
          .withSave(true)
          .withPublish(true)
        .done()
        .build();

      await umbracoApi.content.save(rootContentNode).then(async (generatedRootContent) => {

        await configureDomain(generatedRootContent["id"], "/en", 1, umbracoApi);
        const childContentNode = new ContentBuilder()
          .withContentTypeAlias(generatedRootDocType["alias"])
          .withAction("saveNew")
          .withParent(generatedRootContent["id"])
          .addVariant()
            .withCulture('en-US')
            .withName(childNodeName)
            .withSave(true)
          .done()
          .addVariant()
            .withCulture(swedishCulture)
            .withName("Barn")
            .withSave(true)
          .done()
          .build();

        await umbracoApi.content.save(childContentNode).then(async(generatedChildContent) => {

          await configureDomain(generatedChildContent["id"], "/sv", swedishLanguageId, umbracoApi);
          const grandChildContentNode = new ContentBuilder()
            .withContentTypeAlias(generatedRootDocType["alias"])
            .withAction("saveNew")
            .withParent(generatedChildContent["id"])
            .addVariant()
              .withCulture('en-US')
              .withName(grandChildNodeName)
              .withSave(true)
            .done()
            .addVariant()
              .withCulture(swedishCulture)
              .withName("Barnbarn")
              .withSave(true)
            .done()
            .build();

          await umbracoApi.content.save(grandChildContentNode);
        });
      });
    });

    // Refresh to update the tree
    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [nodeName, childNodeName]));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    await expect(await page.locator('.umb-list')).toBeVisible();
    await page.locator('.checkbox').last().click();
    await page.locator('.btn-success').last().click()

    await umbracoUi.clickMultiple(page.locator('.alert-success > .close'));
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [nodeName, childNodeName, grandChildNodeName]));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    await expect(await page.locator('.umb-list')).toBeVisible();
    await page.locator('.checkbox').last().click();
    await page.locator('.btn-success').last().click()
    // Assert
    await expect(await umbracoUi.getSuccessNotification()).toHaveCount(2);
  })
});
