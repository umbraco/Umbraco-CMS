import {ElementBuilder} from './elementBuilder';

export class ElementValueBuilder {
  parentBuilder: ElementBuilder;
  culture: string;
  segment: string;
  alias: string;
  value: string | string[];
  editorAlias: string;
  entityType: string;

  constructor(parentBuilder: ElementBuilder) {
    this.parentBuilder = parentBuilder;
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
    }

    return {
      culture: this.culture || null,
      segment: this.segment || null,
      alias: this.alias || null,
      value: value || null,
      editorAlias: this.editorAlias || null,
      entityType: this.entityType || null
    }
  };
}