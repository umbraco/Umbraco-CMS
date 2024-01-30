/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { CreateContentTypeForDocumentTypeRequestModel } from './CreateContentTypeForDocumentTypeRequestModel';
import type { DocumentTypeCompositionModel } from './DocumentTypeCompositionModel';
import type { DocumentTypeSortModel } from './DocumentTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateDocumentTypeRequestModel = (CreateContentTypeForDocumentTypeRequestModel & {
    allowedTemplates: Array<ReferenceByIdModel>;
    defaultTemplate?: ReferenceByIdModel | null;
    cleanup: ContentTypeCleanupModel;
    allowedDocumentTypes: Array<DocumentTypeSortModel>;
    compositions: Array<DocumentTypeCompositionModel>;
});

