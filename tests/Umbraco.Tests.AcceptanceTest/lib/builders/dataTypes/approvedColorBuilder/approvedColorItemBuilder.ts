import {ApprovedColorDataTypeBuilder} from '../approvedColorDataTypeBuilder';

export class ApprovedColorItemBuilder {
  parentBuilder: ApprovedColorDataTypeBuilder;
  label: string;
  value: string;

  constructor(parentBuilder: ApprovedColorDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withLabel(label: string) {
    this.label = label;
    return this;
  }

  withValue(value: string) {
    this.value = value;
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
    if (this.value) {
      values.value = this.value;
    }
    return values;
  }
}