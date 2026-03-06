import {BaseExposeBuilder} from '../baseBlockValueBuilder';
import {BlockGridValueBuilder} from './blockGridValueBuilder';

export class BlockGridExposeBuilder extends BaseExposeBuilder {
  parentBuilder: BlockGridValueBuilder;

  constructor(parentBuilder: BlockGridValueBuilder) {
    super();
    this.parentBuilder = parentBuilder;
  }

  done() {
    return this.parentBuilder;
  }
}