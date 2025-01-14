import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.healthCheck.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.healthCheck.clickHealthCheckTab();
});

test('can perform all health checks', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allHealthChecksData = await umbracoApi.healthCheck.getAll();
  const healthCheckGroupCount = allHealthChecksData.total;
  expect(await umbracoUi.healthCheck.checkHealthCheckGroupCount()).toBe(healthCheckGroupCount);

  // Act
  await umbracoUi.healthCheck.clickPerformanceAllChecksButton();

  // Assert
  // verify that the number of health check groups is correct
  expect(await umbracoUi.healthCheck.checkHealthCheckGroupCount()).toBe(healthCheckGroupCount);
  // verify that the details of all health check groups are correct
  for (const healthCheck of allHealthChecksData.items) {
    await umbracoUi.healthCheck.isHealthCheckGroupVisible(healthCheck.name);
    const resultCount = await umbracoApi.healthCheck.getResultsCountByName(healthCheck.name);
    if (resultCount.success > 0) {
      await umbracoUi.healthCheck.doesHeathCheckGroupHaveSuccessItemsCount(healthCheck.name, resultCount.success);
    }
    if (resultCount.warning > 0) {
      await umbracoUi.healthCheck.doesHeathCheckGroupHaveWarningItemsCount(healthCheck.name, resultCount.warning);
    }
    if (resultCount.error > 0) {
      await umbracoUi.healthCheck.doesHeathCheckGroupHaveErrorItemsCount(healthCheck.name, resultCount.error);
    }
  }
});

test('can view the details of a health check', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allHealthChecksData = await umbracoApi.healthCheck.getAll();
  if (allHealthChecksData !== undefined) {
    for (const healthCheck of allHealthChecksData.items) {
      const healthCheckName = healthCheck.name;
      const healthCheckData = await umbracoApi.healthCheck.getByName(healthCheckName);

      // Act
      await umbracoUi.healthCheck.clickHeathCheckGroupByName(healthCheckName);

      // Assert
      for (const check of healthCheckData.checks) {
        await umbracoUi.healthCheck.isCheckNameVisible(check.name);
        await umbracoUi.healthCheck.isCheckDescriptionVisible(check.description);
      }
      await umbracoUi.goBackPage();
    }
  }
});
