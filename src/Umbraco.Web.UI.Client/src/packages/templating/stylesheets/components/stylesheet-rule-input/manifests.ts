import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_RULE_SETTINGS_MODAL_ALIAS = 'Umb.Modal.StylesheetRuleSettings';

const modal: ManifestModal = {
	type: 'modal',
	alias: UMB_STYLESHEET_RULE_SETTINGS_MODAL_ALIAS,
	name: 'Stylesheet Rule Settings Modal',
	js: () => import('./stylesheet-rule-settings-modal.element.js'),
};

export const manifests = [modal];
