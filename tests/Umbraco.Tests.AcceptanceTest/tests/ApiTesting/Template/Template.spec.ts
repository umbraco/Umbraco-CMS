import {AliasHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Template tests', () => {
  let templateId = '';
  const templateName = 'TemplateTester';
  const templateAlias = AliasHelper.toAlias(templateName);

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExists(templateName);
  })

  test('can create a template', async ({umbracoApi}) => {
    // Act
    templateId = await umbracoApi.template.create(templateName, templateAlias, 'Template Stuff');

    // Assert
    expect(await umbracoApi.template.doesExist(templateId)).toBeTruthy();
  });

  test('can update a template', async ({umbracoApi}) => {
    // Arrange
    const newTemplateAlias = 'betterAlias';
    templateId = await umbracoApi.template.create(templateName, templateAlias, 'Template Stuff');
    const templateData = await umbracoApi.template.get(templateId);

    // Act
    // Updates the template
    templateData.alias = newTemplateAlias;
    await umbracoApi.template.update(templateId, templateData);

    // Assert
    expect(await umbracoApi.template.doesExist(templateId)).toBeTruthy();
    // Checks if the template alias was updated
    const updatedTemplate = await umbracoApi.template.get(templateId);
    expect(updatedTemplate.alias).toEqual(newTemplateAlias);
  });

  test('can delete template', async ({umbracoApi}) => {
    // Arrange
    templateId = await umbracoApi.template.create(templateName, templateAlias, 'More Template Stuff');
    expect(await umbracoApi.template.doesExist(templateId)).toBeTruthy();

    // Act
    await umbracoApi.template.delete(templateId);

    // Assert
    expect(await umbracoApi.template.doesExist(templateId)).toBeFalsy();
  });
});
