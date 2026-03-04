import {DataTypeBuilder} from './dataTypeBuilder';

export class CodeEditorDataTypeBuilder extends DataTypeBuilder {
  language: string;
  height: number;
  lineNumbers: boolean;
  minimap: boolean;
  wordWrap: boolean;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.Plain.String';
    this.editorUiAlias = 'Umb.PropertyEditorUi.CodeEditor';
  }

  withLanguage(language: string) {
    this.language = language;
    return this;
  }

  withHeight(height: number) {
    this.height = height;
    return this;
  }

  withLineNumbers(lineNumbers: boolean) {
    this.lineNumbers = lineNumbers;
    return this;
  }

  withMinimap(minimap: boolean) {
    this.minimap = minimap;
    return this;
  }

  withWordWrap(wordWrap: boolean) {
    this.wordWrap = wordWrap;
    return this;
  }

  getValues() {
    let values: any = [];
    values.push({
      alias: 'language',
      value: this.language || 'javascript'
    });
    values.push({
      alias: 'height',
      value: this.height || 400
    });
    values.push({
      alias: 'lineNumbers',
      value: this.lineNumbers !== undefined ? this.lineNumbers : true
    });
    values.push({
      alias: 'minimap',
      value: this.minimap !== undefined ? this.minimap : true
    });
    values.push({
      alias: 'wordWrap',
      value: this.wordWrap !== undefined ? this.wordWrap : false
    });
    return values;
  }
}