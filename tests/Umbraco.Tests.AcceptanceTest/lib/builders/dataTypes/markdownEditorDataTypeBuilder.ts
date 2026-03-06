import {DataTypeBuilder} from './dataTypeBuilder';

export class MarkdownEditorDataTypeBuilder extends DataTypeBuilder {
  preview: boolean;
  overlaySize: string;
  defaultValue: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.MarkdownEditor';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MarkdownEditor';
  }

  withPreview(preview: boolean) {
    this.preview = preview;
    return this;
  }

  withOverlaySize(overlaySize: string) {
    this.overlaySize = overlaySize;
    return this;
  }

  withDefaultValue(defaultValue: string) {
    this.defaultValue = defaultValue;
    return this;
  }

  getValues() {
    let values: any = [];
    if (this.preview !== undefined) {
      values.push({
        alias: 'preview',
        value: this.preview
      });
    }
    if (this.overlaySize !== undefined) {
      values.push({
        alias: 'overlaySize',
        value: this.overlaySize
      });
    }
    if (this.defaultValue !== undefined) {
      values.push({
        alias: 'defaultValue',
        value: this.defaultValue
      });
    }
    return values;
  }
}