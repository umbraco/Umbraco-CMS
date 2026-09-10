import {DocumentBlueprintsBuilder} from './documentBlueprintsBuilder';
import {EntityVariant} from '../types';

export class DocumentBlueprintsVariantBuilder {
  parentBuilder: DocumentBlueprintsBuilder;
  culture: string;
  segment: string;
  name: string;

  constructor(parentBuilder: DocumentBlueprintsBuilder) {
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

  withName(name: string) {
    this.name = name;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build(): EntityVariant {
    return {
      culture: this.culture || null,
      segment: this.segment || null,
      name: this.name || null,
    };
  }
}