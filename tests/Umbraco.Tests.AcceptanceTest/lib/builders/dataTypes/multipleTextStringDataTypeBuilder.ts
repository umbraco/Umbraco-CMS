import {DataTypeBuilder} from './dataTypeBuilder';

export class MultipleTextStringDataTypeBuilder extends DataTypeBuilder {
  min: number;
  max: number;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.MultipleTextstring';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MultipleTextString';
  }

  withMin(min: number) {
    this.min = min;
    return this;
  }

  withMax(max: number) {
    this.max = max;
    return this;
  }

  getValues() {
    let values: any = [];
    if (this.min !== undefined || this.max !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.min,
          max: this.max
        }
      });
    }
    return values;
  }
}