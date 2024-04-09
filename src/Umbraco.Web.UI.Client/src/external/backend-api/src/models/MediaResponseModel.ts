/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';
import type { MediaUrlInfoModel } from './MediaUrlInfoModel';
import type { MediaValueModel } from './MediaValueModel';
import type { MediaVariantResponseModel } from './MediaVariantResponseModel';

export type MediaResponseModel = {
    values: Array<MediaValueModel>;
    variants: Array<MediaVariantResponseModel>;
    id: string;
    urls: Array<MediaUrlInfoModel>;
    isTrashed: boolean;
    mediaType: MediaTypeReferenceResponseModel;
};

