import {BaseContentDataValueBuilder} from '../baseBlockValueBuilder';
import {SingleBlockContentDataBuilder} from './singleBlockContentDataBuilder';

export class SingleBlockContentDataValueBuilder extends BaseContentDataValueBuilder {
  parentBuilder: SingleBlockContentDataBuilder;

  constructor(parentBuilder: SingleBlockContentDataBuilder) {
    super();
    this.parentBuilder = parentBuilder;
  }

  done() {
    return this.parentBuilder;
  }
}
