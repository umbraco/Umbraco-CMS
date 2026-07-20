import {ApprovedColorItemBuilder} from './approvedColorBuilder';
import {DataTypeBuilder} from './dataTypeBuilder';

export class ApprovedColorDataTypeBuilder extends DataTypeBuilder {
  useLabel: boolean;
  approvedColorItemBuilder: ApprovedColorItemBuilder[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.ColorPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.ColorPicker';
    this.approvedColorItemBuilder = [];
  }

  withUseLabel(useLabel: boolean) {
    this.useLabel = useLabel;
    return this;
  }

  addItem() {
    const builder = new ApprovedColorItemBuilder(this);
    this.approvedColorItemBuilder.push(builder);
    return builder;
  }

  getValues() {
    let values: any = [];
    values.push({
      alias: 'useLabel',
      value: this.useLabel || false
    });
    if (this.approvedColorItemBuilder && this.approvedColorItemBuilder.length > 0) {
      values.push({
        alias: 'items',
        value: this.approvedColorItemBuilder.map((builder) => {
          return builder.getValues();
        })
      });
    }
    return values;
  }
}