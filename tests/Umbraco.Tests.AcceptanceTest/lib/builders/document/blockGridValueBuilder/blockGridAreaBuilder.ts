import {BlockGridLayoutBuilder} from './blockGridLayoutBuilder';

export class BlockGridAreaBuilder {
  parentBuilder: BlockGridLayoutBuilder;
  key: string;
  itemsLayoutBuilder: BlockGridLayoutBuilder[];

  constructor(parentBuilder: BlockGridLayoutBuilder) {
    this.parentBuilder = parentBuilder;
    this.itemsLayoutBuilder = [];
  }

  withKey(key: string) {
    this.key = key;
    return this;
  }

  addItems() {
    const builder = new BlockGridLayoutBuilder(this);
    this.itemsLayoutBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    return {
      key: this.key,
      items: this.itemsLayoutBuilder.map((builder) => {
        return builder.getValue();
      })
    };
  }
}