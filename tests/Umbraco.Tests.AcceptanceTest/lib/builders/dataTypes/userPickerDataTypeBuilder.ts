import {DataTypeBuilder} from './dataTypeBuilder';

export class UserPickerDataTypeBuilder extends DataTypeBuilder {
  constructor() {
    super();
    this.editorAlias = 'Umbraco.UserPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.UserPicker';
  }

  getValues() {
    return [];
  }
}
