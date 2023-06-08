/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { UpdateContentTypeRequestModelBaseUpdateDocumentTypePropertyTypeRequestModelUpdateDocumentTypePropertyTypeContainerRequestModel } from './UpdateContentTypeRequestModelBaseUpdateDocumentTypePropertyTypeRequestModelUpdateDocumentTypePropertyTypeContainerRequestModel';

export type UpdateDocumentTypeRequestModel = (UpdateContentTypeRequestModelBaseUpdateDocumentTypePropertyTypeRequestModelUpdateDocumentTypePropertyTypeContainerRequestModel & {
    $type: string;
    allowedTemplateIds?: Array<string>;
    defaultTemplateId?: string | null;
    cleanup?: ContentTypeCleanupModel;
});

