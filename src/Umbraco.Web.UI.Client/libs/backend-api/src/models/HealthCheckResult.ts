/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckAction } from './HealthCheckAction';
import type { StatusResultType } from './StatusResultType';

export type HealthCheckResult = {
    message?: string;
    resultType?: StatusResultType;
    actions?: Array<HealthCheckAction> | null;
    readMoreLink?: string | null;
};

