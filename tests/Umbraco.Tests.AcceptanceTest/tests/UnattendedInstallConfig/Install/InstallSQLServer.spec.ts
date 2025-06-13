// To be able to test different databases, we need to set an additional UnattendedInstallConfig up because we would have to start from scratch, otherwise we would be using the same database.
import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const name = 'TestName';
const email = process.env.UMBRACO_USER_LOGIN;
const password = process.env.UMBRACO_USER_PASSWORD;

test('Install Umbraco using SQLServer Express', async ({page, umbracoUi}) => {
  test.slow();
  // Arrange
  await page.goto(process.env.URL + '/umbraco/install');

  // Act
  await page.getByLabel('name').fill(name);
  await page.getByLabel('email').fill(email);
  await page.getByLabel('password', {exact: true}).fill(password);
  await page.getByLabel('Next').click();
  await page.getByLabel('Next').click();
  await page.locator('#database-type').locator('#native').selectOption('SQL Server Express LocalDB');
  await expect(page.locator('#database-type').locator('option:checked')).toHaveText('SQL Server Express LocalDB')
  await page.getByLabel('Install').click();

  // Assert
  await expect(page.locator('[name="username"]')).toBeVisible({timeout: 60000});
  await page.locator('[name="username"]').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByLabel('Login').click();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});
