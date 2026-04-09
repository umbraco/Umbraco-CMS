import {BaseContentDataValueBuilder} from '../baseBlockValueBuilder';
import {BlockListContentDataBuilder} from './blockListContentDataBuilder';

export class BlockListContentDataValueBuilder extends BaseContentDataValueBuilder{
  parentBuilder: BlockListContentDataBuilder;

  constructor(parentBuilder: BlockListContentDataBuilder) {
    super();
    this.parentBuilder = parentBuilder;
  }

  done() {
    return this.parentBuilder;
  }
}