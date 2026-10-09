import {DataTypeBuilder} from './dataTypeBuilder';

// There is one label editor per type of value it holds, so the type is chosen by targeting an editor
// rather than by configuring a value type.
export class LabelDataTypeBuilder extends DataTypeBuilder {
  labelTemplate: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.Label';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Label';
  }

  withEditor(editorAlias: string, editorUiAlias: string) {
    this.editorAlias = editorAlias;
    this.editorUiAlias = editorUiAlias;
    return this;
  }

  withLabelTemplate(labelTemplate: string) {
    this.labelTemplate = labelTemplate;
    return this;
  }

  getValues() {
    let values: any = [];
    if (this.labelTemplate !== undefined) {
      values.push({
        alias: 'labelTemplate',
        value: this.labelTemplate
      });
    }
    return values;
  }
}
