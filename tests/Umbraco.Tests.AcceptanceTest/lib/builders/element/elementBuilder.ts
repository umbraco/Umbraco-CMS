import {ElementValueBuilder} from './elementValueBuilder';
import {ElementVariantBuilder} from './elementVariantBuilder';
import {ensureIdExists} from '../../helpers/BuilderUtils';

export class ElementBuilder {
  elementValueBuilder: ElementValueBuilder[];
  elementVariantBuilder: ElementVariantBuilder[];
  id: string;
  parentId: string;
  documentTypeId: string;

  constructor() {
    this.elementValueBuilder = [];
    this.elementVariantBuilder = [];
  }

  addValue() {
    const builder = new ElementValueBuilder(this);
    this.elementValueBuilder.push(builder);
    return builder;
  }

  addVariant() {
    const builder = new ElementVariantBuilder(this);
    this.elementVariantBuilder.push(builder);
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

  build() {
    this.id = ensureIdExists(this.id);

    return {
      values: this.elementValueBuilder.map((builder) => {
        return builder.build();
      }),
      variants: this.elementVariantBuilder.map((builder) => {
        return builder.build();
      }),
      id: this.id,
      parent: this.parentId ? {id: this.parentId} : null,
      documentType: {id: this.documentTypeId}
    };
  }
}
