import {DataTypeBuilder} from './dataTypeBuilder';

// There is one member picker editor per number of members it holds, so the two builders differ in which
// editor they target, and only the multiple picker has a member count to validate.
export class MemberPickerDataTypeBuilder extends DataTypeBuilder {
  filter: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.MemberPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MemberPicker';
  }

  withFilter(filter: string) {
    this.filter = filter;
    return this;
  }

  getValues() {
    let values: any[] = [];
    if (this.filter !== undefined) {
      values.push({
        alias: 'filter',
        value: this.filter
      });
    }
    return values;
  }
}

export class MultipleMemberPickerDataTypeBuilder extends MemberPickerDataTypeBuilder {
  minValue: number;
  maxValue: number;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.MultipleMemberPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MultipleMemberPicker';
  }

  withMinValue(minValue: number) {
    this.minValue = minValue;
    return this;
  }

  withMaxValue(maxValue: number) {
    this.maxValue = maxValue;
    return this;
  }

  getValues() {
    let values: any[] = super.getValues();
    if (this.minValue !== undefined || this.maxValue !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.minValue !== undefined ? this.minValue : '',
          max: this.maxValue !== undefined ? this.maxValue : ''
        }
      });
    }
    return values;
  }
}
