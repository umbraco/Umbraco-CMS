import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const parentFolderName = 'ParentFolder';
const firstChildMediaName = 'FirstChildMedia';
const secondChildMediaName = 'SecondChildMedia';
const firstRootMediaName = 'FirstRootMedia';
const secondRootMediaName = 'SecondRootMedia';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
  await umbracoApi.media.ensureNameNotExists(firstRootMediaName);
  await umbracoApi.media.ensureNameNotExists(secondRootMediaName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
  await umbracoApi.media.ensureNameNotExists(firstRootMediaName);
  await umbracoApi.media.ensureNameNotExists(secondRootMediaName);
});

test('can sort children in a media folder with drag and drop', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentId = await umbracoApi.media.createDefaultMediaFolder(parentFolderName);
  await umbracoApi.media.createDefaultMediaFolderAndParentId(firstChildMediaName, parentId);
  await umbracoApi.media.createDefaultMediaFolderAndParentId(secondChildMediaName, parentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickActionsMenuForName(parentFolderName);
  await umbracoUi.media.clickSortChildrenActionMenuOption();
  const firstChildLocator = umbracoUi.media.getTextLocatorWithName(firstChildMediaName);
  const secondChildLocator = umbracoUi.media.getTextLocatorWithName(secondChildMediaName);
  await umbracoUi.media.dragAndDrop(secondChildLocator, firstChildLocator);
  await umbracoUi.media.clickSortButton();

  // Assert
  await umbracoUi.media.openMediaCaretButtonForName(parentFolderName);
  await umbracoUi.media.doesIndexMediaInTreeContainName(parentFolderName, secondChildMediaName, 0);
  await umbracoUi.media.doesIndexMediaInTreeContainName(parentFolderName, firstChildMediaName, 1);
});

test('can sort media at root with drag and drop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.createDefaultMediaFolder(firstRootMediaName);
  await umbracoApi.media.createDefaultMediaFolder(secondRootMediaName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickActionsMenuForName(ConstantHelper.sections.media);
  await umbracoUi.media.clickSortChildrenActionMenuOption();
  const firstRootLocator = umbracoUi.media.getTextLocatorWithName(firstRootMediaName);
  const secondRootLocator = umbracoUi.media.getTextLocatorWithName(secondRootMediaName);
  await umbracoUi.media.dragAndDrop(secondRootLocator, firstRootLocator);
  await umbracoUi.media.clickSortButton();

  // Assert
  const rootNames = await umbracoApi.media.getAllAtRootNames();
  expect(rootNames.indexOf(secondRootMediaName)).toBeLessThan(rootNames.indexOf(firstRootMediaName));
});
