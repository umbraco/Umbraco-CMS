import {MediaBuilder} from './mediaBuilder';
import {MediaValueDataBuilder} from './mediaValueDataBuilder';

export class MediaValueBuilder {
  parentBuilder: MediaBuilder;
  culture: string;
  segment: string;
  alias: string;
  editorAlias: string;
  entityType: string;
  value: string;
  mediaValueData: MediaValueDataBuilder;

  constructor(parentBuilder: MediaBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withCulture(culture: string) {
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

  withEditorAlias(editorAlias: string) {
    this.editorAlias = editorAlias;
    return this;
  }

  withEntityType(entityType: string) {
    this.entityType = entityType;
    return this;
  }

  withValue(value: string) {
    this.value = value;
    return this;
  }

  addValueData() {
    const builder = new MediaValueDataBuilder(this);
    this.mediaValueData = builder;
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    let value: any = null;

    if (this.value != null) {
      value = this.value;
    } else {
      value = this.mediaValueData.getValue();
    }

    return {
      culture: this.culture || null,
      segment: this.segment || null,
      alias: this.alias || null,
      editorAlias: this.editorAlias || null,
      value: value || null,
      entityType: this.editorAlias !== undefined ? 'media-property-value' : null
    };
  }
}