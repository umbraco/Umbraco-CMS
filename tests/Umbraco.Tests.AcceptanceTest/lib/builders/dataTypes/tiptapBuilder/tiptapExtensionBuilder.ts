import {TiptapDataTypeBuilder} from '../tiptapDataTypeBuilder';

export class TiptapExtensionBuilder {
  parentBuilder: TiptapDataTypeBuilder;
  extensionValues: {[key: string]: boolean} = {};

  constructor(parentBuilder: TiptapDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withRichTextEssentials(value: boolean) {
    this.extensionValues['Umb.Tiptap.RichTextEssentials'] = value;
    return this;
  }

  withEmbed(value: boolean) {
    this.extensionValues['Umb.Tiptap.Embed'] = value;
    return this;
  }

  withFigure(value: boolean) {
    this.extensionValues['Umb.Tiptap.Figure'] = value;
    return this;
  }

  withImage(value: boolean) {
    this.extensionValues['Umb.Tiptap.Image'] = value;
    return this;
  }

  withLink(value: boolean) {
    this.extensionValues['Umb.Tiptap.Link'] = value;
    return this;
  }

  withMediaUpload(value: boolean) {
    this.extensionValues['Umb.Tiptap.MediaUpload'] = value;
    return this;
  }

  withSubscript(value: boolean) {
    this.extensionValues['Umb.Tiptap.Subscript'] = value;
    return this;
  }

  withSuperscript(value: boolean) {
    this.extensionValues['Umb.Tiptap.Superscript'] = value;
    return this;
  }
  
  withTable(value: boolean) {
    this.extensionValues['Umb.Tiptap.Table'] = value;
    return this;
  }

  withTextAlign(value: boolean) {
    this.extensionValues['Umb.Tiptap.TextAlign'] = value;
    return this;
  }

  withUnderline(value: boolean) {
    this.extensionValues['Umb.Tiptap.Underline'] = value;
    return this;
  }

  withBlock(value: boolean) {
    this.extensionValues['Umb.Tiptap.Block'] = value;
    return this;
  }

  withWordCount(value: boolean) {
    this.extensionValues['Umb.Tiptap.WordCount'] = value;
    return this;
  }

  withTextDirection(value: boolean) {
    this.extensionValues['Umb.Tiptap.TextDirection'] = value;
    return this;
  }

  withTextIndent(value: boolean) {
    this.extensionValues['Umb.Tiptap.TextIndent'] = value;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return Object.keys(this.extensionValues).filter(key => this.extensionValues[key]);
  }
}