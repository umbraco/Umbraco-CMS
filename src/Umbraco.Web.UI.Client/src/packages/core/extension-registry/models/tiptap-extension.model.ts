import type { UmbTiptapExtensionBase } from '@umbraco-cms/backoffice/tiptap';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapExtension extends ManifestApi<UmbTiptapExtensionBase> {
	type: 'tiptapExtension';
}
