import {DataTypeBuilder} from './dataTypeBuilder';
import {ValueListOptionBuilder} from './valueListBuilder';

export class RadioboxDataTypeBuilder extends DataTypeBuilder {
  optionBuilders: ValueListOptionBuilder<RadioboxDataTypeBuilder>[] = [];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.RadioButtonList';
    this.editorUiAlias = 'Umb.PropertyEditorUi.RadioButtonList';
  }

  addItem() {
    const builder = new ValueListOptionBuilder<RadioboxDataTypeBuilder>(this);
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