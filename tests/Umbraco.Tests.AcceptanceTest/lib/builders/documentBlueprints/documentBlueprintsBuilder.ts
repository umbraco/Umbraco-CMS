import {DocumentBlueprintsValueBuilder} from './documentBlueprintsValueBuilder';
import {DocumentBlueprintsVariantBuilder} from './documentBlueprintsVariantBuilder';
import {ensureIdExists} from '../../helpers/BuilderUtils';

export class DocumentBlueprintsBuilder {
  documentBlueprintsValueBuilder: DocumentBlueprintsValueBuilder[];
  documentBlueprintsVariantBuilder: DocumentBlueprintsVariantBuilder[];
  id: string;
  parentId: string;
  documentTypeId: string;

  constructor() {
    this.documentBlueprintsValueBuilder = [];
    this.documentBlueprintsVariantBuilder = [];
  }

  addValue() {
    const builder = new DocumentBlueprintsValueBuilder(this);
    this.documentBlueprintsValueBuilder.push(builder);
    return builder;
  }

  addVariant() {
    const builder = new DocumentBlueprintsVariantBuilder(this);
    this.documentBlueprintsVariantBuilder.push(builder);
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
      values: this.documentBlueprintsValueBuilder.map((builder) => {
        return builder.build();
      }),
      variants: this.documentBlueprintsVariantBuilder.map((builder) => {
        return builder.build();
      }),
      id: this.id,
      parent: this.parentId ? { id: this.parentId} : null,
      documentType: { id: this.documentTypeId},
    };
  }
}