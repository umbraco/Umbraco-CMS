/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { ContentTypeResponseModelBaseDocumentTypePropertyTypeResponseModelDocumentTypePropertyTypeContainerResponseModel } from './ContentTypeResponseModelBaseDocumentTypePropertyTypeResponseModelDocumentTypePropertyTypeContainerResponseModel';

export type DocumentTypeResponseModel = (ContentTypeResponseModelBaseDocumentTypePropertyTypeResponseModelDocumentTypePropertyTypeContainerResponseModel & {
allowedTemplateIds?: Array<string>;
defaultTemplateId?: string | null;
cleanup?: ContentTypeCleanupModel;
});
