import type { ManifestSearchIndexDetailBox } from './types.js';
import type { ManifestKind } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestKind<ManifestSearchIndexDetailBox> = {
  type: 'kind',
  alias: 'Umb.Kind.SearchIndexDetailBox',
  matchKind: 'default',
  matchType: 'searchIndexDetailBox',
  manifest: {
    type: 'searchIndexDetailBox',
    kind: 'default',
  },
};
