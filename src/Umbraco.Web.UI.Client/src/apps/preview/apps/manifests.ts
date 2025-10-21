import type { ManifestPreviewAppProvider } from '@umbraco-cms/backoffice/extension-registry';

// TODO: [LK] Create a new "preview" package and move these extensions/manifests there.

export const manifests: Array<ManifestPreviewAppProvider> = [
	{
		type: 'previewApp',
		alias: 'Umb.PreviewApps.Device',
		name: 'Preview: Device Switcher',
		element: () => import('./preview-device.element.js'),
		weight: 400,
	},
	{
		type: 'previewApp',
		alias: 'Umb.PreviewApps.Culture',
		name: 'Preview: Culture Switcher',
		element: () => import('./preview-culture.element.js'),
		weight: 310,
	},
	{
		type: 'previewApp',
		alias: 'Umb.PreviewApps.Segment',
		name: 'Preview: Segment Switcher',
		element: () => import('./preview-segment.element.js'),
		weight: 300,
	},
	{
		type: 'previewApp',
		alias: 'Umb.PreviewApps.Environments',
		name: 'Preview: Environments Menu',
		element: () => import('./preview-environments.element.js'),
		weight: 210,
	},
	{
		type: 'previewApp',
		alias: 'Umb.PreviewApps.OpenWebsite',
		name: 'Preview: Open Website Button',
		element: () => import('./preview-open-website.element.js'),
		weight: 200,
	},
	{
		type: 'previewApp',
		alias: 'Umb.PreviewApps.Exit',
		name: 'Preview: Exit Button',
		element: () => import('./preview-exit.element.js'),
		weight: 100,
	},
];
