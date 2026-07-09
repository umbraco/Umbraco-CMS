import {DataTypeBuilder} from './dataTypeBuilder';

export class EmailAddressDataTypeBuilder extends DataTypeBuilder {
  constructor() {
    super();
    this.editorAlias = 'Umbraco.EmailAddress';
    this.editorUiAlias = 'Umb.PropertyEditorUi.EmailAddress';
  }

  getValues() {
    let values: any = [];
    values.push({
      alias: 'inputType',
      value: 'email'
    });
    return values;
  }
}