import { UMB_BOOLEAN_VALUE_MINIMAL_DISPLAY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueMinimalDisplay',
		alias: UMB_BOOLEAN_VALUE_MINIMAL_DISPLAY_ALIAS,
		name: 'Boolean Value Minimal Display',
		element: () => import('./boolean-value-minimal-display.element.js'),
	},
];
