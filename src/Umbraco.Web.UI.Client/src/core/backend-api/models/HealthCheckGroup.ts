/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheck } from './HealthCheck';

export type HealthCheckGroup = {
    name?: string | null;
    checks?: Array<HealthCheck> | null;
};

