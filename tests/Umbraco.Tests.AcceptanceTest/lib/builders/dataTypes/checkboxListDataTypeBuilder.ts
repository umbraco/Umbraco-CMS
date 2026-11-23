import {DataTypeBuilder} from './dataTypeBuilder';
import {ValueListOptionBuilder} from './valueListBuilder';

export class CheckboxListDataTypeBuilder extends DataTypeBuilder {
  optionBuilders: ValueListOptionBuilder<CheckboxListDataTypeBuilder>[] = [];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.CheckBoxList';
    this.editorUiAlias = 'Umb.PropertyEditorUi.CheckBoxList';
  }

  addItem() {
    const builder = new ValueListOptionBuilder<CheckboxListDataTypeBuilder>(this);
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