import {BaseExposeBuilder} from '../baseBlockValueBuilder';
import {BlockListValueBuilder} from './blockListValueBuilder';

export class BlockListExposeBuilder extends BaseExposeBuilder {
  parentBuilder: BlockListValueBuilder;

  constructor(parentBuilder: BlockListValueBuilder) {
    super();
    this.parentBuilder = parentBuilder;
  }

  done() {
    return this.parentBuilder;
  }
}