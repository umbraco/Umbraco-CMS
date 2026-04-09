import {DataTypeBuilder} from './dataTypeBuilder';

export class DecimalDataTypeBuilder extends DataTypeBuilder {
  step: number;
  min: number;
  max: number;
  placeholder: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.Decimal';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Decimal';
  }

  withMin(min: number) {
    this.min = min;
    return this;
  }

  withMax(max: number) {
    this.max = max;
    return this;
  }

  withStep(step: number) {
    this.step = step;
    return this;
  }

  withPlaceholder(placeholder: string) {
    this.placeholder = placeholder;
    return this;
  }

  getValues() {
    let values: any = [];
    values.push({
      alias: 'step',
      value: this.step || 0.01
    });
    if (this.min !== undefined) {
      values.push({
        alias: 'min',
        value: this.min
      });
    }
    if (this.max !== undefined) {
      values.push({
        alias: 'max',
        value: this.max
      });
    }
    if (this.placeholder !== undefined) {
      values.push({
        alias: 'placeholder',
        value: this.placeholder
      });
    }
    return values;
  }
}