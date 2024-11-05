export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.LanguagePicker',
		name: 'Language Picker Modal',
		js: () => import('./language-picker/language-picker-modal.element.js'),
	},
];
