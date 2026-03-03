export class BaseExposeBuilder {
  contentKey: string;
  culture: string;
  segment: string;

  withContentKey(contentKey: string) {
    this.contentKey = contentKey;
    return this;
  }
  
  withCulture(culture: string) {
    this.culture = culture;
    return this;
  }

  withSegment(segment: string) {
    this.segment = segment;
    return this;
  }

  getValue() {
    return {
      contentKey: this.contentKey,
      culture: this.culture || null,
      segment: this.segment || null,
    };
  }
}