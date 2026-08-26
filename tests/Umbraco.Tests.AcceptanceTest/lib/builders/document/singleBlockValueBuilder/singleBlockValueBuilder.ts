import {DocumentBlueprintsValueBuilder} from '../../documentBlueprints';
import {DocumentValueBuilder} from '../documentValueBuilder';
import {SingleBlockContentDataBuilder} from './singleBlockContentDataBuilder';
import {SingleBlockExposeBuilder} from './singleBlockExposeBuilder';
import {SingleBlockLayoutBuilder} from './singleBlockLayoutBuilder';

export class SingleBlockValueBuilder {
  parentBuilder: DocumentValueBuilder | DocumentBlueprintsValueBuilder;
  singleBlockContentDataBuilder: SingleBlockContentDataBuilder[];
  singleBlockExposeBuilder: SingleBlockExposeBuilder[];
  singleBlockLayoutBuilder: SingleBlockLayoutBuilder[];
  singleBlockSettingDataBuilder: [];

  constructor(parentBuilder: DocumentValueBuilder | DocumentBlueprintsValueBuilder) {
    this.parentBuilder = parentBuilder;
    this.singleBlockContentDataBuilder = [];
    this.singleBlockExposeBuilder = [];
    this.singleBlockLayoutBuilder = [];
    this.singleBlockSettingDataBuilder = [];
  }

  addContentData() {
    const builder = new SingleBlockContentDataBuilder(this);
    this.singleBlockContentDataBuilder.push(builder);
    return builder;
  }

  addExpose() {
    const builder = new SingleBlockExposeBuilder(this);
    this.singleBlockExposeBuilder.push(builder);
    return builder;
  }

  addLayout() {
    const builder = new SingleBlockLayoutBuilder(this);
    this.singleBlockLayoutBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    return {
      contentData: this.singleBlockContentDataBuilder.map((builder) => {
        return builder.getValue();
      }),
      expose: this.singleBlockExposeBuilder.map((builder) => {
        return builder.getValue();
      }),
      layout: {
        // The single block editor stores its layout as a one-item array under the 'Umbraco.SingleBlock' key
        // (see SingleBlockValue.cs), unlike block list/grid which allow multiple layout items.
        'Umbraco.SingleBlock': this.singleBlockLayoutBuilder.map((builder) => {
          return builder.getValue();
        })
      },
      settingsData: []
    };
  }
}
