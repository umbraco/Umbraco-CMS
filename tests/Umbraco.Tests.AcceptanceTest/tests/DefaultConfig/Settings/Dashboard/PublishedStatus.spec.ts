import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.publishedStatus.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.publishedStatus.clickPublishedStatusTab();
});

test('can refresh published cache status', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedStatus = await umbracoApi.publishedCache.getStatus();

  // Act
  await umbracoUi.publishedStatus.clickRefreshStatusButton();
  // TODO: create a content item, and check if the ContentStore contains the content or not.

  // Assert
  await umbracoUi.publishedStatus.isSuccessButtonWithTextVisible('Refresh Status');
  await umbracoUi.publishedStatus.isPublishedCacheStatusVisible(expectedStatus);
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

test('can snapshot internal cache', async ({umbracoUi}) => {
  // Act
  await umbracoUi.publishedStatus.clickSnapshotInternalCacheButton();
  await umbracoUi.publishedStatus.clickContinueButton();

  // Assert
  await umbracoUi.publishedStatus.isSuccessButtonWithTextVisible('Snapshot Internal Cache');
});
