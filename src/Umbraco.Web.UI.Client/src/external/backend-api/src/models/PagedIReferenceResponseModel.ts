/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DefaultReferenceResponseModel } from './DefaultReferenceResponseModel';
import type { DocumentReferenceResponseModel } from './DocumentReferenceResponseModel';
import type { MediaReferenceResponseModel } from './MediaReferenceResponseModel';

export type PagedIReferenceResponseModel = {
    total: number;
    items: Array<(DefaultReferenceResponseModel | DocumentReferenceResponseModel | MediaReferenceResponseModel)>;
};

