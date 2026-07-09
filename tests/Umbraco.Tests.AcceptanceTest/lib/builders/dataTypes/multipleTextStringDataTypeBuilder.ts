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
    values.push({
      alias: 'min',
      value: this.min || 0
    });
    values.push({
      alias: 'max',
      value: this.max || 0
    });
    return values;
  }
}