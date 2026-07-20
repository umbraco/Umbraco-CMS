import {DataTypeBuilder} from './dataTypeBuilder';

export class DropdownDataTypeBuilder extends DataTypeBuilder {
  multiple: boolean;
  items: string[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DropDown.Flexible';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Dropdown';
  }

  withMultiple(multiple: boolean) {
    this.multiple = multiple;
    return this;
  }

  withItems(items: string[]) {
    this.items = items;
    return this;
  }

  getValues() {
    let values: any[] = [];
    values.push({
      alias: 'multiple',
      value: this.multiple || false
    });
    if (this.items && this.items.length > 0) {
      values.push({
        alias: 'items',
        value: this.items
      });
    }
    return values;
  }
}