import { manifest as defaultKind } from './default.preview-option.kind.js';
import { manifest as urlProviderKind } from './url-provider.preview-option.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const previewOption: UmbExtensionManifest = {
	type: 'previewOption',
	kind: 'default',
	alias: 'Umb.PreviewOption.Document.SaveAndPreview',
	name: 'Save And Preview Document Preview Option',
	weight: 200,
	meta: {
		label: '#buttons_saveAndPreview',
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	previewOption,
	defaultKind,
	urlProviderKind,
];
