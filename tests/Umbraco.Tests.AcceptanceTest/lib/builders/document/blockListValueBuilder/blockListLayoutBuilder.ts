import {BlockListValueBuilder} from './blockListValueBuilder';

export class BlockListLayoutBuilder {
  parentBuilder: BlockListValueBuilder;
  contentKey: string;

  constructor(parentBuilder: BlockListValueBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withContentKey(contentKey: string) {
    this.contentKey = contentKey;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    return {
      contentKey: this.contentKey
    };
  }
}