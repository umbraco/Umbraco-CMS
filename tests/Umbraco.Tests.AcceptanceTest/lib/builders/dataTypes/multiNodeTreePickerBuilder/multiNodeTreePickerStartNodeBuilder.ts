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
      values.dynamicRoot = {
        originAlias: this.originAlias,
        querySteps: this.startNodeQueryStepsBuilder.map((builder) => builder.getValues()),
      };
    }

    return values;
  }
}