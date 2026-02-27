import {DataTypeBuilder} from './dataTypeBuilder';

export class TextAreaDataTypeBuilder extends DataTypeBuilder {
  maxChars: number;
  rows: number;
  minHeight: number;
  maxHeight: number;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.TextArea';
    this.editorUiAlias = 'Umb.PropertyEditorUi.TextArea';
  }

  withMaxChars(maxChars: number) {
    this.maxChars = maxChars;
    return this;
  }

  withRows(rows: number) {
    this.rows = rows;
    return this;
  }

  withMinHeight(minHeight: number) {
    this.minHeight = minHeight;
    return this;
  }

  withMaxHeight(maxHeight: number) {
    this.maxHeight = maxHeight;
    return this;
  }

  getValues() {
    let values: any = [];
    values.push({
      alias: 'maxChars',
      value: this.maxChars || 0
    });
    values.push({
      alias: 'rows',
      value: this.rows || 0
    });
    values.push({
      alias: 'minHeight',
      value: this.minHeight || 0
    });
    values.push({
      alias: 'maxHeight',
      value: this.maxHeight || 0
    });
    return values;
  }
}