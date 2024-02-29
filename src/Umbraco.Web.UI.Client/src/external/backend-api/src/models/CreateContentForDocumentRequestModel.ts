/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DocumentValueModel } from './DocumentValueModel';
import type { DocumentVariantRequestModel } from './DocumentVariantRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
export type CreateContentForDocumentRequestModel = {
    values: Array<DocumentValueModel>;
    variants: Array<DocumentVariantRequestModel>;
    id?: string | null;
    parent?: ReferenceByIdModel | null;
};

