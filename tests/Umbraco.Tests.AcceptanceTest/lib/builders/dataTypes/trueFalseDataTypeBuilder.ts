import {DataTypeBuilder} from './dataTypeBuilder';

export class TrueFalseDataTypeBuilder extends DataTypeBuilder {
  isDefault: boolean;
  showLabels: boolean;
  labelOn: string;
  labelOff: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.TrueFalse';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Toggle';
  }

  withIsDefault(isDefault: boolean) {
    this.isDefault = isDefault;
    return this;
  }

  withShowLabels(showLabels: boolean) {
    this.showLabels = showLabels;
    return this;
  }

  withLabelOn(labelOn: string) {
    this.labelOn = labelOn;
    return this;
  }

  withLabelOff(labelOff: string) {
    this.labelOff = labelOff;
    return this;
  }

  getValues() {
    let values: any = [];
    if (this.isDefault !== undefined) {
      values.push({
        alias: 'default',
        value: this.isDefault
      });
    }
    if (this.showLabels !== undefined) {
      values.push({
        alias: 'showLabels',
        value: this.showLabels
      });
    }
    if (this.labelOn !== undefined) {
      values.push({
        alias: 'labelOn',
        value: this.labelOn
      });
    }
    if (this.labelOff !== undefined) {
      values.push({
        alias: 'labelOff',
        value: this.labelOff
      });
    }
    return values;
  }
}