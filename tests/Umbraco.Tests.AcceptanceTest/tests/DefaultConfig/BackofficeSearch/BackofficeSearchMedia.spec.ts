import {test} from '@umbraco/acceptance-test-helpers';

const mediaName = 'BackofficeSearchMedia';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can find a media item by name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.createDefaultMediaFile(mediaName);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.clickSearchProvider('Media');
  await umbracoUi.backofficeSearch.searchForMedia(mediaName);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(mediaName);
});
