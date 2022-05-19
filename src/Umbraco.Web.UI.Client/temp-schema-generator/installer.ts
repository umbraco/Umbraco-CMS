import { body, endpoint, request, response } from '@airtasker/spot';

import { ErrorResponse } from './models';

@endpoint({
    method: 'GET',
    path: '/install',
})
export class GetInstall {
    @response({ status: 200 })
    success(@body body: UmbracoInstaller) { }
}

@endpoint({
    method: 'POST',
    path: '/install',
})
export class PostInstall {
    @request
    request(@body body: UmbracoInstaller) { }

    @response({ status: 201 })
    success() { }

    @response({ status: 400 })
    badRequest(@body body: ErrorResponse) { }
}

export interface UmbracoInstaller {
    installId: string;
    steps: UmbracoInstallerStep[];
}

export interface UmbracoInstallerStep {
    model: UmbracoInstallerStepModel | null;
    view: string;
    name: string;
    description: string;
    serverOrder: number;
}

export interface UmbracoInstallerStepModel {
    minCharLength?: number;
    minNonAlphaNumericLength?: number;
    customInstallAvailable?: boolean;
    consentLevels?: ConsentLevel[];
    databases?: UmbracoDatabaseConfiguration[];
    quickInstallSettings?: UmbracoDatabaseConfigurationQuickInstall;
}

export interface ConsentLevel {
    level: 'Minimal' | 'Basic' | 'Detailed';
    description: string;
}

export interface UmbracoDatabaseConfiguration {
    id: string;
    sortOrder: number;
    displayName: string;
    defaultDatabaseName: string;
    providerName: null | string;
    supportsQuickInstall: boolean;
    isAvailable: boolean;
    requiresServer: boolean;
    serverPlaceholder: null | string;
    requiresCredentials: boolean;
    supportsIntegratedAuthentication: boolean;
    requiresConnectionTest: boolean;
}

export interface UmbracoDatabaseConfigurationQuickInstall {
    displayName: string;
    defaultDatabaseName: string;
}
