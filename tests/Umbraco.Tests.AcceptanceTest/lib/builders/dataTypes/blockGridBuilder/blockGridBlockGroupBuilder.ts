import {BlockGridDataTypeBuilder} from '../blockGridDataTypeBuilder';
import {ensureIdExists} from '../../../helpers/BuilderUtils';
import {BlockGridBlockGroup} from '../../types';

export class BlockGridBlockGroupBuilder {
  parentBuilder: BlockGridDataTypeBuilder;
  key: string;
  name: string;

  constructor(parentBuilder: BlockGridDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withName(name: string) {
    this.name = name;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues(): BlockGridBlockGroup {
    this.key = ensureIdExists(this.key);

    return {
      key: this.key,
      name: this.name || null
    };
  }
}