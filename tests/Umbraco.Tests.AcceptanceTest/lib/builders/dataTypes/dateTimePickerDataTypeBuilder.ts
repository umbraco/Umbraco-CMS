import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class DateTimePickerDataTypeBuilder extends DataTypeBuilder {
  timeFormat: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DateTimeUnspecified';
    this.editorUiAlias = 'Umb.PropertyEditorUi.DateTimePicker';
  }

  withTimeFormat(timeFormat: string) {
    this.timeFormat = timeFormat;
    return this;
  }

  getValues() {
    const values: DataTypeValues = [];
    values.push({
      alias: 'timeFormat',
      value: this.timeFormat || 'HH:mm'
    });
    return values;
  }
}