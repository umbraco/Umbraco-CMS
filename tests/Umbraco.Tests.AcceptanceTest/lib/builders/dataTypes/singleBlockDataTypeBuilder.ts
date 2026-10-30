import {DataTypeBuilder} from './dataTypeBuilder';
import {BlockListBlockBuilder} from './blockListBuilder';

export class SingleBlockDataTypeBuilder extends DataTypeBuilder {
  blockBuilder: BlockListBlockBuilder<SingleBlockDataTypeBuilder>[];

  constructor() {
    super();
    this.blockBuilder = [];
    this.editorAlias = 'Umbraco.SingleBlock';
    this.editorUiAlias = 'Umb.PropertyEditorUi.BlockSingle';
  }

  addBlock() {
    const builder = new BlockListBlockBuilder<SingleBlockDataTypeBuilder>(this);
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
