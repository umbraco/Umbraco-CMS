import {DataTypeBuilder} from './dataTypeBuilder';

// There is one dropdown editor per number of values it holds, so the two builders differ only in which
// editor they target - the options are configured the same way.
export class DropdownDataTypeBuilder extends DataTypeBuilder {
  items: string[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DropDown.Flexible';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Dropdown';
  }

  withItems(items: string[]) {
    this.items = items;
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

export class SingleDropdownDataTypeBuilder extends DropdownDataTypeBuilder {
  constructor() {
    super();
    this.editorAlias = 'Umbraco.DropDown.Single';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Dropdown.Single';
  }
}
