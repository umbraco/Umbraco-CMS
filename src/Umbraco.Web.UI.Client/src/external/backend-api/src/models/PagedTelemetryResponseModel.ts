/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { TelemetryResponseModel } from './TelemetryResponseModel';

export type PagedTelemetryResponseModel = {
    total: number;
    items: Array<TelemetryResponseModel>;
};
