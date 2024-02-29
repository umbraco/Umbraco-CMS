/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DatabaseInstallPresentationModel } from './DatabaseInstallPresentationModel';
import type { DatabaseInstallRequestModel } from './DatabaseInstallRequestModel';
import type { TelemetryLevelModel } from './TelemetryLevelModel';
import type { UserInstallPresentationModel } from './UserInstallPresentationModel';
export type InstallRequestModel = {
    user: UserInstallPresentationModel;
    database: (DatabaseInstallPresentationModel | DatabaseInstallRequestModel);
    telemetryLevel: TelemetryLevelModel;
};

