/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckActionRequestModel } from './HealthCheckActionRequestModel';
import type { StatusResultTypeModel } from './StatusResultTypeModel';

export type HealthCheckResultResponseModel = {
    message?: string;
    resultType?: StatusResultTypeModel;
    actions?: Array<HealthCheckActionRequestModel> | null;
    readMoreLink?: string | null;
};

