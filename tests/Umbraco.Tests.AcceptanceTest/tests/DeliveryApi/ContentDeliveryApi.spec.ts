import {expect} from '@playwright/test';
import {test} from '@umbraco/playwright-testhelpers';

// Document
const publishInvRootContent = 'Publish Invariant Root';
const publishInvParentContent = 'Publish Invariant Parent';
const childPublishInvContentPrefix = 'Child Publish Invariant';
const publishInvContentWithTextstring = 'Publish Invariant With Textstring';
const publishInvContentWithNumeric = 'Publish Invariant With Numeric';
const publishInvContentWithTrueFalse = 'Publish Invariant With True/False';
const publishInvContentWithMultiUrlPicker = 'Publish Invariant With Multi URL Picker';
const unpublishInvRootContent = 'Unpublish Invariant Root';
const publishVariantRootContent = 'Publish Variant Root';
const publishInvContentWithSpecialCharacters = 'Test Content, . ! ? # $ % & * @ é ü ă đ 漢字';
// Document Type
const invariantDocumentType = 'Invariant Document Type';
const collectionDocumentType = 'Collection Document Type';
const invariantDocumentTypeWithTextString = 'Invariant Document Type With Textstring';
const invariantDocumentTypeWithTrueFalse = 'Invariant Document Type With True False';
const invariantDocumentTypeWithNumeric = 'Invariant Document Type With Numeric';
const invariantDocumentTypeWithMultiUrlPicker = 'Invariant Document Type With Multi URL Picker Property';
const variantDocumentTypeWithTextstring = 'Variant Document Type With Textstring';
// Data Type
const textstringDataTypeName = 'Textstring';
const trueFalseDataTypeName = 'True/false';
const numericDataTypeName = 'Numeric';
const listViewContentTypeName = 'List View - Content';
const multiUrlPickerDataTypeName = 'Multi URL Picker';
// Property Values
const textPropertyValue = 'This is an invariant text string value';
const numericPropertyValue = '1234';
const trueFalsePropertyValue = true;
// Template
const templateName = 'TestTemplateForContentDeliveryApi';
let templateId = null;
// Others
const childContentAmount = 5;


test.describe('Content Delivery API tests', () => {
  // test.beforeAll(async ({umbracoApi}) => {
  //   //Create a template
  //   templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  //   // Create 3 languges (en-US, da, vi)
  //   await umbracoApi.language.createDanishLanguage();
  //   await umbracoApi.language.createVietnameseLanguage();
  //   //Get data types
  //   const listViewDataType = await umbracoApi.dataType.getByName(listViewContentTypeName);
  //   // Create document types
  //   const invariantDocTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(invariantDocumentType);
  //   const collectionDocTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentType, invariantDocTypeId, listViewDataType.id);
  //   // Create a publish invariant content item at root
  //   const publishInvRootId = await umbracoApi.document.createDefaultDocument(publishInvRootContent, invariantDocTypeId);
  //   await umbracoApi.document.publish(publishInvRootId);
  //   // Create a publish invariant parent item at root
  //   const publishInvParentId = await umbracoApi.document.createDefaultDocument(publishInvParentContent, collectionDocTypeId);;
  //   await umbracoApi.document.publish(publishInvParentId);
  //   // Create publish invariant child items
  //   for (let i = 1; i <= childContentAmount; i++) {
  //     const childId = await umbracoApi.document.createDefaultDocumentWithParent(childPublishInvContentPrefix + i, invariantDocTypeId, publishInvParentId);
  //     await umbracoApi.document.publish(childId);
  //   }
  //   // Create an unpublish invariant content item at root
  //   await umbracoApi.document.createDefaultDocument(unpublishInvRootContent, invariantDocTypeId);
  //   // Create a publish invariant content item with special characters at root
  //   const publishInvContentWithSpecialCharactersId = await umbracoApi.document.createDefaultDocument(publishInvContentWithSpecialCharacters, invariantDocTypeId);
  //   await umbracoApi.document.publish(publishInvContentWithSpecialCharactersId);
  //   // Create a publish variant content item at root
  //   const textStringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
  //   const variantDocTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(variantDocumentTypeWithTextstring, textstringDataTypeName, textStringDataType.id);
  //   const publishVariantRootId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(publishVariantRootContent, variantDocTypeId, textPropertyValue, textstringDataTypeName);
  //   await umbracoApi.document.publishDocumentWithCulture(publishVariantRootId, 'en-US');
  // });

  test.afterAll(async ({umbracoApi}) => {
    // await umbracoApi.document.ensureNameNotExists(publishInvRootContent);
    // await umbracoApi.document.ensureNameNotExists(publishInvParentContent);
    // for (let i = 1; i <= childContentAmount; i++) {
    //   await umbracoApi.document.ensureNameNotExists(childPublishInvContentPrefix + i);
    // }
    // await umbracoApi.document.ensureNameNotExists(unpublishInvRootContent);
    // await umbracoApi.documentType.ensureNameNotExists(invariantDocumentType);
    // await umbracoApi.documentType.ensureNameNotExists(collectionDocumentType);
    // await umbracoApi.template.ensureNameNotExists(templateName);
    // await umbracoApi.language.ensureIsoCodeNotExists('da');
    // await umbracoApi.language.ensureIsoCodeNotExists('vi');
  });

  // Gets a content item by id
  test('can fetch an content item at root by its ID', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvRootContent;
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);
  });

  test('can fetch a child content item by its ID', async ({umbracoApi}) => {
    // Arrange
    const contentName = childPublishInvContentPrefix + '2';
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);
  });

  test('can fetch a content item that has children by its ID', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvParentContent;
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);
  });

  test('cannot fetch an unpublished content item without preview by its ID', async ({umbracoApi}) => {
     // Arrange
    const contentName = unpublishInvRootContent;
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(404);
  });

  test('can fetch a content item with textstring property', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvContentWithTextstring;
    const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, textPropertyValue, textstringDataType.id, templateId, textstringDataTypeName, invariantDocumentTypeWithTextString);
  
    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentId);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithTextString);
  });

  test('can fetch a content item with true/false property', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvContentWithTrueFalse;
    const trueFalseDataType = await umbracoApi.dataType.getByName(trueFalseDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, trueFalsePropertyValue, trueFalseDataType.id, templateId, 'Test TrueFalse', invariantDocumentTypeWithTrueFalse);
  
    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentId);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithTrueFalse);
  });

  test('can fetch a content item with numeric property', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvContentWithNumeric;
    const numericDataType = await umbracoApi.dataType.getByName(numericDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, numericPropertyValue, numericDataType.id, templateId, numericDataTypeName, invariantDocumentTypeWithNumeric);
  
    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentId);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithNumeric);
  });

  test('can fetch a content item with multi URL picker property', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvContentWithMultiUrlPicker;
    const pickerName = childPublishInvContentPrefix + '2';
    const pickerNameData = await umbracoApi.document.getByName(pickerName);
    const multiUrlPickerDataType = await umbracoApi.dataType.getByName(multiUrlPickerDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithDocumentLinkURLPicker(contentName, pickerName, pickerNameData.id, multiUrlPickerDataType.id, templateId, multiUrlPickerDataTypeName, invariantDocumentTypeWithMultiUrlPicker);
  
    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentId);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItemWithMultiURLPicker(contentName, contentItemJson, 'Content');
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithMultiUrlPicker);
  });

  test('can fetch a variant content item by its ID', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishVariantRootContent;
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson, true);
  });

  test('returns 404 when fetching a non-existent content item', async ({umbracoApi}) => {
    // Arrange
    const nonExistentContentId = '00000000-0000-0000-0000-000000000000'; 

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(nonExistentContentId); 

    // Assert
    expect(contentItem.status()).toBe(404);
  });

  // Gets a content item by route
  test('can fetch a content item by its route', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvRootContent;
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithRoute(contentURL);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);
  });

  test('can fetch a child content item by its route', async ({umbracoApi}) => {
    // Arrange
    const contentName = childPublishInvContentPrefix + '1';
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentURL);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);
  });

  test('can fetch a content item by its route with special character', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvContentWithSpecialCharacters;
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentURL);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);
  });

  test('returns 404 when fetching a non-existent content route', async ({umbracoApi}) => {
    // Arrange
    const nonExistentContentPath = '/non-existent-path';

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithRoute(nonExistentContentPath);

    // Assert
    expect(contentItem.status()).toBe(404);
  });

  // Gets content item(s) by id
  test('can fetch multiple content items by their IDs', async ({umbracoApi}) => {
    // Arrange
    const contentName1 = publishInvRootContent;
    const contentData1 = await umbracoApi.document.getByName(contentName1);
    const contentName2 = childPublishInvContentPrefix + '3';
    const contentData2 = await umbracoApi.document.getByName(contentName2);
    const contentNames = [contentName1, contentName2];
    const contentIds = [contentData1.id, contentData2.id];

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsWithIds(contentIds);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.length).toBe(2);
     for (let i = 0; i < contentNames.length; i++) {
      await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentNames[i], contentItemsJson[i]);
      await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentNames[i], contentItemsJson[i]);
      await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentNames[i], contentItemsJson[i]);
    }
  });

  test('returns only valid content items when some IDs are invalid', async ({umbracoApi}) => {
    // Arrange
    const contentName = childPublishInvContentPrefix + '4';
    const contentData = await umbracoApi.document.getByName(contentName);
    const nonExistentContentId = '00000000-0000-0000-0000-000000000000';
    const contentIds = [contentData.id, nonExistentContentId];

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsWithIds(contentIds);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.length).toBe(1);
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemsJson[0]);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemsJson[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemsJson[0]);
  });

  test('returns only publish content items when some IDs are unpublish', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvRootContent;
    const contentData = await umbracoApi.document.getByName(contentName);
    const unpublishContent = unpublishInvRootContent;
    const unpublishContentData = await umbracoApi.document.getByName(unpublishContent);
    const contentIds = [contentData.id, unpublishContentData.id];

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsWithIds(contentIds);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.length).toBe(1);
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemsJson[0]);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForContentItem(contentName, contentItemsJson[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemsJson[0]);
  });

  // Gets content item(s) from a query
  test('can fetch children of a content', async ({umbracoApi}) => {
    // Arrange
    const parentContentName = publishInvParentContent;
    const parentContentData = await umbracoApi.document.getByName(parentContentName);
    const parentContentId = parentContentData.id;
    const fetch = 'children:' + parentContentId;
    const totalChildrenItems = await umbracoApi.document.getChildrenAmount(parentContentId);

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(fetch);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(totalChildrenItems);
  });

  test('can fetch ancestors of a content item', async ({umbracoApi}) => {
    // Arrange
    const childContentName = childPublishInvContentPrefix + '1';
    const expectedParentContentName = publishInvParentContent;
    const childContentData = await umbracoApi.document.getByName(childContentName);
    const childContentId = childContentData.id;
    const fetch = 'ancestors:' + childContentId;

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(fetch);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(1);
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(expectedParentContentName, contentItemsJson.items[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(expectedParentContentName, contentItemsJson.items[0]);
  });

  test('can filter content items', async ({umbracoApi}) => {
    // Arrange
    const contentType = collectionDocumentType;
    const expectedContentName = publishInvParentContent;
    const contentTypeData = await umbracoApi.documentType.getByName(contentType);
    const filter = 'contentType:' + contentTypeData.alias;

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, filter);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(1);
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(expectedContentName, contentItemsJson.items[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(expectedContentName, contentItemsJson.items[0]);
  });

  test('can sort content items', async ({umbracoApi}) => {
    // Arrange
    const sort = 'name:desc';
    const filter = 'name:' + 'Child';

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, filter, sort);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(childContentAmount);
    for (let i = 0; i < childContentAmount; i++) {
      expect(contentItemsJson.items[i].name).toBe(childPublishInvContentPrefix + ' ' + (i + 1));
      await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(childPublishInvContentPrefix + i, contentItemsJson.items[childContentAmount -i]);
      await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(childPublishInvContentPrefix + i, contentItemsJson.items[childContentAmount -i]);
    }
  });

  test('can paginate content items', async ({umbracoApi}) => {
    // Arrange
    const sort = 'name:asc';
    const filter = 'name:' + 'Child';
    const skip = 0;
    const take = childContentAmount - 1;

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, filter, sort, skip, take);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(childContentAmount);
    expect(contentItemsJson.items.length).toBe(take);
  });

  test('returns 400 when using an invalid sort parameter', async ({umbracoApi}) => {
    // Arrange
    const invalidSort= 'invalidSort';

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, invalidSort);

    // Assert
    expect(contentItems).not.toBeNull();
    if (contentItems !== null) {
      expect(contentItems.status()).toBe(400);
    }
  });

  test('returns 400 when using an invalid filter parameter', async ({umbracoApi}) => {
    // Arrange
    const invalidFilter = 'invalidFilter';

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, invalidFilter);

    // Assert
    expect(contentItems).not.toBeNull();
    if (contentItems !== null) {
      expect(contentItems.status()).toBe(400);
    }
  });

  test('returns 400 when using an invalid fetch type', async ({umbracoApi}) => {
      // Arrange
    const fetch = 'invalid';

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(fetch);

    // Assert
    expect(contentItems).not.toBeNull();
    if (contentItems !== null) {
      expect(contentItems.status()).toBe(400);
    }
  });
});
