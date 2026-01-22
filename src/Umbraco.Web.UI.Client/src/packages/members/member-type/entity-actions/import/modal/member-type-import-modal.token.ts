import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMemberTypeImportModalData {
	unique: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberTypeImportModalValue {}

export const UMB_MEMBER_TYPE_IMPORT_MODAL = new UmbModalToken<
	UmbMemberTypeImportModalData,
	UmbMemberTypeImportModalValue
>('Umb.Modal.MemberType.Import', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
