/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaTypeCompositionModel } from './MediaTypeCompositionModel';
import type { MediaTypePropertyTypeContainerResponseModel } from './MediaTypePropertyTypeContainerResponseModel';
import type { MediaTypePropertyTypeResponseModel } from './MediaTypePropertyTypeResponseModel';
import type { MediaTypeSortModel } from './MediaTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type MediaTypeResponseModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection: ReferenceByIdModel;
    isElement: boolean;
    properties: Array<MediaTypePropertyTypeResponseModel>;
    containers: Array<MediaTypePropertyTypeContainerResponseModel>;
    id: string;
    allowedMediaTypes: Array<MediaTypeSortModel>;
    compositions: Array<MediaTypeCompositionModel>;
};

