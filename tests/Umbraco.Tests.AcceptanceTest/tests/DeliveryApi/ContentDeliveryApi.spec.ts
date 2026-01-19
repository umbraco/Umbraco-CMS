import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Document
const publishInvariantRootContent = 'Publish Invariant Root';
const publishInvariantParentContent = 'Publish Invariant Parent';
const childPublishInvariantContentPrefix = 'Child Publish Invariant';
const publishInvariantContentWithTextstring = 'Publish Invariant With Textstring';
const publishInvariantContentWithNumeric = 'Publish Invariant With Numeric';
const publishInvariantContentWithTrueFalse = 'Publish Invariant With True/False';
const publishInvariantContentWithMultiUrlPicker = 'Publish Invariant With Multi URL Picker';
const unpublishInvariantRootContent = 'Unpublish Invariant Root';
const publishVariantRootContent = 'Publish Variant Root';
const publishInvariantContentWithSpecialCharacters = 'Test Content, . ! ? # $ % & * @ é ü ă đ 漢字';
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
// Others
const childContentAmount = 5;

let templateId: string | null = null;
let invariantDocTypeId = '';
let collectionDocTypeId = '';
let publishInvariantParentId = '';

test.beforeEach(async ({umbracoApi}) => {
  // Create a template
  const createdTemplateId = await umbracoApi.template.createDefaultTemplate(templateName);
  templateId = createdTemplateId ?? null;
  // Create 2 languages (da, vi)
  await umbracoApi.language.createDanishLanguage();
  await umbracoApi.language.createVietnameseLanguage();
  // Get data type
  const listViewDataType = await umbracoApi.dataType.getByName(listViewContentTypeName);
  // Create document types
  invariantDocTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(invariantDocumentType) ?? '';
  collectionDocTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentType, invariantDocTypeId, listViewDataType.id) ?? '';
  // Create document
  publishInvariantParentId = await umbracoApi.document.createDefaultDocument(publishInvariantParentContent, collectionDocTypeId) ?? '';
  await umbracoApi.document.publish(publishInvariantParentId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(publishInvariantParentContent);
  await umbracoApi.documentType.ensureNameNotExists(invariantDocumentType);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentType);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.language.ensureIsoCodeNotExists('vi');
});

test.describe('fetch a content item by id', () => {
  test('can fetch an content item at root', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantRootContent;
    const publishInvariantRootId = await umbracoApi.document.createDefaultDocument(contentName, invariantDocTypeId);
    await umbracoApi.document.publish(publishInvariantRootId);
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test('can fetch a child content item', async ({umbracoApi}) => {
    // Arrange
    const contentName = childPublishInvariantContentPrefix + '2';
    const childId = await umbracoApi.document.createDefaultDocumentWithParent(contentName, invariantDocTypeId, publishInvariantParentId);
    await umbracoApi.document.publish(childId);
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test('can fetch a content item that has children', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantParentContent;
    // Create child items
    for (let i = 1; i <= childContentAmount; i++) {
      const childId = await umbracoApi.document.createDefaultDocumentWithParent(childPublishInvariantContentPrefix + i, invariantDocTypeId, publishInvariantParentId);
      await umbracoApi.document.publish(childId);
    }
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    for (let i = 1; i <= childContentAmount; i++) {
      await umbracoApi.document.ensureNameNotExists(childPublishInvariantContentPrefix + i);
    }
  });

  test('cannot fetch an unpublished content item without preview', async ({umbracoApi}) => {
    // Arrange
    const contentName = unpublishInvariantRootContent;
    await umbracoApi.document.createDefaultDocument(contentName, invariantDocTypeId);
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(404);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test('can fetch a content item with textstring property', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantContentWithTextstring;
    const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, textPropertyValue, textstringDataType.id, templateId!, textstringDataTypeName, invariantDocumentTypeWithTextString);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentId);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithTextString);
  });

  test('can fetch a content item with true/false property', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantContentWithTrueFalse;
    const trueFalseDataType = await umbracoApi.dataType.getByName(trueFalseDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, trueFalsePropertyValue, trueFalseDataType.id, templateId!, 'Test TrueFalse', invariantDocumentTypeWithTrueFalse);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentId);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithTrueFalse);
  });

  test('can fetch a content item with numeric property', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantContentWithNumeric;
    const numericDataType = await umbracoApi.dataType.getByName(numericDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, numericPropertyValue, numericDataType.id, templateId!, numericDataTypeName, invariantDocumentTypeWithNumeric);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentId);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithNumeric);
  });

  test('can fetch a content item with multi URL picker property', async ({umbracoApi}) => {
    // Arrange
    // Create child content 
    const pickerName = childPublishInvariantContentPrefix + '2';
    const pickerId = await umbracoApi.document.createDefaultDocumentWithParent(pickerName, invariantDocTypeId, publishInvariantParentId);
    await umbracoApi.document.publish(pickerId);
    const pickerNameData = await umbracoApi.document.getByName(pickerName);

    const contentName = publishInvariantContentWithMultiUrlPicker;
    const multiUrlPickerDataType = await umbracoApi.dataType.getByName(multiUrlPickerDataTypeName);
    const contentId = await umbracoApi.document.createPublishedDocumentWithDocumentLinkURLPicker(contentName, pickerName, pickerNameData.id, multiUrlPickerDataType.id, templateId!, multiUrlPickerDataTypeName, invariantDocumentTypeWithMultiUrlPicker);

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
    await umbracoApi.document.ensureNameNotExists(pickerName);
    await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeWithMultiUrlPicker);
  });

  test('can fetch a variant content item', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishVariantRootContent;
    const textStringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const variantDocTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(variantDocumentTypeWithTextstring, textstringDataTypeName, textStringDataType.id);
    const publishVariantRootId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, variantDocTypeId, textPropertyValue, textstringDataTypeName);
    await umbracoApi.document.publishDocumentWithCulture(publishVariantRootId, 'en-US');
    const contentData = await umbracoApi.document.getByName(contentName);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentData.id);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson, true);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(variantDocumentTypeWithTextstring);
  });

  test('returns 404 when fetching a non-existent content item', async ({umbracoApi}) => {
    // Arrange
    const nonExistentContentId = '00000000-0000-0000-0000-000000000000';

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(nonExistentContentId);

    // Assert
    expect(contentItem.status()).toBe(404);
  });
});

test.describe('fetch a content item by route', () => {
  test('can fetch a content item', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantRootContent;
    const publishInvariantRootId = await umbracoApi.document.createDefaultDocument(contentName, invariantDocTypeId);
    await umbracoApi.document.publish(publishInvariantRootId);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithRoute(contentURL);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test('can fetch a child content item', async ({umbracoApi}) => {
    // Arrange
    const contentName = childPublishInvariantContentPrefix + '1';
    const childId = await umbracoApi.document.createDefaultDocumentWithParent(contentName, invariantDocTypeId, publishInvariantParentId);
    await umbracoApi.document.publish(childId);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentURL);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test('can fetch a content item with a special character route', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantContentWithSpecialCharacters;
    const publishInvariantContentWithSpecialCharactersId = await umbracoApi.document.createDefaultDocument(contentName, invariantDocTypeId);
    await umbracoApi.document.publish(publishInvariantContentWithSpecialCharactersId);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);
    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(contentURL);

    // Assert
    expect(contentItem.status()).toBe(200);
    const contentItemJson = await contentItem.json();
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemJson);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemJson);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test('returns 404 when fetching a non-existent content route', async ({umbracoApi}) => {
    // Arrange
    const nonExistentContentPath = '/non-existent-path';

    // Act
    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithRoute(nonExistentContentPath);

    // Assert
    expect(contentItem.status()).toBe(404);
  });
});

test.describe('fetch content items by ids', () => {
  test('can fetch multiple content items', async ({umbracoApi}) => {
    // Arrange
    const contentName1 = publishInvariantRootContent;
    const publishInvariantRootId = await umbracoApi.document.createDefaultDocument(contentName1, invariantDocTypeId);
    await umbracoApi.document.publish(publishInvariantRootId);

    const contentName2 = childPublishInvariantContentPrefix + '3';
    const childId = await umbracoApi.document.createDefaultDocumentWithParent(contentName2, invariantDocTypeId, publishInvariantParentId);
    await umbracoApi.document.publish(childId);

    const contentData1 = await umbracoApi.document.getByName(contentName1);
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
      await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentNames[i], contentItemsJson[i]);
      await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentNames[i], contentItemsJson[i]);
    }

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName2);
    await umbracoApi.document.ensureNameNotExists(contentName1);
  });

  test('returns only valid content items when some ids are invalid', async ({umbracoApi}) => {
    // Arrange
    const contentName = childPublishInvariantContentPrefix + '4';
    const childId = await umbracoApi.document.createDefaultDocumentWithParent(contentName, invariantDocTypeId, publishInvariantParentId);
    await umbracoApi.document.publish(childId);

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
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemsJson[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemsJson[0]);

    // Clean
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test('returns only published content items when some ids are unpublished', async ({umbracoApi}) => {
    // Arrange
    const contentName = publishInvariantRootContent;
    const publishInvariantRootId = await umbracoApi.document.createDefaultDocument(contentName, invariantDocTypeId);
    await umbracoApi.document.publish(publishInvariantRootId);

    const unpublishContent = unpublishInvariantRootContent;
    await umbracoApi.document.createDefaultDocument(unpublishContent, invariantDocTypeId);

    const contentData = await umbracoApi.document.getByName(contentName);
    const unpublishContentData = await umbracoApi.document.getByName(unpublishContent);
    const contentIds = [contentData.id, unpublishContentData.id];

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsWithIds(contentIds);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.length).toBe(1);
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(contentName, contentItemsJson[0]);
    await umbracoApi.contentDeliveryApi.verifyEditorialPropertiesForInvariantContentItem(contentName, contentItemsJson[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(contentName, contentItemsJson[0]);

    // Clean
    await umbracoApi.document.ensureNameNotExists(unpublishContent);
    await umbracoApi.document.ensureNameNotExists(contentName);
  });
});

test.describe('fetch content items from a query', () => {
  test('can fetch children of a content', async ({umbracoApi}) => {
    // Arrange
    const parentContentName = publishInvariantParentContent;
    // Create child items
    for (let i = 1; i <= childContentAmount; i++) {
      const childId = await umbracoApi.document.createDefaultDocumentWithParent(childPublishInvariantContentPrefix + i, invariantDocTypeId, publishInvariantParentId);
      await umbracoApi.document.publish(childId);
    }
    await umbracoApi.page.waitForTimeout(ConstantHelper.wait.medium); // Wait is needed to ensure content is indexed
    const parentContentData = await umbracoApi.document.getByName(parentContentName);
    const parentContentId = parentContentData.id;
    const fetch = 'children:' + parentContentId;
    const totalChildrenItems = await umbracoApi.document.getChildrenAmount(parentContentId);

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, fetch);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(totalChildrenItems);

    // Clean
    for (let i = 1; i <= childContentAmount; i++) {
      await umbracoApi.document.ensureNameNotExists(childPublishInvariantContentPrefix + i);
    }
  });

  test('can fetch ancestors of a content item', async ({umbracoApi}) => {
    // Arrange
    const expectedParentContentName = publishInvariantParentContent;
    const childContentName = childPublishInvariantContentPrefix + '1';
    const childId = await umbracoApi.document.createDefaultDocumentWithParent(childContentName, invariantDocTypeId, publishInvariantParentId);
    await umbracoApi.document.publish(childId);
    await umbracoApi.page.waitForTimeout(ConstantHelper.wait.medium); // Wait is needed to ensure content is indexed
    const fetch = 'ancestors:' + childId;

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, fetch);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(1);
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(expectedParentContentName, contentItemsJson.items[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(expectedParentContentName, contentItemsJson.items[0]);

    // Clean
    await umbracoApi.document.ensureNameNotExists(childContentName);
  });

  test('can filter content items', async ({umbracoApi}) => {
    // Arrange
    const expectedContentName = publishInvariantParentContent;
    const contentType = collectionDocumentType;
    const contentTypeData = await umbracoApi.documentType.getByName(contentType);
    const filter = 'contentType:' + contentTypeData.alias;
    await umbracoApi.page.waitForTimeout(ConstantHelper.wait.medium); // Wait is needed to ensure content is indexed

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(1);
    await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(expectedContentName, contentItemsJson.items[0]);
    await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(expectedContentName, contentItemsJson.items[0]);
  });

  test('can sort content items', async ({umbracoApi}) => {
    // Arrange
    // Create child items
    for (let i = 0; i < childContentAmount; i++) {
      const childId = await umbracoApi.document.createDefaultDocumentWithParent(childPublishInvariantContentPrefix + i, invariantDocTypeId, publishInvariantParentId);
      await umbracoApi.document.publish(childId);
    }
    await umbracoApi.page.waitForTimeout(ConstantHelper.wait.medium); // Wait is needed to ensure content is indexed
    const sort = 'name:desc';
    const filter = 'name:' + 'Child';

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(childContentAmount);
    for (let i = 0; i < childContentAmount; i++) {
      expect(contentItemsJson.items[i].name).toBe(childPublishInvariantContentPrefix + (childContentAmount - 1 - i));
      await umbracoApi.contentDeliveryApi.verifyBasicPropertiesForContentItem(childPublishInvariantContentPrefix + i, contentItemsJson.items[childContentAmount - 1 - i]);
      await umbracoApi.contentDeliveryApi.verifyCulturePropertyForContentItem(childPublishInvariantContentPrefix + i, contentItemsJson.items[childContentAmount - 1 - i]);
    }

    // Clean
    for (let i = 0; i < childContentAmount; i++) {
      await umbracoApi.document.ensureNameNotExists(childPublishInvariantContentPrefix + i);
    }
  });

  test('can paginate content items', async ({umbracoApi}) => {
    // Arrange
    // Create child items
    for (let i = 1; i <= childContentAmount; i++) {
      const childId = await umbracoApi.document.createDefaultDocumentWithParent(childPublishInvariantContentPrefix + i, invariantDocTypeId, publishInvariantParentId);
      await umbracoApi.document.publish(childId);
    }
    await umbracoApi.page.waitForTimeout(ConstantHelper.wait.medium); // Wait is needed to ensure content is indexed
    const sort = 'name:asc';
    const filter = 'name:' + 'Child';
    const skip = 0;
    const take = childContentAmount - 2;

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, filter, sort, skip, take);

    // Assert
    expect(contentItems.status()).toBe(200);
    const contentItemsJson = await contentItems.json();
    expect(contentItemsJson.total).toBe(childContentAmount);
    expect(contentItemsJson.items.length).toBe(take);

    // Clean
    for (let i = 1; i <= childContentAmount; i++) {
      await umbracoApi.document.ensureNameNotExists(childPublishInvariantContentPrefix + i);
    }
  });

  test('returns 400 when using an invalid sort parameter', async ({umbracoApi}) => {
    // Arrange
    const invalidSort = 'invalidSort';

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, undefined, invalidSort);

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
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, undefined, invalidFilter);

    // Assert
    expect(contentItems).not.toBeNull();
    if (contentItems !== null) {
      expect(contentItems.status()).toBe(400);
    }
  });

  test('returns 400 when using an invalid fetch type', async ({umbracoApi}) => {
    // Arrange
    const invalidFetch = 'invalid';

    // Act
    const contentItems = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery(undefined, invalidFetch);

    // Assert
    expect(contentItems).not.toBeNull();
    if (contentItems !== null) {
      expect(contentItems.status()).toBe(400);
    }
  });
});