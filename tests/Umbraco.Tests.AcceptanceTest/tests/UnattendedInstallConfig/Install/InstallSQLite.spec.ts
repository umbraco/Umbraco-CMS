// To be able to test different databases, we need to set an additional UnattendedInstallConfig up because we would have to start from scratch, otherwise we would be using the same database.
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const name = 'TestName';
const email = process.env.UMBRACO_USER_LOGIN;
const password = process.env.UMBRACO_USER_PASSWORD;

test('Install Umbraco using SQLite', async ({umbracoUi}) => {
  test.slow();
  // Arrange
  await umbracoUi.install.goToInstallPage();

  // Act
  await umbracoUi.install.enterName(name);
  await umbracoUi.install.enterEmail(email);
  await umbracoUi.install.enterPassword(password);
  await umbracoUi.install.clickNextButton();
  await umbracoUi.install.clickNextButton();
  await umbracoUi.install.doesDatabaseHaveType('SQLite');
  await umbracoUi.install.clickInstallButton();

  // Assert
  await umbracoUi.login.enterEmail(email);
  await umbracoUi.login.enterPassword(password);
  await umbracoUi.login.clickLoginButton();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});
