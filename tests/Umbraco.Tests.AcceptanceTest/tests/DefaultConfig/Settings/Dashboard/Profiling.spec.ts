import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

test('can update value of activate the profiler by default', async ({umbracoApi, umbracoUi}) => {
  test.skip(!await umbracoApi.server.isDebugMode(), 'The profiler can only be activated when the server runs in debug mode');

  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.profiling.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.profiling.clickProfilingTab();

  // Act
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(false);
  await umbracoUi.profiling.clickActivateProfilerByDefaultToggleAndWaitForUpdate();

  // Assert
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(true);
  await umbracoUi.profiling.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.profiling.clickProfilingTab();
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(true);
});

test('cannot activate the profiler when not in debug mode', async ({umbracoApi, umbracoUi}) => {
  test.skip(await umbracoApi.server.isDebugMode(), 'The profiler can be activated when the server runs in debug mode');

  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.profiling.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.profiling.clickProfilingTab();

  // Assert
  await umbracoUi.profiling.isNotInDebugModeMessageVisible();
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleVisible(false);
});
