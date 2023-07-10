/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { CreateContentTypeRequestModelBaseCreateDocumentTypePropertyTypeRequestModelCreateDocumentTypePropertyTypeContainerRequestModel } from './CreateContentTypeRequestModelBaseCreateDocumentTypePropertyTypeRequestModelCreateDocumentTypePropertyTypeContainerRequestModel';

export type CreateDocumentTypeRequestModel = (CreateContentTypeRequestModelBaseCreateDocumentTypePropertyTypeRequestModelCreateDocumentTypePropertyTypeContainerRequestModel & {
    allowedTemplateIds?: Array<string>;
    defaultTemplateId?: string | null;
    cleanup?: ContentTypeCleanupModel;
});

