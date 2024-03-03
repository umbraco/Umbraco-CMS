/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type HealthCheckActionRequestModel = {
    healthCheck: ReferenceByIdModel;
    alias?: string | null;
    name?: string | null;
    description?: string | null;
    valueRequired: boolean;
    providedValue?: string | null;
    providedValueValidation?: string | null;
    providedValueValidationRegex?: string | null;
};

