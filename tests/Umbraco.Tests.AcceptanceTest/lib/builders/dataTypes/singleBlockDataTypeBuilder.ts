import {DataTypeBuilder} from './dataTypeBuilder';
import {SingleBlockBlockBuilder} from './singleBlockBuilder';

export class SingleBlockDataTypeBuilder extends DataTypeBuilder {
  singleBlockBlockBuilder: SingleBlockBlockBuilder[];

  constructor() {
    super();
    this.singleBlockBlockBuilder = [];
    this.editorAlias = 'Umbraco.SingleBlock';
    this.editorUiAlias = 'Umb.PropertyEditorUi.BlockSingle';
  }

  addBlock() {
    const builder = new SingleBlockBlockBuilder(this);
    this.singleBlockBlockBuilder.push(builder);
    return builder;
  }

  getValues() {
    let values: any[] = [];

    // Add blocks alias and value if present
    if (this.singleBlockBlockBuilder && this.singleBlockBlockBuilder.length > 0) {
      values.push({
        alias: 'blocks',
        value: this.singleBlockBlockBuilder.map(block => block.getValues())
      });
    }
    return values;
  }
}
