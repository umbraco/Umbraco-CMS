import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test('can see the marketplace', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);

  // Act
  await umbracoUi.package.clickPackagesTab();

  // Assert
  await umbracoUi.package.isMarketPlaceIFrameVisible();
});
