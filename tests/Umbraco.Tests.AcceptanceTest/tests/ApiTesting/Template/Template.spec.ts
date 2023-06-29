import {AliasHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Template tests', () => {
  let templateId = "";
  const templateName = 'TemplateTester';
  const templateAlias = AliasHelper.toAlias(templateName);

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExistsAtRoot(templateName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.template.delete(templateId);
  })

  test('can create a template', async ({page, umbracoApi, umbracoUi}) => {
    templateId = await umbracoApi.template.create(templateName, templateAlias, 'Template Stuff');

    // Assert
    await expect(umbracoApi.template.exists(templateId)).toBeTruthy();
  });

  test('can update a template', async ({page, umbracoApi, umbracoUi}) => {
    const newTemplateAlias = 'betterAlias';

    templateId = await umbracoApi.template.create(templateName, templateAlias, 'Template Stuff');

    const templateData = await umbracoApi.template.get(templateId);

    // Updates the template
    templateData.alias = newTemplateAlias;
    await umbracoApi.template.update(templateId, templateData);

    // Assert
    await expect(umbracoApi.template.exists(templateId)).toBeTruthy();
    // Checks if the template alias was updated
    const updatedTemplate = await umbracoApi.template.get(templateId);
    await expect(updatedTemplate.alias).toEqual(newTemplateAlias);
  });

  test('can delete template', async ({page, umbracoApi, umbracoUi}) => {
    templateId = await umbracoApi.template.create(templateName, templateAlias, 'More Template Stuff');

    await expect(umbracoApi.template.exists(templateId)).toBeTruthy();

    await umbracoApi.template.delete(templateId);

    // Assert
    await expect(await umbracoApi.template.exists(templateId)).toBeFalsy();
  });
});
