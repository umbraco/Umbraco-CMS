/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CreateMediaTypePropertyTypeContainerRequestModel } from './CreateMediaTypePropertyTypeContainerRequestModel';
import type { CreateMediaTypePropertyTypeRequestModel } from './CreateMediaTypePropertyTypeRequestModel';
import type { MediaTypeCompositionModel } from './MediaTypeCompositionModel';
import type { MediaTypeSortModel } from './MediaTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateMediaTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    isElement: boolean;
    properties: Array<CreateMediaTypePropertyTypeRequestModel>;
    containers: Array<CreateMediaTypePropertyTypeContainerRequestModel>;
    id?: string | null;
    parent: ReferenceByIdModel;
    allowedMediaTypes: Array<MediaTypeSortModel>;
    compositions: Array<MediaTypeCompositionModel>;
    collection: ReferenceByIdModel;
};

