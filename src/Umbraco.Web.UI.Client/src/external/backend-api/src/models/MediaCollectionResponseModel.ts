/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaTypeCollectionReferenceResponseModel } from './MediaTypeCollectionReferenceResponseModel';
import type { MediaValueModel } from './MediaValueModel';
import type { MediaVariantResponseModel } from './MediaVariantResponseModel';

export type MediaCollectionResponseModel = {
    values: Array<MediaValueModel>;
    variants: Array<MediaVariantResponseModel>;
    id: string;
    creator?: string | null;
    sortOrder: number;
    mediaType: MediaTypeCollectionReferenceResponseModel;
};

