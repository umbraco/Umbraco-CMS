import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class DatePickerDataTypeBuilder extends DataTypeBuilder {
  format: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DateTime';
    this.editorUiAlias = 'Umb.PropertyEditorUi.DatePicker';
  }

  withFormat(format: string) {
    this.format = format;
    return this;
  }

  getValues() {
    const values: DataTypeValues = [];
    values.push({
      alias: 'format',
      value: this.format || 'YYYY-MM-DD'
    });
    return values;
  }
}