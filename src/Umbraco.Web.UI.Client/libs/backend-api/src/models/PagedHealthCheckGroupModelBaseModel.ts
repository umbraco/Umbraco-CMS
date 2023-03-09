/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthCheckGroupModel } from './HealthCheckGroupModel';
import type { HealthCheckGroupModelBaseModel } from './HealthCheckGroupModelBaseModel';

export type PagedHealthCheckGroupModelBaseModel = {
    total: number;
    items: Array<(HealthCheckGroupModelBaseModel | HealthCheckGroupModel)>;
};

