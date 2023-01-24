/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DatabaseInstall } from './DatabaseInstall';
import type { TelemetryLevel } from './TelemetryLevel';
import type { UserInstall } from './UserInstall';

export type Install = {
    user: UserInstall;
    database: DatabaseInstall;
    telemetryLevel?: TelemetryLevel;
};

