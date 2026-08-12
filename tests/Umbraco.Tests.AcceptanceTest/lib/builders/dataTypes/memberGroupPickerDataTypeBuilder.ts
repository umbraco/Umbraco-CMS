import {DataTypeBuilder} from './dataTypeBuilder';

export class MemberGroupPickerDataTypeBuilder extends DataTypeBuilder {
  constructor() {
    super();
    this.editorAlias = 'Umbraco.MemberGroupPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MemberGroupPicker';
  }

  getValues() {
    return [];
  }
}
