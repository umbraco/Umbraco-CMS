import {DataTypeBuilder} from './dataTypeBuilder';

export class LabelDataTypeBuilder extends DataTypeBuilder {
  umbracoDataValueType: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.Label';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Label';
  }

  withDataValueType(umbracoDataValueType: string) {
    this.umbracoDataValueType = umbracoDataValueType;
    return this;
  }

  getValues() {
    let values: any = [];
    if (this.umbracoDataValueType !== undefined) {
      values.push({
        alias: 'umbracoDataValueType',
        value: this.umbracoDataValueType
      });
    }
    return values;
  }
}