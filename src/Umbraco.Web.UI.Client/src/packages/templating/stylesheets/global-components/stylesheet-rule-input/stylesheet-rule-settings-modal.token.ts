import type { UmbStylesheetRule } from '../../types.js';
import { UMB_STYLESHEET_RULE_SETTINGS_MODAL_ALIAS } from './manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbStylesheetRuleSettingsModalValue {
	rule: UmbStylesheetRule | null;
}

export const UMB_STYLESHEET_RULE_SETTINGS_MODAL = new UmbModalToken<never, UmbStylesheetRuleSettingsModalValue>(
	UMB_STYLESHEET_RULE_SETTINGS_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
		value: { rule: null },
	},
);
