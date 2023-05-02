/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckModelBaseModel } from './HealthCheckModelBaseModel';

export type HealthCheckModel = (HealthCheckModelBaseModel & {
name?: string;
description?: string | null;
});
