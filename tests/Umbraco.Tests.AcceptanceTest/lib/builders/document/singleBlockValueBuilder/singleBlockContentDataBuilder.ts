import {SingleBlockValueBuilder} from './singleBlockValueBuilder';
import {SingleBlockContentDataValueBuilder} from './singleBlockContentDataValueBuilder';

export class SingleBlockContentDataBuilder {
  parentBuilder: SingleBlockValueBuilder;
  contentTypeKey: string;
  key: string;
  contentDataValueBuilder: SingleBlockContentDataValueBuilder[];

  constructor(parentBuilder: SingleBlockValueBuilder) {
    this.parentBuilder = parentBuilder;
    this.contentDataValueBuilder = [];
  }

  withContentTypeKey(contentTypeKey: string) {
    this.contentTypeKey = contentTypeKey;
    return this;
  }

  withKey(key: string) {
    this.key = key;
    return this;
  }

  addContentDataValue() {
    const builder = new SingleBlockContentDataValueBuilder(this);
    this.contentDataValueBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    return {
      contentTypeKey: this.contentTypeKey,
      key: this.key,
      values: this.contentDataValueBuilder.map((builder) => {
        return builder.getValue();
      })
    };
  }
}
