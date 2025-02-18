import type { UmbMemberItemModel } from '../../item/types.js';
import {
	UMB_MEMBER_PICKER_MODAL,
	type UmbMemberPickerModalData,
	type UmbMemberPickerModalValue,
} from '../member-picker-modal/member-picker-modal.token.js';
import { UMB_MEMBER_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../../constants.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbMemberTypeEntityType } from '@umbraco-cms/backoffice/member-type';

interface UmbMemberPickerInputContextOpenArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbMemberTypeEntityType }>;
}

export class UmbMemberPickerInputContext extends UmbPickerInputContext<
	UmbMemberItemModel,
	UmbMemberItemModel,
	UmbMemberPickerModalData,
	UmbMemberPickerModalValue
> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_PICKER_MODAL);
	}

	override async openPicker(
		pickerData?: Partial<UmbMemberPickerModalData>,
		args?: UmbMemberPickerInputContextOpenArgs,
	) {
		const combinedPickerData = {
			...pickerData,
		};

		// transform allowedContentTypes to a pickable filter
		combinedPickerData.pickableFilter = (item) => this.#pickableFilter(item, args?.allowedContentTypes);

		// set default search data
		if (!pickerData?.search) {
			combinedPickerData.search = {
				providerAlias: UMB_MEMBER_SEARCH_PROVIDER_ALIAS,
				...pickerData?.search,
			};
		}

		// pass allowedContentTypes to the search request args
		combinedPickerData.search!.queryParams = {
			allowedContentTypes: args?.allowedContentTypes,
			...pickerData?.search?.queryParams,
		};

		super.openPicker(combinedPickerData);
	}

	#pickableFilter = (
		item: UmbMemberItemModel,
		allowedContentTypes?: Array<{ unique: string; entityType: UmbMemberTypeEntityType }>,
	): boolean => {
		if (allowedContentTypes && allowedContentTypes.length > 0) {
			return allowedContentTypes
				.map((contentTypeReference) => contentTypeReference.unique)
				.includes(item.memberType.unique);
		}
		return true;
	};
}

/** @deprecated Use `UmbMemberPickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbMemberPickerInputContext as UmbMemberPickerContext };
