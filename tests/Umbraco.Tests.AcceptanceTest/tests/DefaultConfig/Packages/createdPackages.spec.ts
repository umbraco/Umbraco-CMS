import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

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
    await page.pause();

    // Clean
    await umbracoApi.package.ensureNameNotExists(packageName);
  });

  test('can create a package with content', async ({page, umbracoApi, umbracoUi}) => {

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
