import { manifests as propertyEditors } from './property-editors/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...propertyEditors];

export const name = 'Umbraco.Core.MarkdownEditor';
export const extensions = [
	{
		name: 'Markdown Editor Bundle',
		alias: 'Umb.Bundle.MarkdownEditor',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
