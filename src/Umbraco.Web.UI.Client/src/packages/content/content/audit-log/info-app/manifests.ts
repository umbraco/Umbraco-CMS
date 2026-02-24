import { manifest as auditLogKindManifest } from './content-audit-log-workspace-info-app.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [auditLogKindManifest];
