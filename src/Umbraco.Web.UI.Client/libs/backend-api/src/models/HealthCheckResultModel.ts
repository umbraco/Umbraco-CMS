/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckActionModel } from './HealthCheckActionModel';
import type { StatusResultTypeModel } from './StatusResultTypeModel';

export type HealthCheckResultModel = {
    message?: string;
    resultType?: StatusResultTypeModel;
    actions?: Array<HealthCheckActionModel> | null;
    readMoreLink?: string | null;
};

