import {DataTypeBuilder} from './dataTypeBuilder';

export class ElementPickerDataTypeBuilder extends DataTypeBuilder {
  minValidation: number;
  maxValidation: number;
  allowedContentTypeIds: string[];

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

  withAllowedContentTypes(elementTypeIds: string[]) {
    this.allowedContentTypeIds = elementTypeIds;
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

    // The backend stores allowedContentTypes as a comma-separated string of element-type ids.
    if (this.allowedContentTypeIds !== undefined) {
      values.push({
        alias: 'allowedContentTypes',
        value: this.allowedContentTypeIds.join(',')
      });
    }

    return values;
  }
}
