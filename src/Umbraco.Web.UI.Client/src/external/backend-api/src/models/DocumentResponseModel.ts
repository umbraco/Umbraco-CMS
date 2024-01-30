/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentForDocumentResponseModel } from './ContentForDocumentResponseModel';
import type { ContentUrlInfoModel } from './ContentUrlInfoModel';
import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DocumentResponseModel = (ContentForDocumentResponseModel & {
    urls: Array<ContentUrlInfoModel>;
    template?: ReferenceByIdModel | null;
    isTrashed: boolean;
    documentType: DocumentTypeReferenceResponseModel;
});

