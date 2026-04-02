import {BaseContentDataValueBuilder} from '../baseBlockValueBuilder';
import {BlockGridContentDataBuilder} from './blockGridContentDataBuilder';

export class BlockGridContentDataValueBuilder extends BaseContentDataValueBuilder {
  parentBuilder: BlockGridContentDataBuilder;

  constructor(parentBuilder: BlockGridContentDataBuilder) {
    super();
    this.parentBuilder = parentBuilder;
  }

  done() {
    return this.parentBuilder;
  }
}