import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberCreateOptionsModalData {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberCreateOptionsModalValue {}

export const UMB_MEMBER_CREATE_OPTIONS_MODAL = new UmbModalToken<
	UmbMemberCreateOptionsModalData,
	UmbMemberCreateOptionsModalValue
>('Umb.Modal.Member.CreateOptions', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
