import {MultiNodeTreePickerStartNodeBuilder} from './multiNodeTreePickerStartNodeBuilder';

export class StartNodeQueryStepsBuilder {
  parentBuilder: MultiNodeTreePickerStartNodeBuilder;
  unique: string;
  alias: string;
  anyOfDocTypeKeys: string[] = [];

  constructor(parentBuilder: MultiNodeTreePickerStartNodeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withUnique(unique: string) {
    this.unique = unique;
    return this;
  }

  withAlias(alias: string) {
    this.alias = alias;
    return this;
  }

  withDocTypeKeys(keys: string[]) {
    this.anyOfDocTypeKeys = keys;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    return {
      unique: this.unique,
      alias: this.alias,
      anyOfDocTypeKeys: this.anyOfDocTypeKeys
    };
  }
}
