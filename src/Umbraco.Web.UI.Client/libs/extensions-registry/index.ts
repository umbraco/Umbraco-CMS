import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extensions-api';

export * from './interfaces';
export * from './models';
export * from './entry-point-extension-initializer';

export const umbExtensionsRegistry = new UmbExtensionRegistry();
