import {BlockGridDataTypeBuilder} from '../blockGridDataTypeBuilder';
import {BlockGridAreaBuilder} from './blockGridAreaBuilder';

export class BlockGridBlockBuilder {
  parentBuilder: BlockGridDataTypeBuilder;
  contentElementTypeKey: string;
  label: string;
  settingsElementTypeKey: string;
  allowAtRoot: boolean;
  allowInAreas: boolean;
  columnSpanOptions: { columnSpan: number }[];
  rowMinSpan: number;
  rowMaxSpan: number;
  editorSize: string;
  inlineEditing: boolean;
  hideContentEditor: boolean;
  backgroundColor: string;
  iconColor: string;
  thumbnail: string;
  stylesheet: string;
  view: string;
  groupKey: string;
  blockGridAreasBuilder: BlockGridAreaBuilder[];
  areaGridColumns: number;

  constructor(parentBuilder: BlockGridDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
    this.blockGridAreasBuilder = [];
    this.columnSpanOptions = [];
  }

  withContentElementTypeKey(contentElementTypeKey: string) {
    this.contentElementTypeKey = contentElementTypeKey;
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

  withAllowAtRoot(allowAtRoot: boolean) {
    this.allowAtRoot = allowAtRoot;
    return this;
  }

  withAllowInAreas(allowInAreas: boolean) {
    this.allowInAreas = allowInAreas;
    return this;
  }

  addColumnSpanOptions(columnSpan: number) {
    this.columnSpanOptions.push({columnSpan});
    return this;
  }

  withMinRowSpan(minRowSpan: number) {
    this.rowMinSpan = minRowSpan;
    return this;
  }

  withMaxRowSpan(maxRowSpan: number) {
    this.rowMaxSpan = maxRowSpan;
    return this;
  }

  withEditorSize(editorSize: string) {
    this.editorSize = editorSize;
    return this;
  }

  withInlineEditing(inlineEditing: boolean) {
    this.inlineEditing = inlineEditing;
    return this;
  }

  withHideContentEditor(hideContentEditor: boolean) {
    this.hideContentEditor = hideContentEditor;
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

  withStylesheet(stylesheet: string) {
    this.stylesheet = stylesheet;
    return this;
  }

  withView(view: string) {
    this.view = view;
    return this;
  }

  withGroupName(groupName: string) {
    this.groupKey = this.parentBuilder.getBlockGroupGUID(groupName);
    return this;
  }

  addArea() {
    const builder = new BlockGridAreaBuilder(this);
    this.blockGridAreasBuilder.push(builder);
    return builder;
  }

  withAreaGridColumns(areaGridColumns: number) {
    this.areaGridColumns = areaGridColumns;
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

    if (this.settingsElementTypeKey) {
      values.settingsElementTypeKey = this.settingsElementTypeKey;
    }

    if (this.allowAtRoot) {
      values.allowAtRoot = this.allowAtRoot;
    }

    if (this.allowInAreas) {
      values.allowInAreas = this.allowInAreas;
    }

    if (this.columnSpanOptions && this.columnSpanOptions.length > 0) {
      values.columnSpanOptions = this.columnSpanOptions;
    }

    if (this.rowMinSpan) {
      values.rowMinSpan = this.rowMinSpan;
    }

    if (this.rowMaxSpan) {
      values.rowMaxSpan = this.rowMaxSpan;
    }

    if (this.editorSize) {
      values.editorSize = this.editorSize;
    }

    if (this.inlineEditing) {
      values.inlineEditing = this.inlineEditing;
    }

    if (this.hideContentEditor) {
      values.hideContentEditor = this.hideContentEditor;
    }

    if (this.backgroundColor) {
      values.backgroundColor = this.backgroundColor;
    }

    if (this.iconColor) {
      values.iconColor = this.iconColor;
    }

    if (this.thumbnail) {
      values.thumbnail = this.thumbnail;
    }

    if (this.stylesheet) {
      values.stylesheet = this.stylesheet;
    }

    if (this.view) {
      values.view = this.view;
    }

    if (this.groupKey) {
      values.groupKey = this.groupKey;
    }

    if (this.blockGridAreasBuilder && this.blockGridAreasBuilder.length > 0) {
      values.areas = this.blockGridAreasBuilder.map((builder) => builder.getValues());
    }

    if (this.areaGridColumns) {
      values.areaGridColumns = this.areaGridColumns;
    }

    return values;
  }
}