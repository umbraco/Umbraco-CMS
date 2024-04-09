/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { DocumentValueModel } from './DocumentValueModel';
import type { DocumentVariantResponseModel } from './DocumentVariantResponseModel';

export type DocumentBlueprintResponseModel = {
    values: Array<DocumentValueModel>;
    variants: Array<DocumentVariantResponseModel>;
    id: string;
    documentType: DocumentTypeReferenceResponseModel;
};

