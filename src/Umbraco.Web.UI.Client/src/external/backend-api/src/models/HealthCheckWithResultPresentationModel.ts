/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckModelBaseModel } from './HealthCheckModelBaseModel';
import type { HealthCheckResultResponseModel } from './HealthCheckResultResponseModel';

export type HealthCheckWithResultPresentationModel = (HealthCheckModelBaseModel & {
    results?: Array<HealthCheckResultResponseModel> | null;
});

