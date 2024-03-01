/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentVariantItemResponseModel } from './DocumentVariantItemResponseModel';
import type { RecycleBinItemResponseModelBaseModel } from './RecycleBinItemResponseModelBaseModel';

export type DocumentRecycleBinItemResponseModel = (RecycleBinItemResponseModelBaseModel & {
    documentType: DocumentTypeReferenceResponseModel;
    variants: Array<DocumentVariantItemResponseModel>;
});

