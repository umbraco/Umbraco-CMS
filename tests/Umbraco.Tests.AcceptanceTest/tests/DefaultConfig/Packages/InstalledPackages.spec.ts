import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

test('can see the umbraco package is installed', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);

  // Act
  await umbracoUi.package.clickInstalledTab();

  // Assert
  await umbracoUi.package.isUmbracoBackofficePackageVisible();
});
