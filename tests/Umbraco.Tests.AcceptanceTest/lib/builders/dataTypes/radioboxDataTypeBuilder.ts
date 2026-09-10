import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class RadioboxDataTypeBuilder extends DataTypeBuilder {
  items: string[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.RadioButtonList';
    this.editorUiAlias = 'Umb.PropertyEditorUi.RadioButtonList';
  }

  withItems(items: string[]) {
    this.items = items;
    return this;
  }

  getValues() {
    const values: DataTypeValues = [];

    if (this.items && this.items.length > 0) {
      values.push({
        alias: 'items',
        value: this.items
      });
    }
    return values;
  }
}