import {DataTypeBuilder} from './dataTypeBuilder';
import {DataTypeValues} from '../types';

export class TagsDataTypeBuilder extends DataTypeBuilder {
  group: string;
  storageType: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.Tags';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Tags';
  }

  withGroup(group: string) {
    this.group = group;
    return this;
  }

  withStorageType(storageType: string) {
    this.storageType = storageType;
    return this;
  }

  getValues() {
    const values: DataTypeValues = [];
    values.push({
      alias: 'group',
      value: this.group || 'default'
    });
    values.push({
      alias: 'storageType',
      value: this.storageType || 'Json'
    });
    return values;
  }
}