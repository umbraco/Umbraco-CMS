import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('Installed packages tests', () => {

  // We can't install any packages so we do not have any installed.
  test('can see no package have been installed', async ({page, umbracoUi}) => {
    // Arrange
    await umbracoUi.goToBackOffice();
    await umbracoUi.package.goToSection(ConstantHelper.sections.packages);

    // Act
    await umbracoUi.package.clickInstalledTab();

    await page.pause();

    const testerr = await expect(page.getByText('No packages have been installed')).toBeVisible();
    const testssserr = await expect(await page.getByText('No packages have been installed')).toBeVisible();

    console.log(testerr);
    console.log(testssserr);
    // Assert
     expect(await umbracoUi.package.isTextNoPackagesHaveBeenInstalledVisible()).toBeTruthy();
  });
});
