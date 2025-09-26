import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import { expect } from '@playwright/test';

//Dashboard
const dashboardName = 'Welcome Dashboard';

test('custom dashboard is visible in content section', async ({page, umbracoUi}) => {
    // Arrange 
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    const dashboardTab = page.locator(`uui-tab[label="${dashboardName}"]`);

    // Assert
    await expect(dashboardTab).toBeVisible();
});


test('custom dashboard is not visible in media section', async ({page, umbracoUi}) => {
    // Arrange 
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.media);

    // Act
    const dashboardTab = page.locator(`uui-tab[label="${dashboardName}"]`);

    // Assert
    await expect(dashboardTab).not.toBeVisible();
});