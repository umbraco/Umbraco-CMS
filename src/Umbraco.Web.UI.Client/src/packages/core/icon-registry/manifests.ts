import { manifests as iconPickerModalManifests } from './icon-picker-modal/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'icons',
		alias: 'Umb.Icons.Backoffice',
		name: 'Backoffice Icons',
		js: () => import('./icons.js'),
	},
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Icons',
		name: 'Icons Context',
		api: () => import('./icon-registry.context.js'),
	},
	...iconPickerModalManifests,
];
