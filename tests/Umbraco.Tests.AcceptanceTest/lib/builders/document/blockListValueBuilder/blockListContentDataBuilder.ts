import {BlockListValueBuilder} from './blockListValueBuilder';
import {BlockListContentDataValueBuilder} from './blockListContentDataValueBuilder';

export class BlockListContentDataBuilder {
  parentBuilder: BlockListValueBuilder;
  contentTypeKey: string;
  key: string;
  contentDataValueBuilder: BlockListContentDataValueBuilder[];

  constructor(parentBuilder: BlockListValueBuilder) {
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

  addContentDataValue(){
    const builder = new BlockListContentDataValueBuilder(this);
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