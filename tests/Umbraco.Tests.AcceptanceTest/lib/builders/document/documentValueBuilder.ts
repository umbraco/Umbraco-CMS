import {DocumentBuilder} from './documentBuilder';
import {MediaPickerValueBuilder} from './mediaPickerValueBuilder';
import {URLPickerValueBuilder} from './urlPickerValueBuilder';
import {ImageCropperValueBuilder} from './imageCropperValueBuilder';
import {BlockGridValueBuilder} from './blockGridValueBuilder';
import {BlockListValueBuilder} from './blockListValueBuilder';

export class DocumentValueBuilder {
  parentBuilder: DocumentBuilder;
  culture: string;
  segment: string;
  alias: string;
  value: string | string[];
  editorAlias: string;
  entityType: string;
  mediaPickerValueBuilder: MediaPickerValueBuilder[];
  urlPickerValueBuilder: URLPickerValueBuilder[];
  imageCropperValueBuilder: ImageCropperValueBuilder;
  temporaryFileId: string;
  blockGridValueBuilder: BlockGridValueBuilder;
  blockListValueBuilder: BlockListValueBuilder;

  constructor(parentBuilder: DocumentBuilder) {
    this.parentBuilder = parentBuilder;
    this.mediaPickerValueBuilder = [];
    this.urlPickerValueBuilder = [];
  }

  withCulture(culture: any) {
    this.culture = culture;
    return this;
  }

  withSegment(segment: string) {
    this.segment = segment;
    return this;
  }

  withAlias(alias: string) {
    this.alias = alias;
    return this;
  }

  withValue(value: any) {
    this.value = value;
    return this;
  }

  withTemporaryFileId(temporaryFileId: string) {
    this.temporaryFileId = temporaryFileId;
    return this;
  }

  addMediaPickerValue() {
    const builder = new MediaPickerValueBuilder(this);
    this.mediaPickerValueBuilder.push(builder);
    return builder;
  }

  addURLPickerValue() {
    const builder = new URLPickerValueBuilder(this);
    this.urlPickerValueBuilder.push(builder);
    return builder;
  }

  addImageCropperValue() {
    const builder = new ImageCropperValueBuilder(this);
    this.imageCropperValueBuilder = builder;
    return builder;
  }

  addBlockGridValue() {
    const builder = new BlockGridValueBuilder(this);
    this.blockGridValueBuilder = builder;
    return builder;
  }

  addBlockListValue() {
    const builder = new BlockListValueBuilder(this);
    this.blockListValueBuilder = builder;
    return builder;
  }

  withEditorAlias(editorAlias: string) {
    this.editorAlias = editorAlias;
    return this;
  }

  withEntityType(entityType: string) {
    this.entityType = entityType;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    let value: any = null;

    if (this.value != null) {
      value = this.value;
    } else {
      if (this.mediaPickerValueBuilder && this.mediaPickerValueBuilder.length > 0) {
        value = this.mediaPickerValueBuilder.map((builder) => {
          return builder.getValue();
        })
      }
      if (this.urlPickerValueBuilder && this.urlPickerValueBuilder.length > 0) {
        value = this.urlPickerValueBuilder.map((builder) => {
          return builder.getValue();
        })
      }
      if (this.imageCropperValueBuilder !== undefined) {
        value = this.imageCropperValueBuilder.getValue();
      }
      if (this.temporaryFileId !== undefined) {
        value = {temporaryFileId: this.temporaryFileId};
      }
      if (this.blockGridValueBuilder !== undefined) {
        value = this.blockGridValueBuilder.getValue();
      }
      if (this.blockListValueBuilder !== undefined) {
        value = this.blockListValueBuilder.getValue();
      }
    }

    return {
      culture: this.culture || null,
      segment: this.segment || null,
      alias: this.alias || null,
      value: value || null,
      editorAlias: this.editorAlias || null,
      entityType: this.editorAlias !== undefined ? 'document-property-value' : null
    }
  };
}