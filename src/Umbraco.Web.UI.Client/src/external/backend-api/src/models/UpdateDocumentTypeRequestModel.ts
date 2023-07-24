/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { UpdateContentTypeRequestModelBaseUpdateDocumentTypePropertyTypeRequestModelUpdateDocumentTypePropertyTypeContainerRequestModel } from './UpdateContentTypeRequestModelBaseUpdateDocumentTypePropertyTypeRequestModelUpdateDocumentTypePropertyTypeContainerRequestModel';

export type UpdateDocumentTypeRequestModel = (UpdateContentTypeRequestModelBaseUpdateDocumentTypePropertyTypeRequestModelUpdateDocumentTypePropertyTypeContainerRequestModel & {
    allowedTemplateIds?: Array<string>;
    defaultTemplateId?: string | null;
    cleanup?: ContentTypeCleanupModel;
});

