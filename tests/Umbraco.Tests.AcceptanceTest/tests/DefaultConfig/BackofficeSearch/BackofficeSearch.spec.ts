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
  await umbracoUi.backofficeSearch.clickOutsideToCloseModal();
  await umbracoUi.backofficeSearch.isSearchModalVisible(false);
});

const sectionDefaultProvider = [
  {section: ConstantHelper.sections.content,    provider: 'Documents'},
  {section: ConstantHelper.sections.media,      provider: 'Media'},
  {section: ConstantHelper.sections.members,    provider: 'Members'},
  {section: ConstantHelper.sections.library,    provider: 'Elements'},
  {section: ConstantHelper.sections.settings,   provider: 'Document Types'},
  {section: ConstantHelper.sections.dictionary, provider: 'Dictionary'},
];

for (const {section, provider} of sectionDefaultProvider) {
  test(`defaults to the ${provider} provider when opened from the ${section} section`, async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.backofficeSearch.goToSection(section);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();

    // Assert
    await umbracoUi.backofficeSearch.isSearchProviderActive(provider);
  });
}

test('can see the no results message when nothing matches', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForDocument('zzz-no-such-content-can-exist-zzz');

  // Assert
  await umbracoUi.backofficeSearch.isNoResultsMessageVisible();
  await umbracoUi.backofficeSearch.doesSearchResultHaveCount(0);
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
  await umbracoUi.backofficeSearch.doesSearchResultHaveCount(0);
});
