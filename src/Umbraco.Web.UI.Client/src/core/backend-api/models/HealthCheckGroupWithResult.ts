/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckWithResult } from './HealthCheckWithResult';

export type HealthCheckGroupWithResult = {
    name?: string | null;
    checks?: Array<HealthCheckWithResult>;
};

