/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DatabaseInstallResponseModel } from './DatabaseInstallResponseModel';
import type { TelemetryLevelModel } from './TelemetryLevelModel';
import type { UserInstallResponseModel } from './UserInstallResponseModel';

export type InstallVResponseModel = {
    user: UserInstallResponseModel;
    database: DatabaseInstallResponseModel;
    telemetryLevel?: TelemetryLevelModel;
};

