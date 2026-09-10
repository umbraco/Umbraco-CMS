import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class TextStringDataTypeBuilder extends DataTypeBuilder {
  maxChars: number;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.TextBox';
    this.editorUiAlias = 'Umb.PropertyEditorUi.TextBox';
  }

  withMaxChars(maxChars: number) {
    this.maxChars = maxChars;
    return this;
  }

  getValues() {
    const values: DataTypeValues = [];
    values.push({
      alias: 'maxChars',
      value: this.maxChars || 0
    });
    return values;
  }
}