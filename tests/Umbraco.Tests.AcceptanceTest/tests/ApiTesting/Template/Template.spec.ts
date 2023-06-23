import {AliasHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Template tests', () => {
  const templateName = 'TemplateTester';
  const templateAlias = AliasHelper.toAlias(templateName);

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.template.ensureTemplateNameNotExistsAtRoot(templateName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.template.ensureTemplateNameNotExistsAtRoot(templateName);
  })

  test('can create a template', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.template.createTemplate(templateName, templateAlias, 'Template Stuff');

    // Assert
    await expect(umbracoApi.template.doesTemplateWithNameExistAtRoot(templateName)).toBeTruthy();
  });

  test('can update a template', async ({page, umbracoApi, umbracoUi}) => {
    const newTemplateAlias = 'betterAlias';

    await umbracoApi.template.createTemplate(templateName, templateAlias, 'Template Stuff');

    const templateData = await umbracoApi.template.getTemplateByNameAtRoot(templateName);

    // Updates the template
    templateData.alias = newTemplateAlias;
    await umbracoApi.template.updateTemplateById(templateData.id, templateData);

    // Assert
    await expect(umbracoApi.template.doesTemplateWithNameExistAtRoot(templateName)).toBeTruthy();
    // Checks if the template alias was updated
    const updatedTemplate = await umbracoApi.template.getTemplateByNameAtRoot(templateName);
    await expect(updatedTemplate.alias == newTemplateAlias).toBeTruthy();
  });

  test('can delete template', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.template.createTemplate(templateName, templateAlias, 'More Template Stuff');

    await expect(umbracoApi.template.doesTemplateWithNameExistAtRoot(templateName)).toBeTruthy();

    await umbracoApi.template.deleteTemplateByNameAtRoot(templateName);

    // Assert
    await expect(await umbracoApi.template.doesTemplateWithNameExistAtRoot(templateName)).toBeFalsy();
  });
});
