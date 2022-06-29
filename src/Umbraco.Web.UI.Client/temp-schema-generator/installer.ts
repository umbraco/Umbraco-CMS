import { body, defaultResponse, endpoint, request, response } from '@airtasker/spot';

import { ProblemDetails } from './models';

@endpoint({
  method: 'GET',
  path: '/install/settings',
})
export class GetInstallSettings {
  @response({ status: 200 })
  success(@body body: InstallSettingsResponse) {}

  @defaultResponse
  default(@body body: ProblemDetails) {}
}

@endpoint({
  method: 'POST',
  path: '/install/setup',
})
export class PostInstallSetup {
  @request
  request(@body body: InstallSetupRequest) {}

  @response({ status: 201 })
  success() {}

  @response({ status: 400 })
  badRequest(@body body: ProblemDetails) {}
}

@endpoint({
  method: 'POST',
  path: '/install/validateDatabase',
})
export class PostInstallValidateDatabase {
  @request
  request(@body body: InstallValidateDatabaseRequest) {}

  @response({ status: 201 })
  success() {}

  @response({ status: 400 })
  badRequest(@body body: ProblemDetails) {}
}

export interface InstallSetupRequest {
  name: string;
  email: string;
  password: string;
  subscribeToNewsletter: boolean;
  telemetryLevel: ConsentLevel;
  database: InstallSetupDatabaseConfiguration;
}

export interface InstallValidateDatabaseRequest {
  database: InstallSetupDatabaseConfiguration;
}

export interface InstallSettingsResponse {
  user: InstallUserModel;
  databases: InstallDatabaseModel[];
}

export interface InstallUserModel {
  minCharLength: number;
  minNonAlphaNumericLength: number;
  consentLevels: TelemetryModel[];
}

export interface InstallSetupDatabaseConfiguration {
  server?: string | null;
  password?: string | null;
  username?: string | null;
  databaseName?: string | null;
  databaseType?: string | null;
  useIntegratedAuthentication?: boolean | null;
  connectionString?: string | null;
}

export interface TelemetryModel {
  level: ConsentLevel;
  description: string;
}

export interface InstallDatabaseModel {
  id: string;
  sortOrder: number;
  displayName: string;
  defaultDatabaseName: string;
  providerName: null | string;
  isConfigured: boolean;
  requiresServer: boolean;
  serverPlaceholder: null | string;
  requiresCredentials: boolean;
  supportsIntegratedAuthentication: boolean;
  requiresConnectionTest: boolean;
}

export type ConsentLevel = 'Minimal' | 'Basic' | 'Detailed';
