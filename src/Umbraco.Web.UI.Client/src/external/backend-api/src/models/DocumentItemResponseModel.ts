/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentVariantItemResponseModel } from './DocumentVariantItemResponseModel';

export type DocumentItemResponseModel = {
    id: string;
    isTrashed: boolean;
    isProtected: boolean;
    documentType: DocumentTypeReferenceResponseModel;
    variants: Array<DocumentVariantItemResponseModel>;
};

