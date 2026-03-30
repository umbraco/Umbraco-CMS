import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

test('can update value of activate the profiler by default', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.profiling.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.profiling.clickProfilingTab();

  // Act
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(false);
  await umbracoUi.profiling.clickActivateProfilerByDefaultToggle();

  // Assert
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(true);
  await umbracoUi.profiling.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.profiling.clickProfilingTab();
  await umbracoUi.waitForTimeout(ConstantHelper.timeout.short); // Wait for the toggle state to be updated
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(true);
});
