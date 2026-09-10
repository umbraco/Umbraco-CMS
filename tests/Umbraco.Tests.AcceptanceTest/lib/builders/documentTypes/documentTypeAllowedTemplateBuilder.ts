import {DocumentTypeBuilder} from './documentTypeBuilder';
import {OptionalEntityReference} from '../types';

export class DocumentTypeAllowedTemplateBuilder {
  parentBuilder: DocumentTypeBuilder;
  id: string;

  constructor(parentBuilder: DocumentTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build(): OptionalEntityReference {
    return {
      id: this.id || null
    };
  }
}