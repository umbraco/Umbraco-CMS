/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCompositionModel } from './ContentTypeCompositionModel';
import type { ContentTypeSortModel } from './ContentTypeSortModel';
import type { CreateMediaTypePropertyTypeContainerRequestModel } from './CreateMediaTypePropertyTypeContainerRequestModel';
import type { CreateMediaTypePropertyTypeRequestModel } from './CreateMediaTypePropertyTypeRequestModel';

export type CreateContentTypeRequestModelBaseCreateMediaTypePropertyTypeRequestModelCreateMediaTypePropertyTypeContainerRequestModel = {
    alias?: string;
    name?: string;
    description?: string | null;
    icon?: string;
    allowedAsRoot?: boolean;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    isElement?: boolean;
    properties?: Array<CreateMediaTypePropertyTypeRequestModel>;
    containers?: Array<CreateMediaTypePropertyTypeContainerRequestModel>;
    allowedContentTypes?: Array<ContentTypeSortModel>;
    compositions?: Array<ContentTypeCompositionModel>;
    id?: string | null;
    containerId?: string | null;
};

