/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCompositionModel } from './ContentTypeCompositionModel';
import type { ContentTypeSortModel } from './ContentTypeSortModel';
import type { UpdateMediaTypePropertyTypeContainerRequestModel } from './UpdateMediaTypePropertyTypeContainerRequestModel';
import type { UpdateMediaTypePropertyTypeRequestModel } from './UpdateMediaTypePropertyTypeRequestModel';

export type UpdateContentTypeRequestModelBaseUpdateMediaTypePropertyTypeRequestModelUpdateMediaTypePropertyTypeContainerRequestModel = {
    alias?: string;
    name?: string;
    description?: string | null;
    icon?: string;
    allowedAsRoot?: boolean;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    isElement?: boolean;
    properties?: Array<UpdateMediaTypePropertyTypeRequestModel>;
    containers?: Array<UpdateMediaTypePropertyTypeContainerRequestModel>;
    allowedContentTypes?: Array<ContentTypeSortModel>;
    compositions?: Array<ContentTypeCompositionModel>;
};

