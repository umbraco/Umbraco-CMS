import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {ContentBuilder, DocumentTypeBuilder, PackageBuilder} from "@umbraco/json-models-builders";

test.describe('Packages', () => {

  const packageName = "TestPackage";
  const rootDocTypeName = "Test document type";
  const nodeName = "1) Home";
  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });

  async function CreatePackage(umbracoApi, contentId) {
    const packageSave = new PackageBuilder()
      .withId(0)
      .withPackageGuid("00000000-0000-0000-0000-000000000000")
      .withContentNodeId(contentId)
      .withName(packageName)
      .build();

    await umbracoApi.packages.save(packageSave);
  }

  async function CreateSimplePackage(umbracoApi) {
    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .build();

    const generatedRootDocType = await umbracoApi.documentTypes.save(rootDocType);

    const rootDocTypeAlias = generatedRootDocType["alias"];

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(rootDocTypeAlias)
      .withAction("saveNew")
      .addVariant()
        .withName(nodeName)
        .withSave(true)
      .done()
      .build();
    const generatedContent = await umbracoApi.content.save(rootContentNode);
    await CreatePackage(umbracoApi, generatedContent.Id);
  }

  test('Creates a simple package', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.packages.ensureNameNotExists(packageName);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);

    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .build();

    const generatedRootDocType = await umbracoApi.documentTypes.save(rootDocType);
    const rootDocTypeAlias = generatedRootDocType["alias"];

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(rootDocTypeAlias)
      .withAction("saveNew")
      .addVariant()
        .withName(nodeName)
        .withSave(true)
      .done()
      .build();
    await umbracoApi.content.save(rootContentNode);

    // We have to wait for navigation to the packages section, if not it can cause the test to fail
    await Promise.all([
      page.waitForNavigation(),
      umbracoUi.goToSection(ConstantHelper.sections.packages)
    ]);
    await page.locator('[data-element="sub-view-umbCreatedPackages"]').click();
    await page.locator("button", {hasText: "Create package"}).click();

    // Fill out package creation form
    // Waits until the element package Content is visible
    await page.locator('[key="packager_packageContent"]').isVisible();
    await expect(page.locator('[label="@treeHeaders_documentTypes"]').locator('[text="' + rootDocTypeName + '"]')).toBeVisible();
    await page.locator("#headerName").type(packageName);
    await page.locator('.controls > .umb-node-preview-add').click();
    await page.locator('.umb-tree-item__label').first().click();
    await page.locator("button", {hasText: "Create"}).click();
    
    // Navigate pack to packages and Assert the file is created
    // Waits until the button download is visible
    await expect(await page.locator('[label-key="general_download"]')).toBeVisible({timeout: 60000});
    
    // Checks if the packages was created
    const doesExist = await umbracoApi.packages.doesNameExist(packageName);
    await expect(doesExist).toBe(true);
    await umbracoUi.goToSection(ConstantHelper.sections.packages);
    
    // Needs to wait until the page has loaded and the button is clickable
    await page.locator('[data-element="sub-view-umbCreatedPackages"]').isVisible();
    await page.locator('[data-element="sub-view-umbCreatedPackages"]').click();
    // Asserts that the package can be found in the table
    await expect(await page.locator('.table-hover')).toHaveCount(1);
    await expect(await page.locator('.table-hover').first()).toContainText(packageName);

    // Cleanup
    await umbracoApi.packages.ensureNameNotExists(packageName);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
  });

  test('Deletes a package', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.packages.ensureNameNotExists(packageName);

    await CreateSimplePackage(umbracoApi);

    // Navigate to create package section
    await umbracoUi.goToSection(ConstantHelper.sections.packages);
    await page.locator('[data-element="sub-view-umbCreatedPackages"]').click()

    // Selects the correct package
    await page.locator('text=' + packageName + ' Delete >> button').click();

    // Waits until the selector is visible
    await expect(page.locator('[label-key="contentTypeEditor_yesDelete"]')).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('contentTypeEditor_yesDelete'));

    // Assert
    await expect(await page.locator("tr", {hasText: packageName})).not.toBeVisible();

    // Cleanup
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
  });

  test('Download package', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.packages.ensureNameNotExists(packageName);

    await CreateSimplePackage(umbracoApi);

    // Navigate to package and download
    await umbracoUi.goToSection(ConstantHelper.sections.packages);
    await page.locator('[data-element="sub-view-umbCreatedPackages"]').click();
    await page.locator("tr", {hasText: "TestPackage"}).click();
    const [download] = await Promise.all([
      page.waitForEvent('download'),
      umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.download))
    ]);

    // Assert
    await expect(await download).not.toBeNull();
    await expect(await download.failure()).toBeNull();

    // Cleanup
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.packages.ensureNameNotExists(packageName);
  });
});
