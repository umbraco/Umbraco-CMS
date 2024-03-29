import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Profiling Dashboard tests', () => {

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.profiling.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.profiling.clickProfilingTab();
  });

  // TODO: Remove .skip when frontend is ready. Currently the value of active the profiler by default is not updated after reloading the page
  test.skip('can update value of activate the profiler by default', async ({umbracoUi}) => {
    // Act
    await umbracoUi.profiling.clickActivateProfilerByDefaultSlider();
    await umbracoUi.reloadPage();

    // Assert
    await umbracoUi.profiling.isActivateProfilerByDefaultSliderChecked(true);
  });
});
