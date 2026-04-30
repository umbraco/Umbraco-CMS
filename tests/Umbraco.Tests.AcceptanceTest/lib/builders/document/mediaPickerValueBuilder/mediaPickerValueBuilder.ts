import {DocumentValueBuilder} from '../documentValueBuilder';

export class MediaPickerValueBuilder {
  parentBuilder: DocumentValueBuilder;
  key: string;
  mediaKey: string;
  mediaTypeAlias: string;
  focalPoint: { left: number, top: number };
  crops: string[];

  constructor(parentBuilder: DocumentValueBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withKey(key: string) {
    this.key = key;
    return this;
  }

  withMediaKey(mediaKey: string) {
    this.mediaKey = mediaKey;
    return this;
  }

  withMediaTypeAlias(mediaTypeAlias: string) {
    this.mediaTypeAlias = mediaTypeAlias;
    return this;
  }

  withFocalPoint(focalPoint: { left: number, top: number }) {
    this.focalPoint = focalPoint;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    let value: any = {};
    if (this.key == null) {
      const crypto = require('crypto');
      this.key = crypto.randomUUID();
    }
    value.key = this.key;
    if (this.mediaKey !== undefined) {
      value.mediaKey = this.mediaKey;
    }
    value.mediaTypeAlias = this.mediaTypeAlias || '';
    value.focalPoint = this.focalPoint || null;
    value.crops = this.crops || [];
    return value;
  }
}
