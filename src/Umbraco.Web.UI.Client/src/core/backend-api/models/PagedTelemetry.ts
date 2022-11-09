/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Telemetry } from './Telemetry';

export type PagedTelemetry = {
    total?: number;
    items?: Array<Telemetry> | null;
};

