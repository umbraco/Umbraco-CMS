// Pulls in the backoffice's ambient extension types (UmbExtensionManifest, UmbExtensionManifestMap, etc.).
// This replaces the former compilerOptions.types entry, which cannot resolve through tsconfig path aliases.
import '@umbraco-cms/backoffice/extension-types';
