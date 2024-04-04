/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MemberTypePropertyTypeVisibilityModel } from './MemberTypePropertyTypeVisibilityModel';
import type { PropertyTypeAppearanceModel } from './PropertyTypeAppearanceModel';
import type { PropertyTypeValidationModel } from './PropertyTypeValidationModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateMemberTypePropertyTypeRequestModel = {
    id: string;
    container: ReferenceByIdModel;
    sortOrder: number;
    alias: string;
    name: string;
    description?: string | null;
    dataType: ReferenceByIdModel;
    variesByCulture: boolean;
    variesBySegment: boolean;
    validation: PropertyTypeValidationModel;
    appearance: PropertyTypeAppearanceModel;
    isSensitive: boolean;
    visibility: MemberTypePropertyTypeVisibilityModel;
};

