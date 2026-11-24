import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberSearchItemModel } from '../../search/types.js';
import { UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';
import type { UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbMemberPickerModalData = UmbPickerModalData<UmbMemberDetailModel, UmbMemberSearchItemModel>;
export type UmbMemberPickerModalValue = UmbPickerModalValue;

export const UMB_MEMBER_PICKER_MODAL = new UmbModalToken<UmbMemberPickerModalData, UmbMemberPickerModalValue>(
	'Umb.Modal.MemberPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			search: {
				providerAlias: UMB_MEMBER_SEARCH_PROVIDER_ALIAS,
			},
		},
	},
);
