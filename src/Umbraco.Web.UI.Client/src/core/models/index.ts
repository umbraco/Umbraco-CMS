import { components } from '../../../schemas/generated-schema';

export type PostInstallRequest = components['schemas']['InstallSetupRequest'];
export type StatusResponse = components['schemas']['StatusResponse'];
export type VersionResponse = components['schemas']['VersionResponse'];
export type ProblemDetails = components['schemas']['ProblemDetails'];
export type UserResponse = components['schemas']['UserResponse'];
export type AllowedSectionsResponse = components['schemas']['AllowedSectionsResponse'];
export type UmbracoInstaller = components['schemas']['InstallSettingsResponse'];

// Models
export type UmbracoPerformInstallDatabaseConfiguration = components['schemas']['InstallSetupDatabaseConfiguration'];
export type UmbracoInstallerDatabaseModel = components['schemas']['InstallDatabaseModel'];
export type UmbracoInstallerUserModel = components['schemas']['InstallUserModel'];
export type TelemetryModel = components['schemas']['TelemetryModel'];
export type ServerStatus = components['schemas']['ServerStatus'];

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;
