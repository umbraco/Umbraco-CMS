import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class EmailAddressDataTypeBuilder extends DataTypeBuilder {
  constructor() {
    super();
    this.editorAlias = 'Umbraco.EmailAddress';
    this.editorUiAlias = 'Umb.PropertyEditorUi.EmailAddress';
  }

  getValues() {
    const values: DataTypeValues = [];
    values.push({
      alias: 'inputType',
      value: 'email'
    });
    return values;
  }
}