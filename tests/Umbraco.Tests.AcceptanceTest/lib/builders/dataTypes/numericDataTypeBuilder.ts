import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class NumericDataTypeBuilder extends DataTypeBuilder {
  min: number;
  max: number;
  step: number;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.Integer';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Integer';
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

  getValues() {
    const values: DataTypeValues = [];
    values.push({
      alias: 'min',
      value: this.min || 0
    });
    values.push({
      alias: 'max',
      value: this.max || 0
    });
    values.push({
      alias: 'step',
      value: this.step || 0
    });
    return values;
  }
}