import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const healthCheckGroupName = 'Security';
const healthCheckName = 'Imaging HMAC Secret Key';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.healthCheck.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.healthCheck.clickHealthCheckTab();
});

test('can see the HMAC secret key health check in the Security group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const healthCheckData = await umbracoApi.healthCheck.getByName(healthCheckGroupName);

  // Act
  await umbracoUi.healthCheck.clickHealthCheckGroupByName(healthCheckGroupName);

  // Assert
  await umbracoUi.healthCheck.isHealthCheckNameVisible(healthCheckName);
  const hmacCheck = healthCheckData.checks.find((check: {name: string}) => check.name === healthCheckName);
  await umbracoUi.healthCheck.isHealthCheckDescriptionVisible(hmacCheck.description);
});

test('can see warning status for HMAC secret key after performing all checks', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.healthCheck.clickPerformanceAllChecksButton();

  // Assert
  // Verify the Security group shows warning count on the overview page
  await umbracoUi.healthCheck.isHealthCheckGroupVisible(healthCheckGroupName);
  const resultCount = await umbracoApi.healthCheck.getResultsCountByName(healthCheckGroupName);
  expect(resultCount.warning).toBeGreaterThanOrEqual(1);
  await umbracoUi.healthCheck.doesHealthCheckGroupHaveWarningItemsCount(healthCheckGroupName, resultCount.warning);
  // Navigate into the Security group to verify the warning message details
  await umbracoUi.healthCheck.clickHealthCheckGroupByName(healthCheckGroupName);
  await umbracoUi.healthCheck.doesHealthCheckHaveResultMessage(healthCheckName, ConstantHelper.healthCheckMessages.imagingHMACSecretKeyIsNotConfigured);
  // Verify the "Read more" link is displayed
  await umbracoUi.healthCheck.isHealthCheckReadMoreLinkVisible(healthCheckName);
});