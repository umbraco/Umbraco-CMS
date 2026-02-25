import {AliasHelper} from "./AliasHelper";
import {ApiHelpers} from "./ApiHelpers";
import {DocumentBuilder, DocumentDomainBuilder} from "../builders";

export class DocumentApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id);
    return await response.json();
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id);
    return response.status() === 200;
  }

  async create(document) {
    if (document == null) {
      return;
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/document', document);
    return response.headers().location.split("v1/document/").pop();
  }

  async delete(id: string) {
    if (id == null) {
      return;
    }
    const response = await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id);
    return response.status();
  }

  async update(id: string, document) {
    if (document == null) {
      return;
    }
    const variantsData = document.variants.map(variant => ({
      culture: variant.culture,
      segment: variant.segment,
      name: variant.name
    }));
    
    const updateData = {
      values: document.values,
      variants: variantsData,
      template: document.template
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id, updateData);
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/document/root?skip=0&take=10000');
  }

  async getChildren(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/document/children?parentId=${id}&skip=0&take=10000`);
    const items = await response.json();
    return items.items;
  }

  async getChildrenAmount(id: string) {
    const response = await this.api.get(`${this.api.baseUrl}/umbraco/management/api/v1/tree/document/children?parentId=${id}&skip=0&take=10000`);
    const items = await response.json();
    return items.total;
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  private async recurseDeleteChildren(id: string) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child.id);
      } else {
        await this.delete(child.id);
      }
    }
    return await this.delete(id);
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      for (const variant of child.variants) {
        if (variant.name === name) {
          if (!toDelete) {
            return await this.get(child.id);
          }
          if (child.hasChildren) {
            return await this.recurseDeleteChildren(child.id);
          } else {
            return await this.delete(child.id);
          }
        }
      }
      if (child.hasChildren) {
        const result = await this.recurseChildren(name, child.id, toDelete);
        if (result) { 
          return result;
        }
      }
    }
    return false;
  }

  async getByName(name: string) {
    const rootDocuments = await this.getAllAtRoot();
    const jsonDocuments = await rootDocuments.json();

    for (const document of jsonDocuments.items) {
      for (const variant of document.variants) {
        if (variant.name === name) {
          return this.get(document.id);
        }
      }
      if (document.hasChildren) {
        const result = await this.recurseChildren(name, document.id, false);
        if (result) { 
          return result;
        }
      }
    }
    return false;
  }

  async ensureNameNotExists(name: string) {
    const rootDocuments = await this.getAllAtRoot();
    const jsonDocuments = await rootDocuments.json();

    for (const document of jsonDocuments.items) {
      for (const variant of document.variants) {
        if (variant.name === name) {
          if (document.hasChildren) {
            await this.recurseDeleteChildren(document.id);
          }
          await this.delete(document.id);
        } else {
          if (document.hasChildren) {
            await this.recurseChildren(name, document.id, true);
          }
        }
      }
    }
  }

  async publish(id: string, publishSchedulesData: any = {"publishSchedules":[{"culture":null}]}) {
    if (id == null) {
      return;
    }
    const response = await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id + '/publish', publishSchedulesData);
    return response.status();
  }

  async getDocumentUrl(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document/urls?id=' + id);
    const urls = await response.json();

    return urls[0].urlInfos[0].url;
  }

  async moveToRecycleBin(id: string) {
    if (id == null) {
      return;
    }
    const response = await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id + '/move-to-recycle-bin');
    return response.status();
  }

  async createDefaultDocument(documentName: string, documentTypeId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithTextContent(documentName: string, documentTypeId: string, textContent: string, dataTypeName: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(textContent)
        .done()
      .build();

    return await this.create(document);
  }

  async createDefaultDocumentWithParent(documentName: string, documentTypeId: string, parentId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .withParentId(parentId)
      .addVariant()
        .withName(documentName)
        .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithTemplate(documentName: string, documentTypeId: string, templateId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .withTemplateId(templateId)
      .build();

    return await this.create(document);
  }

  async createDocumentWithContentPicker(documentName: string, documentTypeId: string, contentPickerId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias('contentPicker')
        .withValue(contentPickerId)
        .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithOneMediaPicker(documentName: string, documentTypeId: string, mediaPickerId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias('mediaPicker')
        .addMediaPickerValue()
          .withMediaKey(mediaPickerId)
          .done()
        .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithTwoMediaPicker(documentName: string, documentTypeId: string, firstMediaPickerId: string, secondMediaPickerId: string, alias: string = 'multipleMediaPicker') {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(alias)
        .addMediaPickerValue()
          .withMediaKey(firstMediaPickerId)
          .done()
        .addMediaPickerValue()
          .withMediaKey(secondMediaPickerId)
          .done()
        .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithMemberPicker(documentName: string, documentTypeId: string, memberId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias('memberPicker')
        .withValue(memberId)
        .done()
      .build();
      
    return await this.create(document);
  }

  async createDocumentWithTags(documentName: string, documentTypeId: string, tagsName: string[]) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias('tags')
        .withValue(tagsName)
        .done()
      .build();
    
    return await this.create(document);
  }

  async createDocumentWithExternalLinkURLPicker(documentName: string, documentTypeId: string, link: string, linkTitle: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias('multiUrlPicker')
        .addURLPickerValue()
          .withIcon('icon-link')
          .withName(linkTitle)
          .withType('external')
          .withUrl(link)
          .done()
        .done()
      .build();
      
    return await this.create(document);
  }

  // Domains
  async getDomains(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id + '/domains');
    return await response.json();
  }

  async updateDomains(id: string, domains) {
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id + '/domains', domains);
  }
  
  // Image Media Picker
  async createDocumentWithImageMediaPicker(documentName: string, documentTypeId: string, propertyAlias: string, mediaKey: string, focalPoint: {left: number, top: number} = {left: 0, top: 0}) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(propertyAlias)
        .addMediaPickerValue()
          .withMediaKey(mediaKey)
          .withFocalPoint(focalPoint)
          .done()
        .done()
      .build();

    return await this.create(document);
  }
  
  async doesImageMediaPickerContainImage(id: string, propertyAlias: string, mediaKey: string) {
    const contentData = await this.getByName(id);
    return contentData.values.some(value =>
      value.alias === propertyAlias && value.value.some(item => item.mediaKey === mediaKey)
    );
  }

  async doesImageMediaPickerContainImageWithFocalPoint(id: string, propertyAlias: string, mediaKey: string, focalPoint: {left: number, top: number}) {
    const contentData = await this.getByName(id);

    if (focalPoint.left <= 0 || focalPoint.top <= 0) {
      return contentData.values.some(value => value.alias === propertyAlias && value.value.some(item => {
        return item.mediaKey === mediaKey && item.focalPoint === null;
      }));
    }

    // When selecting a focalpoint, it is not exact down to the decimal, so we need a small tolerance to account for that.
    const tolerance = 0.02;

    return contentData.values.some(value =>
        value.alias === propertyAlias && value.value.some(item => {
          // Check if the mediaKey is the same and the focalPoint is within the tolerance
          return item.mediaKey === mediaKey &&
            Math.abs(item.focalPoint.left - focalPoint.left) <= tolerance * focalPoint.left &&
            Math.abs(item.focalPoint.top - focalPoint.top) <= tolerance * focalPoint.top;
        })
    );
  }

  async createDocumentWithUploadFile(documentName: string, documentTypeId: string, dataTypeName: string, uploadFileName: string, mineType: string) {
    await this.ensureNameNotExists(documentName);
    const temporaryFile = await this.api.temporaryFile.createTemporaryFile(uploadFileName, 'File', mineType);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withTemporaryFileId(temporaryFile.temporaryFileId)
        .done()
      .build();

    return await this.create(document);
  }

  async createDefaultDocumentWithEnglishCulture(documentName: string, documentTypeId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .withCulture('en-US')
        .done()
      .build();

    return await this.create(document);
  }

  async createDefaultDocumentWithCulture(documentName: string, documentTypeId: string, isoCode: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
      .withName(documentName)
      .withCulture(isoCode)
      .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithMultipleVariants(documentName: string, documentTypeId: string, dataTypeAlias: string, cultureVariants: {isoCode: string, name: string, value: string}[]) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .build();

    for (const variant of cultureVariants) {
      document.variants.push({
        name: variant.name,
        culture: variant.isoCode,
        segment: null
      });

      document.values.push({
        alias: dataTypeAlias,
        value: variant.value,
        culture: variant.isoCode,
        segment: null,
        editorAlias: 'Umbraco.TextBox',
        entityType: 'document-property-value'
      });
    }

    return await this.create(document);
  }

  async createDocumentWithMultipleVariantsWithSharedProperty(documentName: string, documentTypeId: string, dataTypeAlias: string, dataTypeEditorAlias: string, cultureVariants: {isoCode: string, name: string}[], value) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .build();

    for (const variant of cultureVariants) {
      document.variants.push({
        name: variant.name,
        culture: variant.isoCode,
        segment: null
      });
    }

    document.values.push({
      alias: dataTypeAlias,
      value: value,
      culture: null,
      segment: null,
      editorAlias: dataTypeEditorAlias,
      entityType: 'document-property-value'
    });

    return await this.create(document);
  }

  async createDocumentWithEnglishCultureAndTextContent(documentName: string, documentTypeId: string, textContent: string, dataTypeName: string, varyByCultureForText: boolean = false) {
    await this.ensureNameNotExists(documentName);
    const cultureValue = varyByCultureForText ? 'en-US' : null;

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .withCulture('en-US')
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(textContent)
        .withCulture(cultureValue)
        .done()
      .build();

    return await this.create(document);
  }

  async createPublishedDocumentWithValue(documentName: string, value: any, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type') {
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .withValue(value)
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    const publishData = {"publishSchedules":[{"culture":null}]};
    await this.publish(documentId, publishData);
    return documentId;
  }
  
  async isDocumentPublished(id: string) {
    const document = await this.get(id);
    return document.variants[0].state === 'Published';
  }

  async createPublishedDocumentWithImageCropper(documentName: string, cropValue: any, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type', focalPoint: {left: number, top: number} = {left: 0.5, top: 0.5}) {
    // Create temporary file
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryImageFile();
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .addImageCropperValue()
          .withCrop(cropValue)
          .withFocalPoint(focalPoint)
          .withTemporaryFileId(temporaryFile.temporaryFileId)
          .done()
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    await this.publish(documentId);
    return documentId;
  }

  async createPublishedDocumentWithUploadFile(documentName: string, uploadFileName: string, mineType: string, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type') {
    // Create temporary file
    const temporaryFile = await this.api.temporaryFile.createTemporaryFile(uploadFileName, 'File', mineType);
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);   

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .withTemporaryFileId(temporaryFile.temporaryFileId)
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    await this.publish(documentId);
    return {'documentId': documentId, 'temporaryFileId': temporaryFile.temporaryFileId};
  }

  async createPublishedDocumentWithExternalLinkURLPicker(documentName: string, linkTitle: string, linkUrl: string, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type') {
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .addURLPickerValue()
          .withName(linkTitle)
          .withType('external')
          .withUrl(linkUrl)
          .done()
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    await this.publish(documentId);
    return documentId;
  }

  async createPublishedDocumentWithDocumentLinkURLPicker(documentName: string, linkedDocumentName: string, linkedDocumentId: string, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type') {
    // Get the url of the linked document
    const linkedDocumentData = await this.getByName(linkedDocumentName);
    const linkedDocumentUrl = await this.getDocumentUrl(linkedDocumentData.id);
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);   

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .addURLPickerValue()
          .withIcon('icon-document')
          .withName(linkedDocumentName)
          .withType('document')
          .withUnique(linkedDocumentId)
          .withUrl(linkedDocumentUrl)
          .done()
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    await this.publish(documentId);
    return documentId;
  }

  async createPublishedDocumentWithImageLinkURLPicker(documentName: string, imageName: string, imageId: string, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type') {
    // Get the url of the linked document
    const mediaData = await this.api.media.getByName(imageName);
    const mediaUrl = await this.api.media.getFullMediaUrl(mediaData.id);
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);   

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .addURLPickerValue()
          .withIcon('icon-picture')
          .withName(imageName)
          .withType('media')
          .withUnique(imageId)
          .withUrl(mediaUrl)
          .done()
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    await this.publish(documentId);
    return documentId;
  }

  async createPublishedDocumentWithImageLinkAndExternalLink(documentName: string, imageName: string, imageId: string, externalLinkTitle: string, externalLinkUrl: string, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type') {
    // Get the url of the linked document
    const mediaData = await this.api.media.getByName(imageName);
    const mediaUrl = await this.api.media.getFullMediaUrl(mediaData.id);
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);   

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .addURLPickerValue()
          .withIcon('icon-picture')
          .withName(imageName)
          .withType('media')
          .withUnique(imageId)
          .withUrl(mediaUrl)
          .done()
        .addURLPickerValue()
          .withName(externalLinkTitle)
          .withType('external')
          .withUrl(externalLinkUrl)
          .done()
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    await this.publish(documentId);
    return documentId;
  }

  async createPublishedDocumentWithTwoMediaPicker(documentName: string, firstMediaPickerId: string, secondMediaPickerId: string, dataTypeId: string, templateId: string, propertyName: string = 'Test Property Name', documentTypeName: string = 'Test Document Type') {
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, dataTypeId, propertyName, templateId);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(propertyName))
        .addMediaPickerValue()
          .withMediaKey(firstMediaPickerId)
          .done()
        .addMediaPickerValue()
          .withMediaKey(secondMediaPickerId)
          .done()
        .done()
      .build();

    // Create document
    let documentId = await this.create(document);
    documentId = documentId === undefined ? '' : documentId;
    // Publish document
    await this.publish(documentId);
    return documentId;
  }

  async createDefaultDocumentWithABlockGridEditor(documentName: string, elementTypeId: string, documentTypeName: string, blockGridDataTypeName: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlock(blockGridDataTypeName, elementTypeId) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();
      
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockGridEditorAndBlockWithValue(documentName: string, documentTypeName: string, blockGridDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string = 'TestGroup', templateId?: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, elementTypeId, true) || '';
    let documentTypeId: string;
    if (templateId) {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, blockGridDataTypeId, blockGridDataTypeName, templateId) || '';
    } else {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, groupName) || '';
    }
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockGridEditorAndBlockWithValueAndTwoGroups(documentName: string, documentTypeName: string, blockGridDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string = 'TestGroup', secondPropertyName: string, secondGroupName: string = 'GroupTwoName') {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, elementTypeId, true) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndTwoGroups(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, groupName, secondPropertyName, blockGridDataTypeId, secondGroupName) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();

    return await this.create(document);
  }

  async createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValues(documentName: string, documentTypeName: string, blockGridDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string = 'TestGroup', secondElementTypePropertyValue: string, templateId?: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const secondBlockContentKey = crypto.randomUUID();
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, elementTypeId, true) || '';
    let documentTypeId: string;
    if (templateId) {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, blockGridDataTypeId, blockGridDataTypeName, templateId) || '';
    } else {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, groupName) || '';
    }
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(secondBlockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(secondElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addExpose()
            .withContentKey(secondBlockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(secondBlockContentKey)
            .done()
          .done()
        .done()
      .build();
    
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValuesAndTwoGroups(documentName: string, documentTypeName: string, blockGridDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string = 'TestGroup', secondElementTypePropertyValue: string, secondPropertyName: string, secondGroupName: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const secondBlockContentKey = crypto.randomUUID();
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, elementTypeId, true) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndTwoGroups(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, groupName, secondPropertyName, blockGridDataTypeId, secondGroupName) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(secondBlockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(secondElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addExpose()
            .withContentKey(secondBlockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(secondBlockContentKey)
            .done()
          .done()
        .done()
      .build();
    
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockListEditorAndBlockWithValue(documentName: string, documentTypeName: string, blockListDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string, templateId?: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId) || '';
    let documentTypeId: string;
    if (templateId) {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, blockListDataTypeId, blockListDataTypeName, templateId) || '';
    } else {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, groupName) || '';
    }
    
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();
    
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockListEditor(documentName: string, elementTypeId: string, documentTypeName: string, blockListDataTypeName: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();
      
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockListEditorAndBlockWithValueAndTwoGroups(documentName: string, documentTypeName: string, blockListDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string = 'TestGroup', secondPropertyName: string, secondGroupName: string = 'GroupTwoName') {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndTwoGroups(documentTypeName, blockListDataTypeName, blockListDataTypeId, groupName, secondPropertyName, blockListDataTypeId, secondGroupName) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .done()
        .done()
      .build();

    return await this.create(document);
  }
  
  async createDefaultDocumentWithABlockListEditorAndBlockWithTwoValues(documentName: string, documentTypeName: string, blockListDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string = 'TestGroup', secondElementTypePropertyValue: string, templateId?: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const secondBlockContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId) || '';
    let documentTypeId: string;
    if (templateId) {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, blockListDataTypeId, blockListDataTypeName, templateId) || '';  
    } else {
      documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, groupName) || '';
    }
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(secondBlockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(secondElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addExpose()
            .withContentKey(secondBlockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(secondBlockContentKey)
            .done()
          .done()
        .done()
      .build();
    
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockListEditorAndBlockWithTwoValuesAndTwoGroups(documentName: string, documentTypeName: string, blockListDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string, groupName: string = 'TestGroup', secondElementTypePropertyValue: string, secondPropertyName: string, secondGroupName: string) {
    const crypto = require('crypto');
    const blockContentKey = crypto.randomUUID();
    const secondBlockContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndTwoGroups(documentTypeName, blockListDataTypeName, blockListDataTypeId, groupName, secondPropertyName, blockListDataTypeId, secondGroupName) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(secondBlockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(secondElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockContentKey)
            .done()
          .addExpose()
            .withContentKey(secondBlockContentKey)
            .done()
          .addLayout()
            .withContentKey(blockContentKey)
            .done()
          .addLayout()
            .withContentKey(secondBlockContentKey)
            .done()
          .done()
        .done()
      .build();
    
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockListEditorAndBlockGridEditorWithSameAllowedBlock(documentName: string, documentTypeName: string, blockListDataTypeName: string, blockGridDataTypeName: string, elementTypeId: string, elementTypePropertyAlias: string, blockListElementTypePropertyValue: string, blockListElementTypePropertyEditorAlias: string, blockListGroupName: string = 'TestBlockList', blockGridElementTypePropertyValue: string, blockGridElementTypePropertyEditorAlias: string, blockGridGroupName: string = 'TestBlockGrid') {
    const crypto = require('crypto');
    const blockListContentKey = crypto.randomUUID();
    const blockGridContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId) || '';
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, elementTypeId) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndTwoGroups(documentTypeName, blockListDataTypeName, blockListDataTypeId, blockListGroupName, blockGridDataTypeName, blockGridDataTypeId, blockGridGroupName) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockListContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(blockListElementTypePropertyEditorAlias)
              .withValue(blockListElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockListContentKey)
            .done()
          .addLayout()
            .withContentKey(blockListContentKey)
            .done()
          .done()
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(elementTypeId)
            .withKey(blockGridContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(blockGridElementTypePropertyEditorAlias)
              .withValue(blockGridElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockGridContentKey)
            .done()
          .addLayout()
            .withContentKey(blockGridContentKey)
          .done()
        .done()
      .done()
      .build();
    
    return await this.create(document);
  }

  async createDefaultDocumentWithABlockListEditorAndBlockGridEditorWithDifferentAllowedBlock(documentName: string, documentTypeName: string, blockListDataTypeName: string, blockGridDataTypeName: string, blockListElementTypeId: string, blockListElementTypePropertyAlias: string, blockListElementTypePropertyValue: string, blockListElementTypePropertyEditorAlias: string, blockListGroupName: string = 'TestBlockList', blockGridElementTypeId: string, blockGridElementTypePropertyAlias: string, blockGridElementTypePropertyValue: string, blockGridElementTypePropertyEditorAlias: string, blockGridGroupName: string = 'TestBlockGrid') {
    const crypto = require('crypto');
    const blockListContentKey = crypto.randomUUID();
    const blockGridContentKey = crypto.randomUUID();
    const blockListDataTypeId = await this.api.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, blockListElementTypeId) || '';
    const blockGridDataTypeId = await this.api.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, blockGridElementTypeId) || '';
    const documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditorAndTwoGroups(documentTypeName, blockListDataTypeName, blockListDataTypeId, blockListGroupName, blockGridDataTypeName, blockGridDataTypeId, blockGridGroupName) || '';
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockListDataTypeName))
        .addBlockListValue()
          .addContentData()
            .withContentTypeKey(blockListElementTypeId)
            .withKey(blockListContentKey)
            .addContentDataValue()
              .withAlias(blockListElementTypePropertyAlias)
              .withEditorAlias(blockListElementTypePropertyEditorAlias)
              .withValue(blockListElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockListContentKey)
            .done()
          .addLayout()
            .withContentKey(blockListContentKey)
            .done()
          .done()
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(blockGridElementTypeId)
            .withKey(blockGridContentKey)
            .addContentDataValue()
              .withAlias(blockGridElementTypePropertyAlias)
              .withEditorAlias(blockGridElementTypePropertyEditorAlias)
              .withValue(blockGridElementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(blockGridContentKey)
            .done()
          .addLayout()
            .withContentKey(blockGridContentKey)
            .done()
          .done()
        .done()
      .build();
    
    return await this.create(document);
  }
  
  async createDocumentWithABlockGridEditorWithABlockThatContainsABlockInAnArea(documentName: string, documentTypeId: string, blockGridDataTypeName: string, firstElementTypeKey: string, areaKey: string, secondElementTypeKey: string, elementTypePropertyAlias: string, elementTypePropertyValue: string, elementTypePropertyEditorAlias: string) {
    const crypto = require('crypto');
    const firstBlockContentKey = crypto.randomUUID();
    const secondBlockContentKey = crypto.randomUUID();
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(blockGridDataTypeName))
        .addBlockGridValue()
          .addContentData()
            .withContentTypeKey(firstElementTypeKey)
            .withKey(firstBlockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addContentData()
            .withContentTypeKey(secondElementTypeKey)
            .withKey(secondBlockContentKey)
            .addContentDataValue()
              .withAlias(elementTypePropertyAlias)
              .withEditorAlias(elementTypePropertyEditorAlias)
              .withValue(elementTypePropertyValue)
              .done()
            .done()
          .addExpose()
            .withContentKey(firstBlockContentKey)
            .withContentKey(secondBlockContentKey)
            .done()
          .addLayout()
            .withContentKey(firstBlockContentKey)
            .addArea()
              .withKey(areaKey)
              .addItems()
                .withContentKey(secondBlockContentKey)
                .done()
              .done()
            .done()
          .done()
        .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithTextContentAndOneDomain(documentName: string, documentTypeId: string, textContent: string, dataTypeName: string, domainName: string, isoCode: string = 'en-US') {
    const contentId = await this.createDocumentWithTextContent(documentName, documentTypeId, textContent, dataTypeName) || '';
    const domainData = new DocumentDomainBuilder()
      .addDomain()
        .withDomainName(domainName)
        .withIsoCode(isoCode)
        .done()
      .build();

    await this.updateDomains(contentId, domainData);
    return contentId;
  }

  async createDocumentWithTextContentAndTwoDomains(documentName: string, documentTypeId: string, textContent: string, dataTypeName: string, firstDomainName: string, firstIsoCode: string = 'en-US', secondDomainName: string, secondIsoCode: string = 'en-US') {
    const contentId = await this.createDocumentWithTextContent(documentName, documentTypeId, textContent, dataTypeName) || '';
    const domainData = new DocumentDomainBuilder()
      .addDomain()
        .withDomainName(firstDomainName)
        .withIsoCode(firstIsoCode)
        .done()
      .addDomain()
        .withDomainName(secondDomainName)
        .withIsoCode(secondIsoCode)
        .done()
      .build();
    
    await this.updateDomains(contentId, domainData);
    return contentId;
  }
  
  async doesTipTapDataTypeWithNameContainBlockWithValue(documentName: string, dataTypeAlias: string, elementTypeId: string, elementTypeDataTypeAlias: string, blockValue: string) {
    const document = await this.getByName(documentName);
    const tipTapDataType = document.values.find(value => value.alias === dataTypeAlias);
    const block = tipTapDataType.value.blocks.contentData.find(value => value.contentTypeKey === elementTypeId);
    const property = block.values.find(value => value.alias === elementTypeDataTypeAlias);
    return property.value === blockValue;
  }

  async publishDocumentWithCulture(id: string, culture: string) {
    const publishScheduleData = {
      "publishSchedules":[
        {
          "culture": culture
        }
      ]
    };
    
    return await this.publish(id, publishScheduleData);
  }

  async createDocumentWithTextContentAndParent(documentName: string, documentTypeId: string, textContent: string, dataTypeName: string, parentId: string) {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .withParentId(parentId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(textContent)
        .done()
      .build();

    return await this.create(document);
  }

  async createDocumentWithEnglishCultureAndTextContentAndParent(documentName: string, documentTypeId: string, textContent: string, dataTypeName: string, parentId: string, varyByCultureForText: boolean = false) {
    await this.ensureNameNotExists(documentName);
    const cultureValue = varyByCultureForText ? 'en-US' : null;

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .withParentId(parentId)
      .addVariant()
        .withName(documentName)
        .withCulture('en-US')
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(textContent)
        .withCulture(cultureValue)
        .done()
      .build();

    return await this.create(document);
  }
  
  async doesBlockGridContainBlocksWithDataElementKeyInAreaWithKey(documentName: string, blockGridAlias:string ,blockContentKey: string, areaKey: string, blocksInAreas: string[]) {
    const document = await this.getByName(documentName);
    const documentValues = document.values.find(value => value.alias === blockGridAlias);
    const parentBlock = documentValues.value.layout['Umbraco.BlockGrid'].find(value => value.contentKey ===  blockContentKey);
    const area = parentBlock.areas.find(value => value.key === areaKey);
    return area.items.map(value => value.contentKey).every(value => blocksInAreas.includes(value));
  }
  
  async emptyRecycleBin() {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/recycle-bin/document');
  }

  async getRecycleBinItems() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/recycle-bin/document/root?skip=0&take=10000');
  }

  async doesItemExistInRecycleBin(documentItemName: string) {
    const recycleBin = await this.getRecycleBinItems();
    const jsonRecycleBin = await recycleBin.json();
    for (const document of jsonRecycleBin.items) {
      if (document.variants[0].name === documentItemName) {
        return true;
      }
    }
    return false;
  }

  async createDocumentWithTwoCulturesAndTextContent(documentName: string, documentTypeId: string, textContent: string, dataTypeName: string, firstCulture: string, secondCulture: string, firstDomainName: string = '/testfirstdomain', secondDomainName: string = '/testseconddomain') {
    await this.ensureNameNotExists(documentName);

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .withCulture(firstCulture)
        .done()
      .addVariant()
        .withName(documentName)
        .withCulture(secondCulture)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withValue(textContent)
        .done()
      .build();
    const contentId = await this.create(document) || '';
    
    const domainData = new DocumentDomainBuilder()
      .addDomain()
        .withDomainName(firstDomainName)
        .withIsoCode(firstCulture)
        .done()
      .addDomain()
        .withDomainName(secondDomainName)
        .withIsoCode(secondCulture)
        .done()
      .build();
    await this.updateDomains(contentId, domainData);
    return contentId;
  }

  async createDefaultDocumentWithOneDocumentLink(documentName: string, linkedDocumentName: string, linkedDocumentId: string, documentTypeName: string = 'Test Document Type') {
    const multiURLPickerDataTypeName = 'Multi URL Picker';
    // Get the url of the linked document
    const linkedDocumentData = await this.getByName(linkedDocumentName);
    const linkedDocumentUrl = await this.getDocumentUrl(linkedDocumentData.id);
    // Get datatype
    const dataTypeData = await this.api.dataType.getByName(multiURLPickerDataTypeName);
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, multiURLPickerDataTypeName, dataTypeData.id);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);   

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(multiURLPickerDataTypeName))
        .addURLPickerValue()
          .withIcon('icon-document')
          .withName(linkedDocumentName)
          .withType('document')
          .withUnique(linkedDocumentId)
          .withUrl(linkedDocumentUrl)
          .done()
        .done()
      .build();

    // Create document
    return await this.create(document);
  }

  async createDefaultDocumentWithOneMediaLink(documentName: string, linkedMediaName: string, documentTypeName: string = 'Test Document Type') {
    const multiURLPickerDataTypeName = 'Multi URL Picker';
    // Get the url of the linked document
    const linkedMediaData = await this.api.media.getByName(linkedMediaName);
    // Get datatype
    const dataTypeData = await this.api.dataType.getByName(multiURLPickerDataTypeName);
    // Create document type
    let documentTypeId = await this.api.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, multiURLPickerDataTypeName, dataTypeData.id);
    documentTypeId = documentTypeId === undefined ? '' : documentTypeId;
    await this.ensureNameNotExists(documentName);   

    const document = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(multiURLPickerDataTypeName))
        .addURLPickerValue()
          .withIcon(linkedMediaData.mediaType.icon)
          .withName(linkedMediaName)
          .withType('media')
          .withUnique(linkedMediaData.id)
          .withUrl(linkedMediaData.values[0].value.src)
          .done()
        .done()
      .build();

    // Create document
    return await this.create(document);
  }

  async createVariantDocumentWithVariantProperty(documentName: string, documentTypeId: string, dataTypeName: string, propertyVariants: {culture: string, value}[]) {
    await this.ensureNameNotExists(documentName);

    const documentDataBuilder = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId);

    for (const property of propertyVariants) {
      documentDataBuilder
        .addVariant()
          .withName(property.culture === 'en-US' ? documentName : documentName + ' - ' + property.culture)
          .withCulture(property.culture)
          .done()
        .addValue()
          .withAlias(AliasHelper.toAlias(dataTypeName))
          .withValue(property.value)
          .withCulture(property.culture)
          .done();
    }
    const document = documentDataBuilder.build();

    return await this.create(document);
  }

  async updateDomainsForVariantDocument(documentId: string, domains: {domainName: string, isoCode: string}[]) {
    const domainDataBuilder = new DocumentDomainBuilder();
    for (const domain of domains) {
      domainDataBuilder.addDomain()
        .withDomainName(domain.domainName)
        .withIsoCode(domain.isoCode)
        .done();
    } 
    const domainData = domainDataBuilder.build();
    return await this.updateDomains(documentId, domainData);
  }

  async addTextstringValueToInvariantDocument(documentId: string, dataTypeName: string, textValue: string) {
    const documentData = await this.get(documentId);
    const textValueAlias = AliasHelper.toAlias(dataTypeName);
    documentData.values.push({
      alias: textValueAlias,
      value: textValue,
      culture: null,
      segment: null,
      editorAlias: 'Umbraco.Textbox',
      entityType: 'document-property-value'
    });
    return await this.update(documentId, documentData);
  }

  async getNotifications(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id + '/notifications');
    return await response.json();
  }

  async updatetNotifications(id: string, subscribedActionIds: string[] = []) {
    const updateData = {
      "subscribedActionIds": subscribedActionIds
    };
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/document/' + id + '/notifications', updateData);
  }

  async doesNotificationExist(id: string, actionId: string) {
    const notifications = await this.getNotifications(id);
    return notifications.some((notification) => notification.actionId === actionId && notification.subscribed === true);
  }

  async createDocumentWithMultipleCulturesAndSegmentValues(documentName: string, documentTypeId: string, dataTypeName: string, editorAlias: string, cultures: string[], values: {value: string, culture: string, segment: string | null}[]) {
    await this.ensureNameNotExists(documentName);

    const documentBuilder = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId);

    for (const culture of cultures) {
      documentBuilder.addVariant()
        .withName(documentName)
        .withCulture(culture)
        .done();
    }

    const alias = AliasHelper.toAlias(dataTypeName);
    for (const value of values) {
      const valueBuilder = documentBuilder.addValue()
        .withAlias(alias)
        .withValue(value.value)
        .withCulture(value.culture)
        .withEditorAlias(editorAlias);
      if (value.segment) {
        valueBuilder.withSegment(value.segment);
      }
      valueBuilder.done();
    }

    const document = documentBuilder.build();
    return await this.create(document);
  }

  async createDocumentWithMultipleSegmentValues(documentName: string, documentTypeId: string, dataTypeName: string, editorAlias: string, values: {value: string, segment: string | null}[]) {
    await this.ensureNameNotExists(documentName);

    const documentBuilder = new DocumentBuilder()
      .withDocumentTypeId(documentTypeId)
      .addVariant()
        .withName(documentName)
        .done();

    const alias = AliasHelper.toAlias(dataTypeName);
    for (const value of values) {
      const valueBuilder = documentBuilder.addValue()
        .withAlias(alias)
        .withValue(value.value)
        .withEditorAlias(editorAlias);
      if (value.segment) {
        valueBuilder.withSegment(value.segment);
      }
      valueBuilder.done();
    }

    const document = documentBuilder.build();
    return await this.create(document);
  }
}