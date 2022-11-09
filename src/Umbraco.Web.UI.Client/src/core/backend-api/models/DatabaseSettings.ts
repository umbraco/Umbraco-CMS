/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type DatabaseSettings = {
    id?: string;
    sortOrder?: number;
    displayName?: string | null;
    defaultDatabaseName?: string | null;
    providerName?: string | null;
    isConfigured?: boolean;
    requiresServer?: boolean;
    serverPlaceholder?: string | null;
    requiresCredentials?: boolean;
    supportsIntegratedAuthentication?: boolean;
    requiresConnectionTest?: boolean;
};

