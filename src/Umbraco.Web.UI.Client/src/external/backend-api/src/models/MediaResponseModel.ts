/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentForMediaResponseModel } from './ContentForMediaResponseModel';
import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';
import type { MediaUrlInfoModel } from './MediaUrlInfoModel';

export type MediaResponseModel = (ContentForMediaResponseModel & {
    urls: Array<MediaUrlInfoModel>;
    isTrashed: boolean;
    mediaType: MediaTypeReferenceResponseModel;
});

