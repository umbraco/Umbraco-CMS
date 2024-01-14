import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";
import {DocumentTypeBuilder, DocumentBuilder} from "@umbraco/json-models-builders";

test.describe('Created packages tests', () => {

  const packageName = 'TestPackage';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.package.ensureNameNotExists(packageName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.package.goToSection(ConstantHelper.sections.packages);
  });

  test('can create a empty package', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.package.ensureNameNotExists(packageName);

    // Act
    await page.getByRole("tab", {name: 'Created'}).click({force: true});
    // await page.getByLabel('Created').click({force:true});
    await page.getByLabel('Create package').click();
    await page.getByLabel('Name of the package').fill(packageName);
    await umbracoUi.package.clickSaveButton();

    // Assert
    await expect(page.getByRole('button', {name: 'TestPackage'})).toBeVisible();

    // Clean
    await umbracoApi.package.ensureNameNotExists(packageName);
  });

  test('can create a package with content', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeName = 'TestDocumentType';
    const documentTypeAlias = AliasHelper.toAlias(documentTypeName);
    const documentName = 'TestDocument';
    console.log(await umbracoApi.documentType.ensureNameNotExists(documentTypeName));
    await umbracoApi.package.createEmptyPackage(packageName);
    const documentType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(documentTypeAlias)
      .withAllowedAsRoot(true)
      .build();
    const documentTypeId = await umbracoApi.documentType.create(documentType);

    const document = new DocumentBuilder()
      .withContentTypeId(documentTypeId)
      .addVariant()
      .withName(documentName)
      .done()
      .build();
    const documentId = await umbracoApi.document.create(document);

    // Act
    await page.getByRole("tab", {name: 'Created'}).click({force: true});
    await page.getByRole('button', {name: packageName}).click();
    await page.getByLabel('Add').click();
    await page.locator('#caret-button').click();
    await page.getByLabel(documentName).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save changes to package').click();

    // Assert
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.contentNodeId == documentId).toBeTruthy();

    await page.getByRole('button', {name: packageName}).click();
    await expect(page.getByRole('button', {name: documentName + ' ' + documentId})).toBeVisible();

    await page.pause();

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.package.ensureNameNotExists(packageName);
  });

  test('can create a package with media', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can create a package with document types', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can create a package with media types', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can create a package with languages', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const languageId = await umbracoApi.language.createDefaultDanishLanguage();
    await umbracoApi.package.create(packageName);

    await page.getByRole("tab", {name: 'Created'}).click({force: true});


    await page.pause();

    await umbracoApi.language.delete(languageId);


  });
  test('can create a package with dictionary', async ({page, umbracoApi, umbracoUi}) => {

  });
  test('can create a package with data types', async ({page, umbracoApi, umbracoUi}) => {

  });
  test('can create a package with templates', async ({page, umbracoApi, umbracoUi}) => {

  });
  test('can create a package with stylesheets', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can create a package with scripts', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can create a package with partial views', async ({page, umbracoApi, umbracoUi}) => {

  });
});
