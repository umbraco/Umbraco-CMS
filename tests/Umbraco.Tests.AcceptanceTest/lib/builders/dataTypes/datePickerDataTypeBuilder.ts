import {DataTypeBuilder} from './dataTypeBuilder';

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
    let values: any = [];
    values.push({
      alias: 'format',
      value: this.format || 'YYYY-MM-DD'
    });
    return values;
  }
}