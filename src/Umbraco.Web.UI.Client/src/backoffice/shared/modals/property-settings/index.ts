import { UmbModalToken } from '@umbraco-cms/modal';

// TODO: add interface for data
// PropertyTypeViewModelBaseModel

export interface UmbPropertySettingsModalResult {
	label: string;
	alias: string;
	description: string;
	propertyEditorUI?: string;
	labelOnTop: boolean;
	validation: {
		mandatory: boolean;
		mandatoryMessage: string;
		pattern: string;
		patternMessage: string;
	};
}

export const UMB_PROPERTY_SETTINGS_MODAL_TOKEN = new UmbModalToken<undefined, UmbPropertySettingsModalResult>(
	'Umb.Modal.PropertySettings',
	{
		type: 'sidebar',
		size: 'small',
	}
);
