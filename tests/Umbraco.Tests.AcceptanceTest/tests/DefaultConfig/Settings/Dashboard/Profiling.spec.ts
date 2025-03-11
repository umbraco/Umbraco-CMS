import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.profiling.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.profiling.clickProfilingTab();
});

test('can update value of activate the profiler by default', async ({umbracoUi}) => {
  // Act
  await umbracoUi.profiling.clickActivateProfilerByDefaultToggle();
  await umbracoUi.reloadPage();

  // Assert
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(true);
});
