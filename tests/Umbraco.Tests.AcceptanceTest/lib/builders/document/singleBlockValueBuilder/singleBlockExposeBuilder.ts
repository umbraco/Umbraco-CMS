import {BaseExposeBuilder} from '../baseBlockValueBuilder';
import {SingleBlockValueBuilder} from './singleBlockValueBuilder';

export class SingleBlockExposeBuilder extends BaseExposeBuilder {
  parentBuilder: SingleBlockValueBuilder;

  constructor(parentBuilder: SingleBlockValueBuilder) {
    super();
    this.parentBuilder = parentBuilder;
  }

  done() {
    return this.parentBuilder;
  }
}
