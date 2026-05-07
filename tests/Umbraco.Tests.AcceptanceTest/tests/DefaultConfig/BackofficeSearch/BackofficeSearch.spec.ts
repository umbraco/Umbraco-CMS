import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test('can open and close the backoffice search modal', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();

  // Assert
  await umbracoUi.backofficeSearch.isSearchModalVisible();
  await umbracoUi.backofficeSearch.isNavigationTipsVisible();

  // Act
  await umbracoUi.backofficeSearch.clickOutsideToCloseModal();

  // Assert
  await umbracoUi.backofficeSearch.isSearchModalVisible(false);
});

test('defaults to the documents provider when opened from the content section', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();

  // Assert
  await umbracoUi.backofficeSearch.isSearchProviderActive('Documents');
});

test('can see the no results message when nothing matches', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument('zzz-no-such-content-can-exist-zzz');

  // Assert
  await umbracoUi.backofficeSearch.isNoResultsMessageVisible();
  expect(await umbracoUi.backofficeSearch.getSearchResultsCount()).toBe(0);
});

test('search input is empty when re-opening search modal', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.enterSearchQuery('anything');
  await umbracoUi.backofficeSearch.clickOutsideToCloseModal();

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();

  // Assert
  await umbracoUi.backofficeSearch.isNavigationTipsVisible();
  expect(await umbracoUi.backofficeSearch.getSearchResultsCount()).toBe(0);
});
