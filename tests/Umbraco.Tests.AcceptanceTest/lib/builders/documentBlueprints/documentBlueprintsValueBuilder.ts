import {BlockGridValueBuilder} from '../document/blockGridValueBuilder';
import {BlockListValueBuilder} from '../document/blockListValueBuilder';
import {DocumentBlueprintsBuilder} from './documentBlueprintsBuilder';

export class DocumentBlueprintsValueBuilder {
  parentBuilder: DocumentBlueprintsBuilder;
  culture: string | null;
  segment: string;
  alias: string;
  value: string | string[];
  editorAlias: string;
  entityType: string;
  blockGridValueBuilder: BlockGridValueBuilder;
  blockListValueBuilder: BlockListValueBuilder;

  constructor(parentBuilder: DocumentBlueprintsBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withCulture(culture: string | null) {
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

  withValue(value: string | string[]) {
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

  done() {
    return this.parentBuilder;
  }

  build() {
    let value: any = null;

    if (this.value != null) {
      value = this.value;
    } else {
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
      entityType: this.editorAlias !== undefined ? 'document-blueprint-property-value' : null
    };
  }
}