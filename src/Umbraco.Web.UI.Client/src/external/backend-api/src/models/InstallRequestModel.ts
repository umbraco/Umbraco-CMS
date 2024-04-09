/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DatabaseInstallRequestModel } from './DatabaseInstallRequestModel';
import type { TelemetryLevelModel } from './TelemetryLevelModel';
import type { UserInstallRequestModel } from './UserInstallRequestModel';

export type InstallRequestModel = {
    user: UserInstallRequestModel;
    database: DatabaseInstallRequestModel;
    telemetryLevel: TelemetryLevelModel;
};

