import {DataTypeBuilder} from './dataTypeBuilder';
import {MultiNodeTreePickerStartNodeBuilder} from './multiNodeTreePickerBuilder/multiNodeTreePickerStartNodeBuilder';

export class MultiNodeTreePickerDataTypeBuilder extends DataTypeBuilder {
  minNumber: number;
  maxNumber: number;
  ignoreUserStartNodes: boolean;
  filterIds: string;
  multiNodeTreePickerStartNodeBuilder: MultiNodeTreePickerStartNodeBuilder;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.MultiNodeTreePicker';
    this.editorUiAlias = 'Umb.PropertyEditorUi.ContentPicker';
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

  withFilterIds(filterIds: string) {
    this.filterIds = filterIds;
    return this;
  }

  addStartNode() {  
    const builder = new MultiNodeTreePickerStartNodeBuilder(this);
    this.multiNodeTreePickerStartNodeBuilder = builder;
    return builder;
  }

  getValues() {
    let values: any[] = [];

    if (this.minNumber !== undefined || this.maxNumber !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.minNumber,
          max: this.maxNumber
        }
      });
    }
    
    if (this.ignoreUserStartNodes !== undefined) {
      values.push({
        alias: 'ignoreUserStartNodes',
        value: this.ignoreUserStartNodes
      });
    }

    if (this.filterIds !== undefined) {
      values.push({
        alias: 'filter',
        value: this.filterIds
      });
    }

    if (this.multiNodeTreePickerStartNodeBuilder) {
      values.push({
        alias: 'startNode',
        value: this.multiNodeTreePickerStartNodeBuilder.getValues()
      });
    }

    return values;
  }
}