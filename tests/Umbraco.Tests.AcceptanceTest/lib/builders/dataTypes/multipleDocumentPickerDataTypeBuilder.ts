import {DataTypeBuilder} from './dataTypeBuilder';

// Holds any number of documents, which is what the multi node tree picker was reached for. The single
// document picker is Umbraco.ContentPicker.
export class MultipleDocumentPickerDataTypeBuilder extends DataTypeBuilder {
  allowedContentTypes: string;
  minValue: number;
  maxValue: number;
  startNodeId: string;
  ignoreUserStartNodes: boolean;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DocumentPicker.Multiple';
    this.editorUiAlias = 'Umb.PropertyEditorUi.DocumentPicker.Multiple';
  }

  withAllowedContentTypes(allowedContentTypes: string) {
    this.allowedContentTypes = allowedContentTypes;
    return this;
  }

  withMinValue(minValue: number) {
    this.minValue = minValue;
    return this;
  }

  withMaxValue(maxValue: number) {
    this.maxValue = maxValue;
    return this;
  }

  withStartNodeId(startNodeId: string) {
    this.startNodeId = startNodeId;
    return this;
  }

  withIgnoreUserStartNodes(ignoreUserStartNodes: boolean) {
    this.ignoreUserStartNodes = ignoreUserStartNodes;
    return this;
  }

  getValues() {
    let values: any[] = [];
    if (this.allowedContentTypes !== undefined) {
      values.push({
        alias: 'allowedContentTypes',
        value: this.allowedContentTypes
      });
    }
    if (this.minValue !== undefined || this.maxValue !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.minValue !== undefined ? this.minValue : '',
          max: this.maxValue !== undefined ? this.maxValue : ''
        }
      });
    }
    if (this.startNodeId !== undefined) {
      values.push({
        alias: 'startNodeId',
        value: this.startNodeId
      });
    }
    if (this.ignoreUserStartNodes !== undefined) {
      values.push({
        alias: 'ignoreUserStartNodes',
        value: this.ignoreUserStartNodes
      });
    }
    return values;
  }
}
