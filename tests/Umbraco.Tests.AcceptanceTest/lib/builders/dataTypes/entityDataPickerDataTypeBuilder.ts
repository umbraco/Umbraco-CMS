import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class EntityDataPickerDataTypeBuilder extends DataTypeBuilder {
  minValue: number;
  maxValue: number;
  dataSource: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.EntityDataPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.EntityDataPicker';
  }

  withMinValue(minValue: number) {
    this.minValue = minValue;
    return this;
  }

  withMaxValue(maxValue: number) {
    this.maxValue = maxValue;
    return this;
  }

  withDataSource(dataSource: string) {
    this.dataSource = dataSource;
    return this;
  }

  getValues() {
    const values: DataTypeValues = [];

    // Add validationLimit alias and value if present
    if (this.minValue !== undefined || this.maxValue !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.minValue !== undefined ? this.minValue : '',
          max: this.maxValue !== undefined ? this.maxValue : ''
        }
      });
    }

    values.push({
      alias: 'umbEditorDataSource',
      value: this.dataSource
    });
    return values;
  }
}