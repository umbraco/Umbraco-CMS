import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbExamineFieldsSettingsModalData = never;

type FieldSettingsType = {
	name: string;
	exposed: boolean;
};

export type UmbExamineFieldsSettingsModalValue = {
	fields: Array<FieldSettingsType>;
};

export const UMB_EXAMINE_FIELDS_SETTINGS_MODAL = new UmbModalToken<
	UmbExamineFieldsSettingsModalData,
	UmbExamineFieldsSettingsModalValue
>('Umb.Modal.ExamineFieldsSettings', {
	config: {
		type: 'sidebar',
		size: 'small',
	},
});
