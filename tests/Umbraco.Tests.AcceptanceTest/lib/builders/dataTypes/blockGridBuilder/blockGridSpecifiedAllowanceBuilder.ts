import {BlockGridAreaBuilder} from './blockGridAreaBuilder';

export class BlockGridSpecifiedAllowanceBuilder {
  parentBuilder: BlockGridAreaBuilder;
  minAllowed: number;
  maxAllowed: number;
  elementTypeKey: string;
  groupKey: string;

  constructor(parentBuilder: BlockGridAreaBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withElementTypeKey(elementTypeKey: string) {
    this.elementTypeKey = elementTypeKey;
    return this;
  }

  withGroupName(groupName: string) {
    this.groupKey = this.parentBuilder.parentBuilder.parentBuilder.getBlockGroupGUID(groupName);
    return this;
  }

  withMinAllowed(minAllowed: number) {
    this.minAllowed = minAllowed;
    return this;
  }

  withMaxAllowed(maxAllowed: number) {
    this.maxAllowed = maxAllowed;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    let values: any[] = [];

    if (this.elementTypeKey !== undefined) {
      values.push({elementTypeKey: this.elementTypeKey});
    }

    if (this.groupKey !== undefined) {
      values.push({groupKey: this.groupKey});
    }

    if (this.minAllowed !== undefined) {
      values.push({minAllowed: this.minAllowed});
    }

    if (this.maxAllowed !== undefined) {
      values.push({maxAllowed: this.maxAllowed});
    }

    return values;
  }
}