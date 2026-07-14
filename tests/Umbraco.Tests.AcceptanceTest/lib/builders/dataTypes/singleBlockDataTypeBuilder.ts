import {DataTypeBuilder} from './dataTypeBuilder';
import {BlockListBlockBuilder} from './blockListBuilder';
import {BlockListDataTypeBuilder} from './blockListDataTypeBuilder';

export class SingleBlockDataTypeBuilder extends DataTypeBuilder {
  blockBuilder: BlockListBlockBuilder[];

  constructor() {
    super();
    this.blockBuilder = [];
    this.editorAlias = 'Umbraco.SingleBlock';
    this.editorUiAlias = 'Umb.PropertyEditorUi.BlockSingle';
  }

  addBlock() {
    // Reuse Block List's block builder: it only calls the shared done()/build(), so passing this
    // sibling builder as its parent is safe. The `as unknown as` cast is needed because TypeScript
    // rejects a direct cast between the two sibling types; it's compile-time only.
    const builder = new BlockListBlockBuilder(this as unknown as BlockListDataTypeBuilder);
    this.blockBuilder.push(builder);
    return builder;
  }

  getValues() {
    const values: any[] = [];

    if (this.blockBuilder && this.blockBuilder.length > 0) {
      values.push({
        alias: 'blocks',
        value: this.blockBuilder.map(block => block.getValues())
      });
    }
    return values;
  }
}
