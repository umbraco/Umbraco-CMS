import {DocumentTypeBuilder} from './documentTypeBuilder';
import {buildComposition} from '../../helpers/BuilderUtils';

export class DocumentTypeCompositionBuilder {
  parentBuilder: DocumentTypeBuilder;
  documentTypeId: string;
  compositionType: string;

  constructor(parentBuilder: DocumentTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withDocumentTypeId(documentTypeId: string) {
    this.documentTypeId = documentTypeId;
    return this;
  }

  withCompositionType(compositionType: string) {
    this.compositionType = compositionType;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return buildComposition('documentType', this.documentTypeId, this.compositionType);
  }
}