import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// TODO: There is currently an issue with playwright and the Iframe in the marketplace, look into this.
// The MarketPlace is a iFrame we are using from the DXP team, so it is not something we should test. This test is just checking if we have the IFrame
test.skip('can see the marketplace', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);

  // Act
  await umbracoUi.package.clickPackagesTab();

  // Assert
  await umbracoUi.package.isMarketPlaceIFrameVisible();
});
