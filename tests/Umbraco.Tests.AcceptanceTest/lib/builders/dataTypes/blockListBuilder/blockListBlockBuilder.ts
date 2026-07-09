import {BlockListDataTypeBuilder} from '../blockListDataTypeBuilder';

export class BlockListBlockBuilder {
  parentBuilder: BlockListDataTypeBuilder;
  contentElementTypeKey: string;
  label: string;
  editorSize: string;
  settingsElementTypeKey: string;
  backgroundColor: string;
  iconColor: string;
  stylesheet: string[];
  forceHideContentEditorInOverlay: boolean;
  thumbnail: string;

  constructor(parentBuilder: BlockListDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
    this.stylesheet = [];
  }

  withContentElementTypeKey(contentElementTypeKey: string) {
    this.contentElementTypeKey = contentElementTypeKey;
    return this;
  }

  withLabel(label: string) {
    this.label = label;
    return this;
  }

  withEditorSize(editorSize: string) {
    this.editorSize = editorSize;
    return this;
  }

  withSettingsElementTypeKey(settingsElementTypeKey: string) {
    this.settingsElementTypeKey = settingsElementTypeKey;
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

  withStylesheet(stylesheet: string) {
    this.stylesheet.push(stylesheet);
    return this;
  }

  withHideContentEditor(hideContentEditor: boolean) {
    this.forceHideContentEditorInOverlay = hideContentEditor;
    return this;
  }

  withThumbnail(thumbnail: string) {
    this.thumbnail = thumbnail;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    let values: any = {};

    if (this.contentElementTypeKey) {
      values.contentElementTypeKey = this.contentElementTypeKey;
    }

    if (this.label) {
      values.label = this.label;
    }

    if (this.editorSize) {
      values.editorSize = this.editorSize;
    }

    if (this.settingsElementTypeKey) {
      values.settingsElementTypeKey = this.settingsElementTypeKey;
    }

    if (this.backgroundColor) {
      values.backgroundColor = this.backgroundColor;
    }

    if (this.iconColor) {
      values.iconColor = this.iconColor;
    }

    if (this.stylesheet.length > 0) {
      values.stylesheet = this.stylesheet;
    }

    if (this.forceHideContentEditorInOverlay) {
      values.forceHideContentEditorInOverlay = this.forceHideContentEditorInOverlay;
    }

    if (this.thumbnail) {
      values.thumbnail = this.thumbnail;
    }

    return values;
  }
}