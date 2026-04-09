import {DocumentValueBuilder} from './documentValueBuilder';
import {DocumentVariantBuilder} from './documentVariantBuilder';
import {ensureIdExists} from '../../helpers/BuilderUtils';

export class DocumentBuilder {
  documentValueBuilder: DocumentValueBuilder[];
  documentVariantBuilder: DocumentVariantBuilder[];
  id: string;
  parentId: string;
  documentTypeId: string;
  templateId: string;

  constructor() {
    this.documentValueBuilder = [];
    this.documentVariantBuilder = [];
  }

  addValue() {
    const builder = new DocumentValueBuilder(this);
    this.documentValueBuilder.push(builder);
    return builder;
  }

  addVariant() {
    const builder = new DocumentVariantBuilder(this);
    this.documentVariantBuilder.push(builder);
    return builder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  withParentId(parentId: string) {
    this.parentId = parentId;
    return this;
  }

  withDocumentTypeId(documentTypeId: string) {
    this.documentTypeId = documentTypeId;
    return this;
  }

  withTemplateId(templateId: string) {
    this.templateId = templateId;
    return this;
  }

  build() {
    this.id = ensureIdExists(this.id);

    return {
      values: this.documentValueBuilder.map((builder) => {
        return builder.build();
      }),
      variants: this.documentVariantBuilder.map((builder) => {
        return builder.build();
      }),
      id: this.id,
      parent: this.parentId ? { id: this.parentId} : null,
      documentType: { id: this.documentTypeId},
      template: this.templateId ? { id: this.templateId} : null,
    };
  }
}