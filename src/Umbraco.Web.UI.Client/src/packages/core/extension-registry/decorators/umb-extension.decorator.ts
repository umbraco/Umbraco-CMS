// Re-export the decorator from extension-api.
// The base decorator already uses the global UmbExtensionManifest type,
// so no wrapper is needed — this re-export makes it available from
// @umbraco-cms/backoffice/extension-registry for convenience.
export { umbExtension } from '@umbraco-cms/backoffice/extension-api';
