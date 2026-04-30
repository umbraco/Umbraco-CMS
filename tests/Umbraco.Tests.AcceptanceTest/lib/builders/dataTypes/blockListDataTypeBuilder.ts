import {DataTypeBuilder} from './dataTypeBuilder';
import {BlockListBlockBuilder} from './blockListBuilder';

export class BlockListDataTypeBuilder extends DataTypeBuilder {
  minValue: number;
  maxValue: number;
  maxPropertyWidth: string;
  useSingleBlockMode: boolean;
  useLiveEditing: boolean;
  useInlineEditingAsDefault: boolean;
  blockListBlockBuilder: BlockListBlockBuilder[];

  constructor() {
    super();
    this.blockListBlockBuilder = [];
    this.editorAlias = 'Umbraco.BlockList';
    this.editorUiAlias = 'Umb.PropertyEditorUi.BlockList';
  }

  withMinValue(minValue: number) {
    this.minValue = minValue;
    return this;
  }

  withMaxValue(maxValue: number) {
    this.maxValue = maxValue;
    return this;
  }

  withMaxPropertyWidth(maxPropertyWidth: string) {
    this.maxPropertyWidth = maxPropertyWidth;
    return this;
  }

  withSingleBlockMode(useSingleBlockMode: boolean) {
    this.useSingleBlockMode = useSingleBlockMode;
    return this;
  }

  withLiveEditing(useLiveEditing: boolean) {
    this.useLiveEditing = useLiveEditing;
    return this;
  }

  withInlineEditingAsDefault(useInlineEditingAsDefault: boolean) {
    this.useInlineEditingAsDefault = useInlineEditingAsDefault;
    return this;
  }

  addBlock() {
    const builder = new BlockListBlockBuilder(this);
    this.blockListBlockBuilder.push(builder);
    return builder;
  }

  getValues() {
    let values: any[] = [];

    // Add validationLimit alias and value if present
    if (this.minValue !== undefined || this.maxValue !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.minValue !== undefined ? this.minValue : '',
          max: this.maxValue !== undefined ? this.maxValue : ''
        }
      });
    }

    // Add maxPropertyWidth alias and value if present
    if (this.maxPropertyWidth !== undefined) {
      values.push({
        alias: 'maxPropertyWidth',
        value: this.maxPropertyWidth
      });
    }

    // Add useSingleBlockMode alias and value if present
    if (this.useSingleBlockMode !== undefined) {
      values.push({
        alias: 'useSingleBlockMode',
        value: this.useSingleBlockMode
      });
    }

    // Add useLiveEditing alias and value if present
    if (this.useLiveEditing !== undefined) {
      values.push({
        alias: 'useLiveEditing',
        value: this.useLiveEditing
      });
    }

    // Add useInlineEditingAsDefault alias and value if present
    if (this.useInlineEditingAsDefault !== undefined) {
      values.push({
        alias: 'useInlineEditingAsDefault',
        value: this.useInlineEditingAsDefault
      });
    }

    // Add blocks alias and value if present
    if (this.blockListBlockBuilder && this.blockListBlockBuilder.length > 0) {
      values.push({
        alias: 'blocks',
        value: this.blockListBlockBuilder.map(block => block.getValues())
      });
    }
    return values;
  }
}