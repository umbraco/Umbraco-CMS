import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Dashboard
const dashboardName = 'Welcome Dashboard';

test('can see the custom dashboard in content section', async ({umbracoUi}) => {
  // Act
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Assert
  await umbracoUi.content.isDashboardTabWithNameVisible(dashboardName, true);
});

test('can not see the custom dashboard in media section', async ({umbracoUi}) => {
  // Act
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.media);

  // Assert
  await umbracoUi.content.isDashboardTabWithNameVisible(dashboardName, false);
});