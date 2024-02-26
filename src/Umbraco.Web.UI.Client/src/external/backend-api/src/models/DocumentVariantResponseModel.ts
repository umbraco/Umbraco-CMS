/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentVariantStateModel } from './DocumentVariantStateModel';
import type { VariantResponseModelBaseModel } from './VariantResponseModelBaseModel';

export type DocumentVariantResponseModel = (VariantResponseModelBaseModel & {
    state: DocumentVariantStateModel;
    publishDate?: string | null;
});

