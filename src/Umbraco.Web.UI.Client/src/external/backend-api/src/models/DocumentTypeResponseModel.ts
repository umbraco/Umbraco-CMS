/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { ContentTypeForDocumentTypeResponseModel } from './ContentTypeForDocumentTypeResponseModel';

export type DocumentTypeResponseModel = (ContentTypeForDocumentTypeResponseModel & {
allowedTemplateIds: Array<string>;
defaultTemplateId?: string | null;
cleanup: ContentTypeCleanupModel;
});
