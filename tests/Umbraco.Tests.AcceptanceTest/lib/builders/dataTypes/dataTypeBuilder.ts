import {ensureIdExists} from '../../helpers/BuilderUtils';

export abstract class DataTypeBuilder {
  id: string;
  parentId: string;
  name: string;
  editorAlias: string;
  editorUiAlias: string;

  withName(name: string) {
    this.name = name;
    return this;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  withParentId(parentId: string) {
    this.parentId = parentId;
    return this;
  }

  build() {
    this.id = ensureIdExists(this.id);
    return {
      editorAlias: this.editorAlias,
      editorUiAlias: this.editorUiAlias,
      id: this.id,
      name: this.name,
      parent: this.parentId ? {id: this.parentId} : null,
      values: this.getValues()
    };
  }

  abstract getValues();
}