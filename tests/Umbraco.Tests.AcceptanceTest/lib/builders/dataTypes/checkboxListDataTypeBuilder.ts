import {DataTypeBuilder} from './dataTypeBuilder';

export class CheckboxListDataTypeBuilder extends DataTypeBuilder {
  items: string[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.CheckBoxList';
    this.editorUiAlias = 'Umb.PropertyEditorUi.CheckBoxList';
  }

  withItems(values: string[]) {
    this.items = values;
    return this;
  }

  getValues() {
    let values: any[] = [];

    if (this.items && this.items.length > 0) {
      values.push({
        alias: 'items',
        value: this.items
      });
    }
    return values;
  }
}