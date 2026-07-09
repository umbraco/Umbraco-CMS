import {DataTypeBuilder} from './dataTypeBuilder';

export class UploadFieldDataTypeBuilder extends DataTypeBuilder {
  fileExtensions: string[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.UploadField';
    this.editorUiAlias = 'Umb.PropertyEditorUi.UploadField';
  }

  withFileExtensions(fileExtensions: string[]) {
    this.fileExtensions = fileExtensions;
    return this;
  }

  getValues() {
    let values: any[] = [];

    if (this.fileExtensions && this.fileExtensions.length > 0) {
      values.push({
        alias: 'fileExtensions',
        value: this.fileExtensions
      });
    }
    return values;
  }
}