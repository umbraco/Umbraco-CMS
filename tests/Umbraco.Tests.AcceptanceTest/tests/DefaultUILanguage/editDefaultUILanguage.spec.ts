import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {UserBuilder} from '@umbraco/playwright-models';

test.describe('DefaultUILanguage', () => {

    test.beforeEach(async ({page, umbracoApi}) => {
        await umbracoApi.login();
    });

    test('DefaultUiLanguage da-DK', async ({page, umbracoApi, umbracoUi}) => {
        const name = "test";
        const email = "Test@email.com";
        const userGroup = ["editor"];
        const language = "da-DK";
        
        await umbracoApi.users.ensureEmailNotExits(email);

        const user = new UserBuilder()
            .withName(name)
            .withEmail(email)
            .withUserGroup(userGroup)
            .build()
        await umbracoApi.users.postCreateUser(user);

        await page.locator('[data-element="section-users"]').click();
        await page.locator('.umb-user-card__content', {hasText: name}).click();

        // Assert
        await expect(await page.locator('[value="string:' + language + '"][selected="selected"]')).toHaveCount(1);
        
        // Clean 
        await umbracoApi.users.ensureEmailNotExits(email);
    });
});

