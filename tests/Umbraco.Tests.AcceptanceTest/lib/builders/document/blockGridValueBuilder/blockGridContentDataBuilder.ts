import {BlockGridValueBuilder} from './blockGridValueBuilder';
import {BlockGridContentDataValueBuilder} from './blockGridContentDataValueBuilder';

export class BlockGridContentDataBuilder {
  parentBuilder: BlockGridValueBuilder;
  contentTypeKey: string;
  key: string;
  udi: string;
  contentDataValueBuilder: BlockGridContentDataValueBuilder[];

  constructor(parentBuilder: BlockGridValueBuilder) {
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

  withUdi(udi: string) {
    this.udi = udi;
    return this;
  }

  addContentDataValue(){
    const builder = new BlockGridContentDataValueBuilder(this);
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
      udi: this.udi || null,
      values: this.contentDataValueBuilder.map((builder) => {
        return builder.getValue();
      })
    };
  }
}