import {DocumentValueBuilder} from '../documentValueBuilder';
import {SingleBlockContentDataBuilder} from './singleBlockContentDataBuilder';
import {SingleBlockExposeBuilder} from './singleBlockExposeBuilder';
import {SingleBlockLayoutBuilder} from './singleBlockLayoutBuilder';

export class SingleBlockValueBuilder {
  parentBuilder: DocumentValueBuilder;
  singleBlockContentDataBuilder: SingleBlockContentDataBuilder[];
  singleBlockExposeBuilder: SingleBlockExposeBuilder[];
  singleBlockLayoutBuilder: SingleBlockLayoutBuilder[];

  constructor(parentBuilder: DocumentValueBuilder) {
    this.parentBuilder = parentBuilder;
    this.singleBlockContentDataBuilder = [];
    this.singleBlockExposeBuilder = [];
    this.singleBlockLayoutBuilder = [];
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
        'Umbraco.SingleBlock': this.singleBlockLayoutBuilder.map((builder) => {
          return builder.getValue();
        })
      },
      settingsData: []
    };
  }
}
