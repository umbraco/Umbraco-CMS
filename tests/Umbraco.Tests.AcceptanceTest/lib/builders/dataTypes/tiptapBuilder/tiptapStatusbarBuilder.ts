import {TiptapDataTypeBuilder} from '../tiptapDataTypeBuilder';

export class TiptapStatusbarBuilder {
  parentBuilder: TiptapDataTypeBuilder;
  statusbarValues: { [key: string]: boolean } = {};

  constructor(parentBuilder: TiptapDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
    this.statusbarValues = {};
  }

  withWordCount(value: boolean) {
    this.statusbarValues['Umb.Tiptap.Statusbar.WordCount'] = value;
    return this;
  }

  withElementPath(value: boolean) {
    this.statusbarValues['Umb.Tiptap.Statusbar.ElementPath'] = value;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return Object.keys(this.statusbarValues).filter(key => this.statusbarValues[key]);
  }
}