import type { UmbMemberItemModel } from '../../repository/index.js';
import { UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';
import type { UmbPickerModalSearchConfig } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMemberPickerModalData {
	multiple?: boolean;
	filter?: (member: UmbMemberItemModel) => boolean;
	search?: UmbPickerModalSearchConfig;
}

export interface UmbMemberPickerModalValue {
	selection: Array<string | null>;
}

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
