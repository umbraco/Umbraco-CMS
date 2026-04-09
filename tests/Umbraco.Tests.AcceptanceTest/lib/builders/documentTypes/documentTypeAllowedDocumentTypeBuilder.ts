import {DocumentTypeBuilder} from './documentTypeBuilder';

export class DocumentTypeAllowedDocumentTypeBuilder {
  parentBuilder: DocumentTypeBuilder;
  id: string;
  sortOrder: number;

  constructor(parentBuilder: DocumentTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  withSortOrder(sortOrder: number) {
    this.sortOrder = sortOrder;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return {
      documentType: {
        id: this.id || null
      },
      sortOrder: this.sortOrder || 0
    };
  }
}