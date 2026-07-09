import {MultiNodeTreePickerDataTypeBuilder} from '../multiNodeTreePickerDataTypeBuilder';
import {StartNodeQueryStepsBuilder} from './startNodeQueryStepsBuilder';

export class MultiNodeTreePickerStartNodeBuilder {
  parentBuilder: MultiNodeTreePickerDataTypeBuilder;
  type: string;
  originAlias: string;
  startNodeQueryStepsBuilder: StartNodeQueryStepsBuilder[];

  constructor(parentBuilder: MultiNodeTreePickerDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
    this.startNodeQueryStepsBuilder = [];
  }

  withType(type: string) {
    this.type = type;
    return this;
  }

  withOriginAlias(originAlias: string) {
    this.originAlias = originAlias;
    return this;
  }

  addQueryStep() {
    const builder = new StartNodeQueryStepsBuilder(this);
    this.startNodeQueryStepsBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    let values: any = {};

    if (this.type) {
      values.type = this.type;
    }

    if (this.originAlias) {
      values.originAlias = this.originAlias;
    }  

    if (this.startNodeQueryStepsBuilder && this.startNodeQueryStepsBuilder.length > 0) {
      values.startNodeQuerySteps = this.startNodeQueryStepsBuilder.map((builder) => {
        return builder.getValues();
      }); 
    }

    return values;
  }
}