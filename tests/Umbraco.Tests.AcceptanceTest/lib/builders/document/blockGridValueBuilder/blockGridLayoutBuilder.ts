import {BlockGridAreaBuilder} from './blockGridAreaBuilder';

export class BlockGridLayoutBuilder {
  parentBuilder;
  columnSpan: number;
  contentKey: string;
  contentUdi: string;
  rowSpan: number;
  settingsKey: string;
  settingsUdi: string;
  areasBuilder: BlockGridAreaBuilder[];

  constructor(parentBuilder) {
    this.parentBuilder = parentBuilder;
    this.areasBuilder = [];
  }

  withColumnSpan(columnSpan: number) {
    this.columnSpan = columnSpan;
    return this;
  }

  withContentKey(contentKey: string) {
    this.contentKey = contentKey;
    return this;
  }

  withContentUdi(contentUdi: string) {
    this.contentUdi = contentUdi;
    return this;
  }

  withRowSpan(rowSpan: number) {
    this.rowSpan = rowSpan;
    return this;
  }

  withSettingsUdi(settingsUdi: string) {
    this.settingsUdi = settingsUdi;
    return this;
  }

  withSettingsKey(settingsKey: string) {
    this.settingsKey = settingsKey;
    return this;
  }

  addArea() {
    const builder = new BlockGridAreaBuilder(this);
    this.areasBuilder.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    return {
      $type: 'BlockGridLayoutItem',
      columnSpan: this.columnSpan || 12,
      contentKey: this.contentKey,
      areas: this.areasBuilder.map((builder) => {
        return builder.getValue();
      }),
      contentUdi: this.contentUdi || null,
      rowSpan: this.rowSpan || 1,
      settingsKey: this.settingsKey || null,
      settingsUdi: this.settingsUdi || null
    };
  }
}