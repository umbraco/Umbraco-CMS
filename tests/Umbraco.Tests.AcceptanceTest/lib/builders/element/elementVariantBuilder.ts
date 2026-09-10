import {ElementBuilder} from './elementBuilder';
import {EntityVariant} from '../types';

export class ElementVariantBuilder {
  parentBuilder: ElementBuilder;
  culture: string;
  segment: string;
  name: string;

  constructor(parentBuilder: ElementBuilder) {
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