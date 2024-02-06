/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaTypePropertyTypeContainerResponseModel } from './MediaTypePropertyTypeContainerResponseModel';
import type { MediaTypePropertyTypeResponseModel } from './MediaTypePropertyTypeResponseModel';

export type ContentTypeForMediaTypeResponseModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    isElement: boolean;
    properties: Array<MediaTypePropertyTypeResponseModel>;
    containers: Array<MediaTypePropertyTypeContainerResponseModel>;
    id: string;
};

