import {ApiHelpers} from "./ApiHelpers";
import {MediaBuilder} from "../builders";
import {AliasHelper} from "./AliasHelper";

export class MediaApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async ensureNameNotExists(name: string) {
    const rootMedia = await this.getAllAtRoot();
    const jsonMedia = await rootMedia.json();

    for (const media of jsonMedia.items) {
      if (media.variants[0].name === name) {
        if (media.hasChildren) {
          return await this.recurseDeleteChildren(media);
        }
        return await this.delete(media.id);
      } else if (media.hasChildren) {
        await this.recurseChildren(name, media.id, true);
      }
    }
    return null;
  }

  async getAllAtRoot() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/media/root?skip=0&take=10000');
  }

  private async recurseChildren(name: string, id: string, toDelete: boolean) {
    const items = await this.getChildren(id);

    for (const child of items) {
      if (child.variants[0].name === name) {
        if (!toDelete) {
          return await this.get(child.id);
        }
        if (child.hasChildren) {
          return await this.recurseDeleteChildren(child);
        } else {
          return await this.delete(child.id);
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

  private async recurseDeleteChildren(media) {
    if (!media.hasChildren) {
      return await this.delete(media.id);
    }
    const items = await this.getChildren(media.id);

    for (const child of items) {
      if (child.hasChildren) {
        await this.recurseDeleteChildren(child);
      } else {
        await this.delete(child.id);
      }
    }
    return await this.delete(media.id);
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/media/' + id);
    return await response.json();
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/media/' + id);
  }

  async getChildren(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/tree/media/children?parentId=' + id + '&skip=0&take=10000');
    const items = await response.json();
    return items.items;
  }

  async create(media) {
    if (media == null) {
      return;
    }
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/media', media);
    return response.headers().location.split("/").pop();
  }

  async doesNameExist(name: string) {
    return await this.getByName(name);
  }

  async getByName(name: string) {
    const rootMedia = await this.getAllAtRoot();
    const jsonMedia = await rootMedia.json();

    for (const media of jsonMedia.items) {
      if (media.variants[0].name === name) {
        return await this.get(media.id);
      } else if (media.hasChildren) {
        const result = await this.recurseChildren(name, media.id, false);
        if (result) { 
          return result;
        }
      }
    }
    return null;
  }

  async getFullMediaUrl(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/media/urls?id=' + id);
    const urls = await response.json();

    return urls[0].urlInfos[0].url;
  }
  
  async getRecycleBinItems() {
    return await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/recycle-bin/media/root?skip=0&take=10000');
  }

  async emptyRecycleBin() {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/recycle-bin/media');
  }

  async doesMediaItemExistInRecycleBin(mediaItemName: string) {
    const recycleBin = await this.getRecycleBinItems();
    const jsonRecycleBin = await recycleBin.json();
    for (const media of jsonRecycleBin.items) {
      if (media.variants[0].name === mediaItemName) {
        return true;
      }
    }
    return false;
  }

  async trashMediaItem(mediaItemName: string) {
    const media = await this.getByName(mediaItemName);
    return await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/media/' + media.id + '/move-to-recycle-bin');
  }

  async getMediaPathByName(name: string) {
    const media = await this.getByName(name);
    const mediaUrl = await this.getMediaUrlWithoutBaseUrl(media.id);
    
    if (media && mediaUrl > 0) {
      // Gets the random mediaPath for the media
      const mediaPath = mediaUrl.split('/media/').pop()?.split('/')[0];
      // Gets the file name from the mediaUrl
      const fileName = mediaUrl.split('/').pop();

      return {
        mediaPath: mediaPath,
        fileName: fileName
      };
    }

    return null;
  }

  async getMediaUrlWithoutBaseUrl(id: string) {
    const mediaUrl = await this.getFullMediaUrl(id);

    if (mediaUrl != null) {
      return mediaUrl.split(this.api.baseUrl).pop();
    }
    return null;
  }

  async createDefaultMediaFile(mediaName: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryFile();
    return await this.createDefaultMediaItem(mediaName, temporaryFile);
  }

  async createDefaultMediaFileAndParentId(mediaName: string, parentId: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryFile();
    return await this.createDefaultMediaItemWithParentId(mediaName, parentId, temporaryFile);
  }

  async createDefaultMediaFolder(mediaFolderName: string) {
    const mediaType = await this.api.mediaType.getByName('Folder');
    await this.ensureNameNotExists(mediaFolderName);

    const media = new MediaBuilder()
      .withMediaTypeId(mediaType.id)
      .addVariant()
        .withName(mediaFolderName)
        .done()
      .build();

    return await this.create(media);
  }

  async createDefaultMediaFolderAndParentId(mediaName: string, parentId: string) {
    const mediaType = await this.api.mediaType.getByName('Folder');
    await this.ensureNameNotExists(mediaName);

    const media = new MediaBuilder()
      .withMediaTypeId(mediaType.id)
      .withParentId(parentId)
      .addVariant()
        .withName(mediaName)
        .done()
      .build();

    return await this.create(media);
  }
  
  async createDefaultMediaWithImage(mediaName: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryImageFile();
    return await this.createDefaultMediaItem(mediaName, temporaryFile);
  }

  async createDefaultMediaWithArticle(mediaName: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryArticleFile();
    return await this.createDefaultMediaItem(mediaName, temporaryFile);
  }
  
  async createDefaultMediaWithImageAndParentId(mediaName: string, parentId: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryImageFile();
    return await this.createDefaultMediaItemWithParentId(mediaName, parentId, temporaryFile);
  }

  async getAllMediaNames(orderBy: string = 'updateDate', orderDirection: string = 'Descending') {
    let mediaNames: string[] = [];
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/collection/media?id=&dataTypeId=&orderBy=' + orderBy + '&orderDirection=' + orderDirection + '&skip=0&take=100');
    const mediaItems = await response.json();
    for (const media of mediaItems.items) {
      mediaNames.push(media.variants[0].name);
    }
    return mediaNames;
  }

  async doesMediaItemHaveChildName(mediaId: string, childName: string) {
    const childrenItems = await this.getChildren(mediaId);
    for (const child of childrenItems) {
      if (child.variants[0].name === childName) {
        return true;
      }
    }
    return false;
  }

  async createDefaultMediaWithVideo(mediaName: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryVideoFile();
    return await this.createDefaultMediaItem(mediaName, temporaryFile);
  }

  async createDefaultMediaWithVideoAndParentId(mediaName: string, parentId: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryVideoFile();
    return await this.createDefaultMediaItemWithParentId(mediaName, parentId, temporaryFile);
  }

  async createDefaultMediaWithAudio(mediaName: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporaryAudioFile();
    return await this.createDefaultMediaItem(mediaName, temporaryFile);
  }

  async createDefaultMediaWithSVG(mediaName: string) {
    const temporaryFile = await this.api.temporaryFile.createDefaultTemporarySVGFile();
    return await this.createDefaultMediaItem(mediaName, temporaryFile);
  }

  private async createDefaultMediaItem(mediaName: string, temporaryFile) {
    await this.ensureNameNotExists(mediaName);

    const media = new MediaBuilder()
      .withMediaTypeId(temporaryFile.mediaTypeId)
      .addVariant()
        .withName(mediaName)
        .done()
      .addValue()
        .withAlias('umbracoFile')
        .addValueData()
          .withTemporaryFileId(temporaryFile.temporaryFileId)
          .done()
        .done()
      .build();
    
    return await this.create(media);
  }

  private async createDefaultMediaItemWithParentId(mediaName: string, parentId: string, temporaryFile) {
    await this.ensureNameNotExists(mediaName);

    const media = new MediaBuilder()
      .withMediaTypeId(temporaryFile.mediaTypeId)
      .withParentId(parentId)
      .addVariant()
        .withName(mediaName)
        .done()
      .addValue()
        .withAlias('umbracoFile')
        .addValueData()
          .withTemporaryFileId(temporaryFile.temporaryFileId)
          .done()
        .done()
      .build();

    return await this.create(media);
  }

  async createDefaultMediaWithTextstring(mediaName: string, mediaTypeName: string, textValue: string, dataTypeName: string) {
    await this.ensureNameNotExists(mediaName);
    await this.api.mediaType.ensureNameNotExists(mediaTypeName);
    const dataTypeData = await this.api.dataType.getByName(dataTypeName);
    const mediaTypeId = await this.api.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, 'Test Group', true) || '';

    const media = new MediaBuilder()
      .withMediaTypeId(mediaTypeId)
      .addVariant()
        .withName(mediaName)
        .done()
      .addValue()
        .withAlias(AliasHelper.toAlias(dataTypeName))
        .withEntityType('media-property-value')
        .withValue(textValue)
        .done()
      .build();

    return await this.create(media);
  }
}