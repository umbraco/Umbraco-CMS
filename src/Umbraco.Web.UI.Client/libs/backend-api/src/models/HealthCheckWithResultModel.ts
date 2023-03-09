/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckModelBaseModel } from './HealthCheckModelBaseModel';
import type { HealthCheckResultModel } from './HealthCheckResultModel';

export type HealthCheckWithResultModel = (HealthCheckModelBaseModel & {
    results?: Array<HealthCheckResultModel> | null;
});

