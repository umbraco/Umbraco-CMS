import {DocumentValueBuilder} from '../documentValueBuilder';

export class ImageCropperValueBuilder {
  parentBuilder: DocumentValueBuilder;
  crops: any[];
  focalPoint: {left: number, top: number};
  temporaryFileId: string;
  src: string;

  constructor(parentBuilder: DocumentValueBuilder) {
    this.parentBuilder = parentBuilder;
    this.crops = [];
  }

  withCrop(crop: {label: string, alias: string, width: number, height: number}) {
    this.crops.push(crop);
    return this;
  }
  
  withFocalPoint(focalPoint: {left: number, top: number}) {
    this.focalPoint = focalPoint;
    return this;
  }

  withSrc(src: string) {
    this.src = src;
    return this;
  }

  withTemporaryFileId(temporaryFileId: string) {
    this.temporaryFileId = temporaryFileId;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    let value: any = {};
    value.crops = this.crops || [];
    value.focalPoint = this.focalPoint || null;
    value.src = this.src || '';
    value.temporaryFileId = this.temporaryFileId || '';
    return value;
  }
}