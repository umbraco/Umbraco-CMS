import {TiptapDataTypeBuilder} from '../tiptapDataTypeBuilder';

export class TiptapBlockBuilder {
  parentBuilder: TiptapDataTypeBuilder;
  contentElementTypeKey: string;
  displayInline: boolean;
  backgroundColor: string;
  iconColor: string;
  thumbnail: string;
  editorSize: string;
  label: string;
  settingsElementTypeKey: string;

  constructor(parentBuilder: TiptapDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withContentElementTypeKey(contentElementTypeKey: string) {
    this.contentElementTypeKey = contentElementTypeKey;
    return this;
  }

  withDisplayInline(displayInline: boolean) {
    this.displayInline = displayInline;
    return this;
  }

  withBackgroundColor(backgroundColor: string) {
    this.backgroundColor = backgroundColor;
    return this;
  }

  withIconColor(iconColor: string) {
    this.iconColor = iconColor;
    return this;
  }

  withThumbnail(thumbnail: string) {
    this.thumbnail = thumbnail;
    return this;
  }

  withEditorSize(editorSize: string) {
    this.editorSize = editorSize;
    return this;
  }

  withLabel(label: string) {
    this.label = label;
    return this;
  }

  withSettingsElementTypeKey(settingsElementTypeKey: string) {
    this.settingsElementTypeKey = settingsElementTypeKey;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    let values = {};

    if (this.contentElementTypeKey !== '') {
      values['contentElementTypeKey'] = this.contentElementTypeKey;
    }

    if (this.displayInline) {
      values['displayInline'] = this.displayInline;
    }

    if (this.backgroundColor !== '') {
      values['backgroundColor'] = this.backgroundColor;
    }

    if (this.iconColor !== '') {
      values['iconColor'] = this.iconColor;
    }

    if (this.thumbnail !== '') {
      values['thumbnail'] = this.thumbnail;
    }

    if (this.editorSize !== '') {
      values['editorSize'] = this.editorSize;
    }

    if (this.label !== '') {
      values['label'] = this.label;
    }

    if (this.settingsElementTypeKey !== '') {
      values['settingsElementTypeKey'] = this.settingsElementTypeKey;
    }

    return values;
  };
}