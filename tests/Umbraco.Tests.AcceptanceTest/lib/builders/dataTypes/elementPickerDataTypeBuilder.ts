import {DataTypeBuilder} from './dataTypeBuilder';

export class ElementPickerDataTypeBuilder extends DataTypeBuilder {
  minValidation: number;
  maxValidation: number;
  allowedContentTypeIds: string[];
  startNodeId: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.ElementPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.ElementPicker';
  }

  withMinValidation(min: number) {
    this.minValidation = min;
    return this;
  }

  withMaxValidation(max: number) {
    this.maxValidation = max;
    return this;
  }

  withAllowedContentTypeIds(allowedContentTypeIds: string[]) {
    this.allowedContentTypeIds = allowedContentTypeIds;
    return this;
  }

  withStartNodeId(startNodeId: string) {
    this.startNodeId = startNodeId;
    return this;
  }
  
  getValues() {
    let values: any[] = [];

    if (this.minValidation !== undefined || this.maxValidation !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.minValidation !== undefined ? this.minValidation : undefined,
          max: this.maxValidation !== undefined ? this.maxValidation : undefined
        }
      });
    }

    if (this.allowedContentTypeIds !== undefined) {
      values.push({
        alias: 'allowedContentTypes',
        value: this.allowedContentTypeIds.join(',')
      });
    }

    if (this.startNodeId !== undefined) {
      values.push({
        alias: 'startNodeId',
        value: [this.startNodeId]
      });
    }

    return values;
  }
}
