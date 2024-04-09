/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentVariantStateModel } from './DocumentVariantStateModel';

export type DocumentVariantResponseModel = {
    culture?: string | null;
    segment?: string | null;
    name: string;
    createDate: string;
    updateDate: string;
    state: DocumentVariantStateModel;
    publishDate?: string | null;
};

