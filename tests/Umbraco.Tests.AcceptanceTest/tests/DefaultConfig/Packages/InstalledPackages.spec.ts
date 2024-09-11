import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// We can't install any packages so we do not have any installed.
//TODO: Remove skip when the frontend is ready
test.skip('can see no package have been installed', async ({page, umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);

  // Act
  await umbracoUi.package.clickInstalledTab();

  // Assert
  await umbracoUi.package.isTextNoPackagesHaveBeenInstalledVisible();
});
