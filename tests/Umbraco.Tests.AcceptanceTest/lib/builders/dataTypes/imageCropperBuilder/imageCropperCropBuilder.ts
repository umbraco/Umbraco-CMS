import {ImageCropperDataTypeBuilder} from '../imageCropperDataTypeBuilder';
import {CropConfiguration} from '../../types';

export class ImageCropperCropBuilder {
  parentBuilder: ImageCropperDataTypeBuilder;
  label: string;
  alias: string;
  height: number;
  width: number;

  constructor(parentBuilder: ImageCropperDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withLabel(label: string) {
    this.label = label;
    return this;
  }

  withAlias(alias: string) {
    this.alias = alias;
    return this;
  }

  withHeight(height: number) {
    this.height = height;
    return this;
  }

  withWidth(width: number) {
    this.width = width;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues(): CropConfiguration {
    const values: CropConfiguration = {};
    if (this.label) {
      values.label = this.label;
    }
    if (this.alias) {
      values.alias = this.alias;
    }
    if (this.width) {
      values.width = this.width;
    }
    if (this.height) {
      values.height = this.height;
    }
    return values;
  }
}