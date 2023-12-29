import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Relation type tests', () => {
  const relationTypeName = 'Test Relation Type';
  const relationTypeId = crypto.randomUUID();
  const objectTypeName = 'Document';
  let objectTypeId = '';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.relationType.ensureNameNotExists(relationTypeName);
    objectTypeId = await umbracoApi.objectTypes.getIdByName(objectTypeName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.relationType.ensureNameNotExists(relationTypeName);
  });

  test('can create a relation type', async ({umbracoApi}) => {
    // Act
    await umbracoApi.relationType.create(relationTypeName, relationTypeId, false, false, objectTypeId, objectTypeId);

    // Assert
    expect(await umbracoApi.relationType.doesExist(relationTypeId)).toBeTruthy();
  });

  test('can update a relation type', async ({umbracoApi}) => {
    // Arrange
    const wrongRelationTypeName = 'Updated Relation Type';
    await umbracoApi.relationType.create(wrongRelationTypeName, relationTypeId, false, false, objectTypeId, objectTypeId);
    const relationTypeData = await umbracoApi.relationType.get(relationTypeId);

    // Act
    relationTypeData.name = relationTypeName;
    await umbracoApi.relationType.update(relationTypeId, relationTypeData);

    // Assert
    expect(await umbracoApi.relationType.doesExist(relationTypeId)).toBeTruthy();
    // Checks if the relation type name was updated
    const updatedRelationType = await umbracoApi.relationType.get(relationTypeId);
    expect(updatedRelationType.name).toEqual(relationTypeName);
  });

  test('can delete a relation type', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.relationType.create(relationTypeName, relationTypeId, false, false, objectTypeId, objectTypeId);
    expect(await umbracoApi.relationType.doesExist(relationTypeId)).toBeTruthy();

    //Act
    await umbracoApi.relationType.delete(relationTypeId);

    // Assert
    expect(await umbracoApi.relationType.doesExist(relationTypeId)).toBeFalsy();
  });
});
