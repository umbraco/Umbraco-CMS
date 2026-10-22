import type { UmbMemberGroupItemModel } from '../../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMemberGroupPickerModalData {
	multiple?: boolean;
	/**
	 * @deprecated Deprecated since v17. Filtering member groups on the client breaks server-side pagination. Use `pickableFilter` to prevent member groups from being picked instead. Scheduled for removal in Umbraco 21.
	 */
	filter?: (memberGroup: UmbMemberGroupItemModel) => boolean;
	/**
	 * Items for which this returns false are shown but cannot be picked.
	 */
	pickableFilter?: (memberGroup: UmbMemberGroupItemModel) => boolean;
}

export interface UmbMemberGroupPickerModalValue {
	selection: Array<string | null>;
}

export const UMB_MEMBER_GROUP_PICKER_MODAL = new UmbModalToken<
	UmbMemberGroupPickerModalData,
	UmbMemberGroupPickerModalValue
>('Umb.Modal.MemberGroupPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
