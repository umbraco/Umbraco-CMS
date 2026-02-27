import {DataTypeBuilder} from './dataTypeBuilder';

export class ContentPickerDataTypeBuilder extends DataTypeBuilder {
  showOpenButton: boolean;
  ignoreUserStartNodes: boolean;
  startNodeId: string;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.ContentPicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.DocumentPicker';
  }

  withShowOpenButton(showOpenButton: boolean) {
    this.showOpenButton = showOpenButton;
    return this;
  }

  withIgnoreUserStartNodes(ignoreUserStartNodes: boolean) {
    this.ignoreUserStartNodes = ignoreUserStartNodes;
    return this;
  }

  withStartNodeId(startNodeId: string) {
    this.startNodeId = startNodeId;
    return this;
  }

  getValues() {
    let values: any[] = [];

    if (this.showOpenButton !== undefined) {
      values.push({
        alias: 'showOpenButton',
        value: this.showOpenButton
      });
    }

    if (this.ignoreUserStartNodes !== undefined) {
      values.push({
        alias: 'ignoreUserStartNodes',
        value: this.ignoreUserStartNodes
      });
    }

    if (this.startNodeId !== undefined) {
      values.push({
        alias: 'startNodeId',
        value: this.startNodeId
      });
    }
    return values;
  }
}