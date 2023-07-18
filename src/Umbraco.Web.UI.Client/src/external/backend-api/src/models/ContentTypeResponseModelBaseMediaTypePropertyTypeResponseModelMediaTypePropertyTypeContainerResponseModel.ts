/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCompositionModel } from './ContentTypeCompositionModel';
import type { ContentTypeSortModel } from './ContentTypeSortModel';
import type { MediaTypePropertyTypeContainerResponseModel } from './MediaTypePropertyTypeContainerResponseModel';
import type { MediaTypePropertyTypeResponseModel } from './MediaTypePropertyTypeResponseModel';

export type ContentTypeResponseModelBaseMediaTypePropertyTypeResponseModelMediaTypePropertyTypeContainerResponseModel = {
    alias?: string;
    name?: string;
    description?: string | null;
    icon?: string;
    allowedAsRoot?: boolean;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    isElement?: boolean;
    properties?: Array<MediaTypePropertyTypeResponseModel>;
    containers?: Array<MediaTypePropertyTypeContainerResponseModel>;
    allowedContentTypes?: Array<ContentTypeSortModel>;
    compositions?: Array<ContentTypeCompositionModel>;
    id?: string;
};

