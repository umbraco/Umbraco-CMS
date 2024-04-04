/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentVariantItemResponseModel } from './DocumentVariantItemResponseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DocumentTreeItemResponseModel = {
    hasChildren: boolean;
    parent?: ReferenceByIdModel | null;
    noAccess: boolean;
    isTrashed: boolean;
    id: string;
    isProtected: boolean;
    documentType: DocumentTypeReferenceResponseModel;
    variants: Array<DocumentVariantItemResponseModel>;
};

