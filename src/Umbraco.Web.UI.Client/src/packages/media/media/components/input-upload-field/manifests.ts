import type { ManifestFileUploadPreview } from '@umbraco-cms/backoffice/extension-registry';
const previews: Array<ManifestFileUploadPreview> = [
	{
		type: 'fileUploadPreview',
		alias: 'My PDF Showcase',
		name: 'PDF Showcase',
		forMimeTypes: ['application/pdf'],
	},
];
console.log('export..');

export const manifests = [...previews];
