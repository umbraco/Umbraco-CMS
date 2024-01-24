/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { UpdateContentTypeForDocumentTypeRequestModel } from './UpdateContentTypeForDocumentTypeRequestModel';

export type UpdateDocumentTypeRequestModel = (UpdateContentTypeForDocumentTypeRequestModel & {
allowedTemplateIds: Array<string>;
defaultTemplateId?: string | null;
cleanup: ContentTypeCleanupModel;
});
