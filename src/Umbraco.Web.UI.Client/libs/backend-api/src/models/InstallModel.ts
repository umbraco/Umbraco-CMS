/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DatabaseInstallModel } from './DatabaseInstallModel';
import type { TelemetryLevelModel } from './TelemetryLevelModel';
import type { UserInstallModel } from './UserInstallModel';

export type InstallModel = {
    user: UserInstallModel;
    database: DatabaseInstallModel;
    telemetryLevel?: TelemetryLevelModel;
};

