/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentForMediaResponseModel } from './ContentForMediaResponseModel';
import type { ContentUrlInfoModel } from './ContentUrlInfoModel';
import type { MediaTypeReferenceResponseModel } from './MediaTypeReferenceResponseModel';

export type MediaResponseModel = (ContentForMediaResponseModel & {
    urls: Array<ContentUrlInfoModel>;
    isTrashed: boolean;
    mediaType: MediaTypeReferenceResponseModel;
});

