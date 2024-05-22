import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.modelsBuilder.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.modelsBuilder.clickModelsBuilderTab();
});

test('can reload models builder dashboard', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const modelsBuilderData = await umbracoApi.modelsBuilder.getDashboard();
  const expectedVersion = 'Version: ' + modelsBuilderData.version;
  const expectedMode = "The ModelsMode is '" + modelsBuilderData.mode + "'";
  const expectedModelsNamespace = 'The models namespace is ' + modelsBuilderData.modelsNamespace;
  const expectedOutOfDateModels = modelsBuilderData.outOfDateModels ? 'Tracking of out-of-date models is enabled' : 'Tracking of out-of-date models is not enabled';

  // Act
  await umbracoUi.modelsBuilder.clickReloadButton();

  // Assert
  await umbracoUi.modelsBuilder.isSuccessButtonWithTextVisible('Reload');
  await umbracoUi.modelsBuilder.doesModelsBuilderDashboardHaveText(expectedVersion);
  await umbracoUi.modelsBuilder.doesModelsBuilderDashboardHaveText(expectedMode);
  await umbracoUi.modelsBuilder.doesModelsBuilderDashboardHaveText(expectedModelsNamespace);
  await umbracoUi.modelsBuilder.doesModelsBuilderDashboardHaveText(expectedOutOfDateModels);
});
