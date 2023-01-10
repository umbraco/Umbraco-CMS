/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckResult } from './HealthCheckResult';

export type HealthCheckWithResult = {
    key?: string;
    name?: string | null;
    description?: string | null;
    results?: Array<HealthCheckResult> | null;
};

