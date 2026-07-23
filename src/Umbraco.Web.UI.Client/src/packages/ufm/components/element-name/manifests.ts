import type { ManifestUfmComponent } from '../../extensions/ufm-component.extension.js';

export const manifests: Array<ManifestUfmComponent> = [
	{
		type: 'ufmComponent',
		alias: 'Umb.Markdown.ElementName',
		name: 'Element Name UFM Component',
		api: () => import('./element-name.component.js'),
		meta: { alias: 'umbElementName' },
	},
];
