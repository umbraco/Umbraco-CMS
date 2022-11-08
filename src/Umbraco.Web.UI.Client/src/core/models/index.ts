import type { components } from '../../../schemas/generated-schema';
import type { UserStatus } from '../../backoffice/sections/users/user-extensions';
import { Entity } from '../mocks/data/entities';

export type PostInstallRequest = components['schemas']['InstallSetupRequest'];
export type StatusResponse = components['schemas']['StatusResponse'];
export type VersionResponse = components['schemas']['VersionResponse'];
export type ProblemDetails = components['schemas']['ProblemDetails'];
export type UserResponse = components['schemas']['UserResponse'];
export type AllowedSectionsResponse = components['schemas']['AllowedSectionsResponse'];
export type UmbracoInstaller = components['schemas']['InstallSettingsResponse'];
export type UmbracoUpgrader = components['schemas']['UpgradeSettingsResponse'];
export type ManifestsResponse = components['schemas']['ManifestsResponse'];
export type ManifestsPackagesInstalledResponse = components['schemas']['ManifestsPackagesInstalledResponse'];

// Models
export type UmbracoPerformInstallDatabaseConfiguration = components['schemas']['InstallSetupDatabaseConfiguration'];
export type UmbracoInstallerDatabaseModel = components['schemas']['InstallDatabaseModel'];
export type UmbracoInstallerUserModel = components['schemas']['InstallUserModel'];
export type TelemetryModel = components['schemas']['TelemetryModel'];
export type ServerStatus = components['schemas']['ServerStatus'];
export type PackageInstalled = components['schemas']['PackageInstalled'];
export type ConsentLevelSettings = components['schemas']['ConsentLevelSettings'];

// Extension Manifests
export * from '../extensions-registry/models';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;

// Users
export interface UserEntity extends Entity {
	type: 'user';
}

export interface UserDetails extends UserEntity {
	email: string;
	status: UserStatus;
	language: string;
	lastLoginDate?: string;
	lastLockoutDate?: string;
	lastPasswordChangeDate?: string;
	updateDate: string;
	createDate: string;
	failedLoginAttempts: number;
	userGroup?: string; //TODO Implement this
}

export interface UserGroupEntity extends Entity {
	type: 'userGroup';
}

export interface UserGroupDetails extends UserGroupEntity {
	key: string;
	name: string;
	icon: string;
	sections?: Array<string>;
	contentStartNode?: string;
	mediaStartNode?: string;
}
