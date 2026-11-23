import {DataTypeBuilder} from './dataTypeBuilder';
import {ValueListOptionBuilder} from './valueListBuilder';

// There is one dropdown editor per number of values it holds, so the two builders differ only in which
// editor they target - the options are configured the same way.
export class DropdownDataTypeBuilder extends DataTypeBuilder {
  optionBuilders: ValueListOptionBuilder<DropdownDataTypeBuilder>[] = [];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.DropDown.Flexible';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Dropdown';
  }

  addItem() {
    const builder = new ValueListOptionBuilder<DropdownDataTypeBuilder>(this);
    this.optionBuilders.push(builder);
    return builder;
  }

  getValues() {
    let values: any[] = [];
    if (this.optionBuilders.length > 0) {
      values.push({
        alias: 'items',
        value: this.optionBuilders.map(builder => builder.getValues())
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
