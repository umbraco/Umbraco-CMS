import {DataTypeBuilder} from './dataTypeBuilder';

export class ElementPickerDataTypeBuilder extends DataTypeBuilder {
  minValidation: number;
  maxValidation: number;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.ElementPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.ElementPicker';
  }

  withMinValidation(minValidation: number) {
    this.minValidation = minValidation;
    return this;
  }

  withMaxValidation(maxValidation: number) {
    this.maxValidation = maxValidation;
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

    return values;
  }
}
