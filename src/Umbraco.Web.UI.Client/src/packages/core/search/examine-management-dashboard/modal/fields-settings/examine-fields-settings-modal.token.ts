import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbExamineFieldsSettingsModalData = never;

export type UmbExamineFieldSettingsType = {
	name: string;
	exposed: boolean;
};

export type UmbExamineFieldsSettingsModalValue = {
	fields: Array<UmbExamineFieldSettingsType>;
};

export const UMB_EXAMINE_FIELDS_SETTINGS_MODAL = new UmbModalToken<
	UmbExamineFieldsSettingsModalData,
	UmbExamineFieldsSettingsModalValue
>('Umb.Modal.Examine.FieldsSettings', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
