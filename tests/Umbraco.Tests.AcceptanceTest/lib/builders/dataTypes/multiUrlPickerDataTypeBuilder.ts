import {DataTypeBuilder} from './dataTypeBuilder';

export class MultiUrlPickerDataTypeBuilder extends DataTypeBuilder {
  minNumber: number;
  maxNumber: number;
  ignoreUserStartNodes: boolean;
  overlaySize: string;
  hideAnchor: boolean;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.MultiUrlPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MultiUrlPicker';
  }

  withMinNumber(minNumber: number) {
    this.minNumber = minNumber;
    return this;
  }
  
  withMaxNumber(maxNumber: number) {
    this.maxNumber = maxNumber;
    return this;
  }
 
  withIgnoreUserStartNodes(ignoreUserStartNodes: boolean) {
    this.ignoreUserStartNodes = ignoreUserStartNodes;
    return this;
  }

  withOverlaySize(overlaySize: string) {
    this.overlaySize = overlaySize;
    return this;
  }

  withHideAnchor(hideAnchor: boolean) {
    this.hideAnchor = hideAnchor;
    return this;
  }

  getValues() {
    let values: any = [];
    if (this.minNumber !== undefined) {
      values.push({
        alias: 'minNumber',
        value: this.minNumber
      });
    }
    if (this.maxNumber !== undefined) {
      values.push({
        alias: 'maxNumber',
        value: this.maxNumber
      });
    }
    if (this.ignoreUserStartNodes !== undefined) {
      values.push({
        alias: 'ignoreUserStartNodes',
        value: this.ignoreUserStartNodes
      });
    }
    if (this.overlaySize !== undefined) {
      values.push({
        alias: 'overlaySize',
        value: this.overlaySize
      });
    }
    if (this.hideAnchor !== undefined) {
      values.push({
        alias: 'hideAnchor',
        value: this.hideAnchor
      });
    }
    return values;
  }
}