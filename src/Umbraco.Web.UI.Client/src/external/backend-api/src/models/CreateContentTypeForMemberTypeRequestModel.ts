/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CreateMemberTypePropertyTypeContainerRequestModel } from './CreateMemberTypePropertyTypeContainerRequestModel';
import type { CreateMemberTypePropertyTypeRequestModel } from './CreateMemberTypePropertyTypeRequestModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateContentTypeForMemberTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection?: ReferenceByIdModel | null;
    isElement: boolean;
    properties: Array<CreateMemberTypePropertyTypeRequestModel>;
    containers: Array<CreateMemberTypePropertyTypeContainerRequestModel>;
    id?: string | null;
};

