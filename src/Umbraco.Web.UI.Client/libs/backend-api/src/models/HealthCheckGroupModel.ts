/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckGroupModelBaseModel } from './HealthCheckGroupModelBaseModel';
import type { HealthCheckModel } from './HealthCheckModel';

export type HealthCheckGroupModel = (HealthCheckGroupModelBaseModel & {
    checks?: Array<HealthCheckModel>;
});

