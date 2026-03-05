import {DocumentBuilder} from './documentBuilder';

export class DocumentVariantBuilder {
  parentBuilder: DocumentBuilder;
  culture: string;
  segment: string;
  name: string;

  constructor(parentBuilder: DocumentBuilder) {
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

  build() {
    return {
      culture: this.culture || null,
      segment: this.segment || null,
      name: this.name || null,
    };
  }
}