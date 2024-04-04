/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentValueModel } from './DocumentValueModel';
import type { DocumentVariantRequestModel } from './DocumentVariantRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type UpdateDocumentRequestModel = {
    values: Array<DocumentValueModel>;
    variants: Array<DocumentVariantRequestModel>;
    template: ReferenceByIdModel;
};

