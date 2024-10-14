export const UMB_STYLESHEET_RULE_SETTINGS_MODAL_ALIAS = 'Umb.Modal.StylesheetRuleSettings';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_STYLESHEET_RULE_SETTINGS_MODAL_ALIAS,
		name: 'Stylesheet Rule Settings Modal',
		element: () => import('./stylesheet-rule-settings-modal.element.js'),
	},
];
