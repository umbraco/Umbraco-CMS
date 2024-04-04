/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberTypeCompositionModel } from './MemberTypeCompositionModel';
import type { MemberTypePropertyTypeContainerResponseModel } from './MemberTypePropertyTypeContainerResponseModel';
import type { MemberTypePropertyTypeResponseModel } from './MemberTypePropertyTypeResponseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type MemberTypeResponseModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection: ReferenceByIdModel;
    isElement: boolean;
    properties: Array<MemberTypePropertyTypeResponseModel>;
    containers: Array<MemberTypePropertyTypeContainerResponseModel>;
    id: string;
    compositions: Array<MemberTypeCompositionModel>;
};

