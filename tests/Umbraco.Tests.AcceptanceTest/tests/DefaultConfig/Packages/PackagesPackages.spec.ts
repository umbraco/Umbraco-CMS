import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// TODO: There is currently an issue with playwright and the Iframe in the marketplace, look into this.
test.skip('can see the marketplace', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);

  // Act
  await umbracoUi.package.clickPackagesTab();

  // Assert
  await umbracoUi.waitForTimeout(1000);
  await umbracoUi.package.isMarketPlaceIFrameVisible();
});
