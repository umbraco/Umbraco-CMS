import {ApiHelpers} from "./ApiHelpers";

export class TemporaryFileApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/temporary-file/' + id);
    return response.status() === 200;
  }

  async create(id: string, name: string, mimeType, filePath) {
    return this.api.postMultiPartForm(this.api.baseUrl + '/umbraco/management/api/v1/temporary-file', id, name, mimeType, filePath)
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/temporary-file/' + id);
    return await response.json();
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/temporary-file/' + id);
  }

  async createDefaultTemporaryFile() {
    return this.createTemporaryFile('File.txt', 'File', 'text/plain');
  }

  async createDefaultTemporaryImageFile() {
    return this.createTemporaryFile('Umbraco.png', 'Image', 'image/png');
  }

  async createDefaultTemporaryArticleFile() {
    return this.createTemporaryFile('Article.pdf', 'Article', 'application/pdf');
  }

  async createDefaultTemporaryVideoFile() {
    return this.createTemporaryFile('Video.mp4', 'Video', 'video/mp4'); 
  }

  async createDefaultTemporaryAudioFile() {
    return this.createTemporaryFile('Audio.mp3', 'Audio', 'audio/mpeg'); 
  }

  async createDefaultTemporarySVGFile() {
    return this.createTemporaryFile('VectorGraphics.svg', 'Vector Graphics (SVG)', 'image/svg+xml'); 
  }

  async createTemporaryFile(fileName: string, mediaTypeName: string, mimeType: string) {
    const mediaType = await this.api.mediaType.getByName(mediaTypeName);
    const crypto = require('crypto');
    const temporaryFileId = crypto.randomUUID();
    const filePath = './fixtures/mediaLibrary/' + fileName;
    await this.create(temporaryFileId, fileName, mimeType, filePath);

    return {mediaTypeId: mediaType.id, temporaryFileId: temporaryFileId};
  }
}