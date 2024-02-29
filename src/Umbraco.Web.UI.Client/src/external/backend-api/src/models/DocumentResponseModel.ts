/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentForDocumentResponseModel } from './ContentForDocumentResponseModel';
import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentUrlInfoModel } from './DocumentUrlInfoModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
export type DocumentResponseModel = (ContentForDocumentResponseModel & {
    urls: Array<DocumentUrlInfoModel>;
    template?: ReferenceByIdModel | null;
    isTrashed: boolean;
    documentType: DocumentTypeReferenceResponseModel;
});

