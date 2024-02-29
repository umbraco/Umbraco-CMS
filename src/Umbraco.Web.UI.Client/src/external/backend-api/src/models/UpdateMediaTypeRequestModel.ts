/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MediaTypeCompositionModel } from './MediaTypeCompositionModel';
import type { MediaTypeSortModel } from './MediaTypeSortModel';
import type { UpdateContentTypeForMediaTypeRequestModel } from './UpdateContentTypeForMediaTypeRequestModel';
export type UpdateMediaTypeRequestModel = (UpdateContentTypeForMediaTypeRequestModel & {
    allowedMediaTypes: Array<MediaTypeSortModel>;
    compositions: Array<MediaTypeCompositionModel>;
});

