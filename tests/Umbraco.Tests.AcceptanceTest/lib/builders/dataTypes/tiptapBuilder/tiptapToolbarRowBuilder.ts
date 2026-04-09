import {TiptapDataTypeBuilder} from '../tiptapDataTypeBuilder';
import {TiptapToolbarGroupBuilder} from './tiptapToolbarGroupBuilder';

export class TiptapToolbarRowBuilder {
  parentBuilder: TiptapDataTypeBuilder;
  tiptapToolbarGroupBuilder: TiptapToolbarGroupBuilder[];

  constructor(parentBuilder: TiptapDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
    this.tiptapToolbarGroupBuilder = [];
  }

  addToolbarGroup() {
    const builder = new TiptapToolbarGroupBuilder(this);
    this.tiptapToolbarGroupBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return this.tiptapToolbarGroupBuilder.map(builder => builder.build());
  }
}