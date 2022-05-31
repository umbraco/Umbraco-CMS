import { body, endpoint, request, response } from '@airtasker/spot';

import { ErrorResponse } from './models';

@endpoint({
  method: 'GET',
  path: '/install',
})
export class GetInstall {
  @response({ status: 200 })
  success(@body body: UmbracoInstaller) {}
}

@endpoint({
  method: 'POST',
  path: '/install',
})
export class PostInstall {
  @request
  request(@body body: UmbracoPerformInstallRequest) {}

  @response({ status: 201 })
  success() {}

  @response({ status: 400 })
  badRequest(@body body: ErrorResponse) {}
}

@endpoint({
  method: 'POST',
  path: '/install/database/validate',
})
export class PostInstallValidateDatabase {
  @request
  request(@body body: UmbracoPerformInstallDatabaseConfiguration) {}

  @response({ status: 201 })
  success() {}

  @response({ status: 400 })
  badRequest(@body body: ErrorResponse) {}
}

export interface UmbracoPerformInstallRequest {
  name: string;
  email: string;
  password: string;
  subscribeToNewsletter: boolean;
  telemetryLevel: ConsentLevel;
  database: UmbracoPerformInstallDatabaseConfiguration;
}

export interface UmbracoPerformInstallDatabaseConfiguration {
  server?: string | null;
  password?: string | null;
  username?: string | null;
  databaseName?: string | null;
  databaseType?: string | null;
  useIntegratedAuthentication?: boolean | null;
  connectionString?: string | null;
}

export interface UmbracoInstaller {
  user: UmbracoInstallerUserModel;
  databases: UmbracoInstallerDatabaseModel[];
}

export interface UmbracoInstallerUserModel {
  minCharLength: number;
  minNonAlphaNumericLength: number;
  consentLevels: TelemetryModel[];
}

export interface TelemetryModel {
  level: ConsentLevel;
  description: string;
}

export interface UmbracoInstallerDatabaseModel {
  id: string;
  sortOrder: number;
  displayName: string;
  defaultDatabaseName: string;
  providerName: null | string;
  isAvailable: boolean;
  requiresServer: boolean;
  serverPlaceholder: null | string;
  requiresCredentials: boolean;
  supportsIntegratedAuthentication: boolean;
  requiresConnectionTest: boolean;
}

export type ConsentLevel = 'Minimal' | 'Basic' | 'Detailed';
