import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const healthCheckName = 'Imaging HMAC Secret Key';

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
      await umbracoUi.healthCheck.doesHealthCheckGroupHaveSuccessItemsCount(healthCheck.name, resultCount.success);
    }
    if (resultCount.warning > 0) {
      await umbracoUi.healthCheck.doesHealthCheckGroupHaveWarningItemsCount(healthCheck.name, resultCount.warning);
    }
    if (resultCount.error > 0) {
      await umbracoUi.healthCheck.doesHealthCheckGroupHaveErrorItemsCount(healthCheck.name, resultCount.error);
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
      await umbracoUi.healthCheck.clickHealthCheckGroupByName(healthCheckName);

      // Assert
      for (const check of healthCheckData.checks) {
        await umbracoUi.healthCheck.isHealthCheckNameVisible(check.name);
        await umbracoUi.healthCheck.isHealthCheckDescriptionVisible(check.description);
      }
      await umbracoUi.goBackPage();
    }
  }
});

test('can see the HMAC secret key health check in the Security group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const healthCheckGroupName = 'Security';
  const healthCheckData = await umbracoApi.healthCheck.getByName(healthCheckGroupName);

  // Act
  await umbracoUi.healthCheck.clickHealthCheckGroupByName(healthCheckGroupName);

  // Assert
  await umbracoUi.healthCheck.isHealthCheckNameVisible(healthCheckName);
  const hmacCheck = healthCheckData.checks.find((check: {name: string}) => check.name === healthCheckName);
  await umbracoUi.healthCheck.isHealthCheckDescriptionVisible(hmacCheck.description);
});

test('can see success status for HMAC secret key after performing checks', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.healthCheck.clickHealthCheckGroupByName('Security');
  await umbracoUi.healthCheck.clickPerformChecksButton();

  // Assert
  await umbracoUi.healthCheck.doesHealthCheckHaveResultMessage(healthCheckName, ConstantHelper.healthCheckMessages.imagingHMACSecretKeyIsConfigured);
  await umbracoUi.healthCheck.isHealthCheckReadMoreLinkVisible(healthCheckName, false);
});
