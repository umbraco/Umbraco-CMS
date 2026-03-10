import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const customDataTypeName = 'Custom Content Picker Dynamic Root';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can configure content root as dynamic root origin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickDefineDynamicRootButton();
  await umbracoUi.dataType.chooseDynamicRootOrigin('Content Root');
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesContentPickerHaveDynamicRoot(customDataTypeName, 'ContentRoot')).toBeTruthy();
});

test('can change dynamic root origin from root to content root', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.dataType.goToDataType(customDataTypeName);
  await umbracoUi.dataType.clickDefineDynamicRootButton();
  await umbracoUi.dataType.chooseDynamicRootOrigin('Root');

  // Act
  await umbracoUi.dataType.clickEditDynamicRootOriginButton('Root');
  await umbracoUi.dataType.chooseDynamicRootOrigin('Content Root');
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesContentPickerHaveDynamicRoot(customDataTypeName, 'ContentRoot')).toBeTruthy();
});

test('can add a query step to content root dynamic root', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);
  await umbracoUi.dataType.clickDefineDynamicRootButton();
  await umbracoUi.dataType.chooseDynamicRootOrigin('Content Root');

  // Act
  await umbracoUi.dataType.clickAddDynamicRootQueryStepButton();
  await umbracoUi.dataType.chooseDynamicRootQueryStep('Nearest Ancestor Or Self');
  // Select the document type in the picker
  await umbracoUi.dataType.clickTextButtonWithName(documentTypeName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  const dynamicRoot = await umbracoApi.dataType.getContentPickerDynamicRoot(customDataTypeName);
  expect(dynamicRoot).toBeTruthy();
  expect(dynamicRoot.originAlias).toBe('ContentRoot');
  expect(dynamicRoot.querySteps).toBeTruthy();
  expect(dynamicRoot.querySteps.length).toBe(1);
  expect(dynamicRoot.querySteps[0].alias).toBe('NearestAncestorOrSelf');

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see content root origin in the origin picker modal', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickDefineDynamicRootButton();

  // Assert
  await umbracoUi.dataType.isDynamicRootOriginPickerModalVisible();
  await umbracoUi.dataType.isDynamicRootOriginInPickerModal('Content Root', 'Root of the content tree.');
  await umbracoUi.dataType.isDynamicRootOriginInPickerModal('Root');
  await umbracoUi.dataType.isDynamicRootOriginInPickerModal('Parent');
  await umbracoUi.dataType.isDynamicRootOriginInPickerModal('Current');
  await umbracoUi.dataType.isDynamicRootOriginInPickerModal('Site');
  await umbracoUi.dataType.isDynamicRootOriginInPickerModal('Specific Node');
});
