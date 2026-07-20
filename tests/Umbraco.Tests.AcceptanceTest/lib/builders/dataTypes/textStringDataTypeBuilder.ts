import {DataTypeBuilder} from './dataTypeBuilder';

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
    let values: any = [];
    values.push({
      alias: 'maxChars',
      value: this.maxChars || 0
    });
    return values;
  }
}