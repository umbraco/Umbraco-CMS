import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.publishedStatus.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.publishedStatus.clickPublishedStatusTab();
});

test('can reload the memory cache', async ({umbracoUi}) => {
  // Act
  await umbracoUi.publishedStatus.clickReloadMemoryCacheButton();
  await umbracoUi.publishedStatus.clickContinueButton();

  // Assert
  await umbracoUi.publishedStatus.isSuccessButtonWithTextVisible('Reload Memory Cache');
});

test('can rebuild the database cache', async ({umbracoUi}) => {
  // Act
  await umbracoUi.publishedStatus.clickRebuildDatabaseCacheButton();
  await umbracoUi.publishedStatus.clickContinueButton();

  // Assert
  await umbracoUi.publishedStatus.isSuccessButtonWithTextVisible('Rebuild Database Cache');
});
