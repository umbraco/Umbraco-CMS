import { manifest as defaultKindManifest } from './audit-log-action-default.kind.js';
import { manifest as contentRollbackKindManifest } from './content-rollback.audit-log-action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifestKind> = [defaultKindManifest, contentRollbackKindManifest];
