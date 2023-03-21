import { UmbModalToken } from '@umbraco-cms/modal';

export type UmbExamineFieldsSettingsModalData = Array<{
	name: string;
	exposed: boolean;
}>;

export interface UmbCreateDocumentModalResultData {
	fields?: UmbExamineFieldsSettingsModalData;
}

export const UMB_EXAMINE_FIELDS_SETTINGS_MODAL_TOKEN = new UmbModalToken<
	UmbExamineFieldsSettingsModalData,
	UmbCreateDocumentModalResultData
>('Umb.Modal.ExamineFieldsSettings', {
	type: 'sidebar',
	size: 'small',
});
