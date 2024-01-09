import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Package tests', () => {
  let packageId = "";
  const packageName = 'TestName';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.package.ensureNameNotExists(packageName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.package.delete(packageId);
  });

  test('can create a package', async ({page, umbracoApi, umbracoUi}) => {
    packageId = await umbracoApi.package.create(packageName);

    // Assert
    expect(await umbracoApi.package.doesExist(packageId)).toBeTruthy();
  });


  test('can update a package', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongPackageName = 'WrongName';
    await umbracoApi.package.ensureNameNotExists(wrongPackageName);

    // Act
    packageId = await umbracoApi.package.create(wrongPackageName);
    const packageData = await umbracoApi.package.get(packageId);
    packageData.name = packageName;
    await umbracoApi.package.update(packageId, packageData);

    // Assert
    const updatedPackageData = await umbracoApi.package.get(packageId);
    expect(updatedPackageData.name).toEqual(packageName);
  });


  test('can delete a package', async ({page, umbracoApi, umbracoUi}) => {
    packageId = await umbracoApi.package.create(packageName);

    // Assert
    expect(await umbracoApi.package.doesExist(packageId)).toBeTruthy();
  });

});
