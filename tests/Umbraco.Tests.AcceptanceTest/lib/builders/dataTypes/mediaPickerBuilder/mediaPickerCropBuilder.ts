import {MediaPickerDataTypeBuilder} from '../mediaPickerDataTypeBuilder';

export class MediaPickerCropBuilder {
  parentBuilder: MediaPickerDataTypeBuilder;
  label: string;
  alias: string;
  height: number;
  width: number;

  constructor(parentBuilder: MediaPickerDataTypeBuilder) {
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

  getValues() {
    let values: any = {};
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