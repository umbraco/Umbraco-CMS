/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemResponseModel } from './ContentTreeItemResponseModel';
import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { VariantItemResponseModel } from './VariantItemResponseModel';

export type DocumentTreeItemResponseModel = (ContentTreeItemResponseModel & {
    isProtected: boolean;
    documentType: DocumentTypeReferenceResponseModel;
    variants: Array<VariantItemResponseModel>;
});

