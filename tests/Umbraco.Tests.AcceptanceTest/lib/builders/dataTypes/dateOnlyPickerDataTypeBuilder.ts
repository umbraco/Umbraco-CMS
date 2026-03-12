import {DataTypeBuilder} from './dataTypeBuilder';

export class DateOnlyPickerDataTypeBuilder extends DataTypeBuilder {
  format: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DateOnly';
    this.editorUiAlias = 'Umb.PropertyEditorUi.DateOnlyPicker';
  }

  getValues() {
    return [];
  }
}