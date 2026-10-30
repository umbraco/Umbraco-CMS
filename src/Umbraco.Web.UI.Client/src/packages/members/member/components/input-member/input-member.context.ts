import type { UmbMemberItemModel } from '../../item/types.js';
import type { UmbMemberSearchItemModel } from '../../search/types.js';
import type { UmbMemberDetailModel } from '../../types.js';
import {
	UMB_MEMBER_PICKER_MODAL,
	type UmbMemberPickerModalData,
	type UmbMemberPickerModalValue,
} from '../member-picker-modal/member-picker-modal.token.js';
import { UMB_MEMBER_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_SEARCH_PROVIDER_ALIAS } from '../../constants.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPickerModalSearchConfig } from '@umbraco-cms/backoffice/modal';
import type { UmbMemberTypeEntityType } from '@umbraco-cms/backoffice/member-type';

interface UmbMemberPickerInputContextOpenArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbMemberTypeEntityType }>;
}

export class UmbMemberPickerInputContext extends UmbPickerInputContext<
	UmbMemberItemModel,
	UmbMemberDetailModel,
	UmbMemberPickerModalData,
	UmbMemberPickerModalValue
> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_PICKER_MODAL);
	}

	override async openPicker(
		pickerData?: Partial<Omit<UmbMemberPickerModalData, 'search'>> & {
			search?: Partial<UmbPickerModalSearchConfig<Record<string, unknown>, UmbMemberSearchItemModel>>;
		},
		args?: UmbMemberPickerInputContextOpenArgs,
	) {
		const combinedPickerData: Partial<UmbMemberPickerModalData> = {
			...pickerData,
			// combine internal allowedContentTypes filter with user-supplied pickableFilter
			pickableFilter: this._combinePickableFilters(
				(item) => this.#pickableFilter(item, args?.allowedContentTypes),
				pickerData?.pickableFilter,
			),
			search: {
				providerAlias: UMB_MEMBER_SEARCH_PROVIDER_ALIAS,
				...pickerData?.search,
				// pass allowedContentTypes to the search request args
				queryParams: {
					allowedContentTypes: args?.allowedContentTypes,
					...pickerData?.search?.queryParams,
				},
			},
		};

		await super.openPicker(combinedPickerData);
	}

	#pickableFilter = (
		item: UmbMemberDetailModel,
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
