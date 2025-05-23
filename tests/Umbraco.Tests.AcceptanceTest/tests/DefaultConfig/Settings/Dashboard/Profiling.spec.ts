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
  // TODO: We need to wait a bit to make sure the page is loaded after we have reloaded the page, otherwise it can be flaky and it might not find the toggle
  await umbracoUi.waitForTimeout(500);

  // Assert
  await umbracoUi.profiling.isActivateProfilerByDefaultToggleChecked(true);
});
