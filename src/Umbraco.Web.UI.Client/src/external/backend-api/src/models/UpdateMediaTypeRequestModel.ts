/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaTypeCompositionModel } from './MediaTypeCompositionModel';
import type { MediaTypeSortModel } from './MediaTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { UpdateMediaTypePropertyTypeContainerRequestModel } from './UpdateMediaTypePropertyTypeContainerRequestModel';
import type { UpdateMediaTypePropertyTypeRequestModel } from './UpdateMediaTypePropertyTypeRequestModel';

export type UpdateMediaTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection?: ReferenceByIdModel | null;
    isElement: boolean;
    properties: Array<UpdateMediaTypePropertyTypeRequestModel>;
    containers: Array<UpdateMediaTypePropertyTypeContainerRequestModel>;
    allowedMediaTypes: Array<MediaTypeSortModel>;
    compositions: Array<MediaTypeCompositionModel>;
};

