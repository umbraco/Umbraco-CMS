/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckGroupPresentationBaseModel } from './HealthCheckGroupPresentationBaseModel';
import type { HealthCheckModel } from './HealthCheckModel';

export type HealthCheckGroupPresentationModel = (HealthCheckGroupPresentationBaseModel & {
    checks?: Array<HealthCheckModel>;
});

