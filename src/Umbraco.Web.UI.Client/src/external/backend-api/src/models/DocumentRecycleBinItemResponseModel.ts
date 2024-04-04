/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentVariantItemResponseModel } from './DocumentVariantItemResponseModel';
import type { ItemReferenceByIdResponseModel } from './ItemReferenceByIdResponseModel';

export type DocumentRecycleBinItemResponseModel = {
    id: string;
    hasChildren: boolean;
    parent: ItemReferenceByIdResponseModel;
    documentType: DocumentTypeReferenceResponseModel;
    variants: Array<DocumentVariantItemResponseModel>;
};

