import {DataTypeBuilder} from './dataTypeBuilder';

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
    let values: any = [];
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