import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

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
    const values: DataTypeValues = [];
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