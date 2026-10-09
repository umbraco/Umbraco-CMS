import {SingleBlockDataTypeBuilder} from '../singleBlockDataTypeBuilder';

export class SingleBlockBlockBuilder {
  parentBuilder: SingleBlockDataTypeBuilder;
  contentElementTypeKey: string;
  label: string;
  settingsElementTypeKey: string;

  constructor(parentBuilder: SingleBlockDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withContentElementTypeKey(contentElementTypeKey: string) {
    this.contentElementTypeKey = contentElementTypeKey;
    return this;
  }

  withLabel(label: string) {
    this.label = label;
    return this;
  }

  withSettingsElementTypeKey(settingsElementTypeKey: string) {
    this.settingsElementTypeKey = settingsElementTypeKey;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    let values: any = {};

    if (this.contentElementTypeKey) {
      values.contentElementTypeKey = this.contentElementTypeKey;
    }

    if (this.label) {
      values.label = this.label;
    }

    if (this.settingsElementTypeKey) {
      values.settingsElementTypeKey = this.settingsElementTypeKey;
    }

    return values;
  }
}
