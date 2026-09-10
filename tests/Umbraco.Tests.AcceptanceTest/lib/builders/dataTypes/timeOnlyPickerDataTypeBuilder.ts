import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class TimeOnlyPickerDataTypeBuilder extends DataTypeBuilder {
  timeFormat: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.TimeOnly';
    this.editorUiAlias = 'Umb.PropertyEditorUi.TimeOnlyPicker';
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