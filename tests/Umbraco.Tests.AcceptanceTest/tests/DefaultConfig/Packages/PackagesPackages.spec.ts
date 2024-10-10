import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// The MarketPlace is a iFrame we are using from the DXP team, so it is not something we should test. This test is just checking if we have the IFrame
test('can see the marketplace', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);

  // Act
  await umbracoUi.package.clickPackagesTab();

  // Assert
  await umbracoUi.package.isMarketPlaceIFrameVisible();
});
