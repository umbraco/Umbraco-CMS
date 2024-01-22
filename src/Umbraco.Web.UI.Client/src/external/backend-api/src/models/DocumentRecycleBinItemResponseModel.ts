/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { RecycleBinItemResponseModelBaseModel } from './RecycleBinItemResponseModelBaseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';

export type DocumentRecycleBinItemResponseModel = (RecycleBinItemResponseModelBaseModel & {
    documentType: DocumentTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
});

