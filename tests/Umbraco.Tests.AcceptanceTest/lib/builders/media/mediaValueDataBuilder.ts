import {MediaValueBuilder} from './mediaValueBuilder';

export class MediaValueDataBuilder {
  parentBuilder: MediaValueBuilder;
  temporaryFileId: string;
  src: string;
  crops: any[];
  focalPoint: {left: number, top: number};

  constructor(parentBuilder: MediaValueBuilder) {
    this.parentBuilder = parentBuilder;
    this.crops = [];
  }

  withTemporaryFileId(temporaryFileId: string) {
    this.temporaryFileId = temporaryFileId;
    return this;
  }

  withSrc(src: string) {
    this.src = src;
    return this;
  }

  withCrop(crop: {label: string, alias: string, width: number, height: number}) {
    this.crops.push(crop);
    return this;
  }
  
  withFocalPoint(focalPoint: {left: number, top: number}) {
    this.focalPoint = focalPoint;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    let value: any = {};
    value.crops = this.crops || [];
    if (this.focalPoint !== undefined) {
      value.focalPoint = this.focalPoint; 
    }
    if (this.src !== undefined) {
      value.src = this.src;
    }
    value.temporaryFileId = this.temporaryFileId || '';
    return value;
  }
}