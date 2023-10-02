import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbExamineFieldsSettingsModalData = Array<{
	name: string;
	exposed: boolean;
}>;

export interface UmbExamineFieldsSettingsModalValue {
	fields?: UmbExamineFieldsSettingsModalData;
}

export const UMB_EXAMINE_FIELDS_SETTINGS_MODAL = new UmbModalToken<
	UmbExamineFieldsSettingsModalData,
	UmbExamineFieldsSettingsModalValue
>('Umb.Modal.ExamineFieldsSettings', {
	type: 'sidebar',
	size: 'small',
});
