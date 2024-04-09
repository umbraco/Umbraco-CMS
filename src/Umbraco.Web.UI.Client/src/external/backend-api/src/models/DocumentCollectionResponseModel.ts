/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeCollectionReferenceResponseModel } from './DocumentTypeCollectionReferenceResponseModel';
import type { DocumentValueModel } from './DocumentValueModel';
import type { DocumentVariantResponseModel } from './DocumentVariantResponseModel';

export type DocumentCollectionResponseModel = {
    values: Array<DocumentValueModel>;
    variants: Array<DocumentVariantResponseModel>;
    id: string;
    creator?: string | null;
    sortOrder: number;
    documentType: DocumentTypeCollectionReferenceResponseModel;
    updater?: string | null;
};

