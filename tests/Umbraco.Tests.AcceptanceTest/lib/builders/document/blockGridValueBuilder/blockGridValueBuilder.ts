import {DocumentBlueprintsValueBuilder} from '../../documentBlueprints';
import {DocumentValueBuilder} from '../documentValueBuilder';
import {BlockGridContentDataBuilder} from './blockGridContentDataBuilder';
import {BlockGridExposeBuilder} from './blockGridExposeBuilder';
import {BlockGridLayoutBuilder} from './blockGridLayoutBuilder';

export class BlockGridValueBuilder {
  parentBuilder: DocumentValueBuilder | DocumentBlueprintsValueBuilder;
  blockGridContentDataBuilder: BlockGridContentDataBuilder[];
  blockGridExposeBuilder: BlockGridExposeBuilder[];
  blockGridLayoutBuilder: BlockGridLayoutBuilder[];
  blockGridSettingDataBuilder: [];

  constructor(parentBuilder: DocumentValueBuilder | DocumentBlueprintsValueBuilder) {
    this.parentBuilder = parentBuilder;
    this.blockGridContentDataBuilder = [];
    this.blockGridExposeBuilder = [];
    this.blockGridLayoutBuilder = [];
    this.blockGridSettingDataBuilder = [];
  }

  addContentData() {
    const builder = new BlockGridContentDataBuilder(this);
    this.blockGridContentDataBuilder.push(builder);
    return builder;
  }

  addExpose() {
    const builder = new BlockGridExposeBuilder(this);
    this.blockGridExposeBuilder.push(builder);
    return builder;
  }

  addLayout() {
    const builder = new BlockGridLayoutBuilder(this);
    this.blockGridLayoutBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    return {
      contentData: this.blockGridContentDataBuilder.map((builder) => {
        return builder.getValue();
      }),
      expose: this.blockGridExposeBuilder.map((builder) => {
        return builder.getValue();
      }),
      layout: {
        'Umbraco.BlockGrid': this.blockGridLayoutBuilder.map((builder) => {
          return builder.getValue();
        })
      },
      settingsData: []
    };
  }
}