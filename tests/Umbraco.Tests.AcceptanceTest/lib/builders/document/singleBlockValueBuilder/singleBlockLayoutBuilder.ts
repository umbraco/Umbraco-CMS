import {SingleBlockValueBuilder} from './singleBlockValueBuilder';

export class SingleBlockLayoutBuilder {
  parentBuilder: SingleBlockValueBuilder;
  contentKey: string;

  constructor(parentBuilder: SingleBlockValueBuilder) {
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
