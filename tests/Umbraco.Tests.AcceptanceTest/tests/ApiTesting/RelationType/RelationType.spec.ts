import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Relation type tests', () => {
  const relationTypeName = 'Test Relation Type';
  const objectTypeName = 'Document';
  let relationTypeId = '';
  let objectTypeId = '';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.relationType.ensureNameNotExists(relationTypeName);
    objectTypeId = await umbracoApi.objectTypes.getIdByName(objectTypeName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.relationType.ensureNameNotExists(relationTypeName);
  });

  // Skip this test because there is currently no endpoint available for creating a relation type anymore.
  test.skip('can create a relation type', async ({umbracoApi}) => {
    // Act
    relationTypeId = await umbracoApi.relationType.create(relationTypeName, false, false, objectTypeId, objectTypeId);

    // Assert
    expect(await umbracoApi.relationType.doesExist(relationTypeId)).toBeTruthy();
  });

  // Skip this test because there is currently no endpoint available for updating a relation type anymore.
  test.skip('can update a relation type', async ({umbracoApi}) => {
    // Arrange
    const wrongRelationTypeName = 'Updated Relation Type';
    relationTypeId = await umbracoApi.relationType.create(wrongRelationTypeName, false, false, objectTypeId, objectTypeId);
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

  // Skip this test because there is currently no endpoint available for deleting a relation type anymore.
  test.skip('can delete a relation type', async ({umbracoApi}) => {
    // Arrange
    relationTypeId = await umbracoApi.relationType.create(relationTypeName, false, false, objectTypeId, objectTypeId);
    expect(await umbracoApi.relationType.doesExist(relationTypeId)).toBeTruthy();

    //Act
    await umbracoApi.relationType.delete(relationTypeId);

    // Assert
    expect(await umbracoApi.relationType.doesExist(relationTypeId)).toBeFalsy();
  });
});
