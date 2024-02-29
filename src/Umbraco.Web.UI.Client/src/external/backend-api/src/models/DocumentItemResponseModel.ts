/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentVariantItemResponseModel } from './DocumentVariantItemResponseModel';
import type { ItemResponseModelBaseModel } from './ItemResponseModelBaseModel';
export type DocumentItemResponseModel = (ItemResponseModelBaseModel & {
    isTrashed: boolean;
    isProtected: boolean;
    documentType: DocumentTypeReferenceResponseModel;
    variants: Array<DocumentVariantItemResponseModel>;
});

