/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { DocumentTypeCompositionModel } from './DocumentTypeCompositionModel';
import type { DocumentTypeSortModel } from './DocumentTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { UpdateContentTypeForDocumentTypeRequestModel } from './UpdateContentTypeForDocumentTypeRequestModel';

export type UpdateDocumentTypeRequestModel = (UpdateContentTypeForDocumentTypeRequestModel & {
    allowedTemplates: Array<ReferenceByIdModel>;
    defaultTemplate?: ReferenceByIdModel | null;
    cleanup: ContentTypeCleanupModel;
    allowedDocumentTypes: Array<DocumentTypeSortModel>;
    compositions: Array<DocumentTypeCompositionModel>;
});

