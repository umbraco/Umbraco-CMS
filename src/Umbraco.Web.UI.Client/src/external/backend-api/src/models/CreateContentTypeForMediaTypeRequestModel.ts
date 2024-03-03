/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CreateMediaTypePropertyTypeContainerRequestModel } from './CreateMediaTypePropertyTypeContainerRequestModel';
import type { CreateMediaTypePropertyTypeRequestModel } from './CreateMediaTypePropertyTypeRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateContentTypeForMediaTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection?: ReferenceByIdModel | null;
    isElement: boolean;
    properties: Array<CreateMediaTypePropertyTypeRequestModel>;
    containers: Array<CreateMediaTypePropertyTypeContainerRequestModel>;
    id?: string | null;
    parent?: ReferenceByIdModel | null;
};

