/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberTypeCompositionModel } from './MemberTypeCompositionModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { UpdateMemberTypePropertyTypeContainerRequestModel } from './UpdateMemberTypePropertyTypeContainerRequestModel';
import type { UpdateMemberTypePropertyTypeRequestModel } from './UpdateMemberTypePropertyTypeRequestModel';

export type UpdateMemberTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection?: ReferenceByIdModel | null;
    isElement: boolean;
    properties: Array<UpdateMemberTypePropertyTypeRequestModel>;
    containers: Array<UpdateMemberTypePropertyTypeContainerRequestModel>;
    compositions: Array<MemberTypeCompositionModel>;
};

