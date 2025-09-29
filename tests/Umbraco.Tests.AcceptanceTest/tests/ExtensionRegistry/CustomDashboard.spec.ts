import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

//Dashboard
const dashboardName = 'Welcome Dashboard';

test('custom dashboard is visible in content section', async ({page, umbracoUi}) => {
    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Assert
    await umbracoUi.content.isDashboardTabVisible(dashboardName, true);
});

test('custom dashboard is not visible in media section', async ({page, umbracoUi}) => {
    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.media);

    // Assert
    await umbracoUi.content.isDashboardTabVisible(dashboardName, false);
});