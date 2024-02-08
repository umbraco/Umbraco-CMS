/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UpdateMemberTypePropertyTypeContainerRequestModel } from './UpdateMemberTypePropertyTypeContainerRequestModel';
import type { UpdateMemberTypePropertyTypeRequestModel } from './UpdateMemberTypePropertyTypeRequestModel';

export type UpdateContentTypeForMemberTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    isElement: boolean;
    properties: Array<UpdateMemberTypePropertyTypeRequestModel>;
    containers: Array<UpdateMemberTypePropertyTypeContainerRequestModel>;
};

