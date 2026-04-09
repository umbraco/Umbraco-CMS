import {DocumentBlueprintsValueBuilder} from '../../documentBlueprints';
import {DocumentValueBuilder} from '../documentValueBuilder';
import {BlockListContentDataBuilder} from './blockListContentDataBuilder';
import {BlockListExposeBuilder} from './blockListExposeBuilder';
import {BlockListLayoutBuilder} from './blockListLayoutBuilder';

export class BlockListValueBuilder {
  parentBuilder: DocumentValueBuilder | DocumentBlueprintsValueBuilder;
  blockListContentDataBuilder: BlockListContentDataBuilder[];
  blockListExposeBuilder: BlockListExposeBuilder[];
  blockListLayoutBuilder: BlockListLayoutBuilder[];
  blockListSettingDataBuilder: [];

  constructor(parentBuilder: DocumentValueBuilder | DocumentBlueprintsValueBuilder) {
    this.parentBuilder = parentBuilder;
    this.blockListContentDataBuilder = [];
    this.blockListExposeBuilder = [];
    this.blockListLayoutBuilder = [];
    this.blockListSettingDataBuilder = [];
  }

  addContentData() {
    const builder = new BlockListContentDataBuilder(this);
    this.blockListContentDataBuilder.push(builder);
    return builder;
  }

  addExpose() {
    const builder = new BlockListExposeBuilder(this);
    this.blockListExposeBuilder.push(builder);
    return builder;
  }

  addLayout() {
    const builder = new BlockListLayoutBuilder(this);
    this.blockListLayoutBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    return {
      contentData: this.blockListContentDataBuilder.map((builder) => {
        return builder.getValue();
      }),
      expose: this.blockListExposeBuilder.map((builder) => {
        return builder.getValue();
      }),
      layout: {
        'Umbraco.BlockList': this.blockListLayoutBuilder.map((builder) => {
          return builder.getValue();
        })
      },
      settingsData: []
    };
  }
}