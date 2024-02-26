/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PropertyTypeAppearanceModel } from './PropertyTypeAppearanceModel';
import type { PropertyTypeValidationModel } from './PropertyTypeValidationModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type PropertyTypeModelBaseModel = {
    id: string;
    container?: ReferenceByIdModel | null;
    sortOrder: number;
    alias: string;
    name: string;
    description?: string | null;
    dataType: ReferenceByIdModel;
    variesByCulture: boolean;
    variesBySegment: boolean;
    validation: PropertyTypeValidationModel;
    appearance: PropertyTypeAppearanceModel;
};

