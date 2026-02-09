import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from '../../modals/types.js';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../../constants.js';
import type { UmbDocumentItemModel } from '../../item/types.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { isDocumentTreeItem } from '../../tree/utils.js';
import { UmbDocumentItemDataResolver } from '../../item/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';

interface UmbDocumentPickerInputContextOpenArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>;
	includeTrashed?: boolean;
}

export class UmbDocumentPickerInputContext extends UmbPickerInputContext<
	UmbDocumentItemModel & { name: string }, // HACK: [LK:2025-01-01]
	UmbDocumentTreeItemModel,
	UmbDocumentPickerModalData,
	UmbDocumentPickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_PICKER_MODAL);
	}

	override async openPicker(
		pickerData?: Partial<UmbDocumentPickerModalData>,
		args?: UmbDocumentPickerInputContextOpenArgs,
	) {
		const combinedPickerData = {
			...pickerData,
		};

		// transform allowedContentTypes to a pickable filter
		combinedPickerData.pickableFilter = (item) => this.#pickableFilter(item, args?.allowedContentTypes);

		// set default search data
		if (!pickerData?.search) {
			combinedPickerData.search = {
				providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
				...pickerData?.search,
			};
		}

		const variantContext = await this.getContext(UMB_VARIANT_CONTEXT);
		const culture = await variantContext?.getDisplayCulture();

		// pass allowedContentTypes to the search request args
		combinedPickerData.search!.queryParams = {
			allowedContentTypes: args?.allowedContentTypes,
			includeTrashed: args?.includeTrashed,
			culture,
			...pickerData?.search?.queryParams,
		};

		await super.openPicker(combinedPickerData);
	}

	protected override async _requestItemName(unique: string): Promise<string> {
		const item = this.getSelectedItemByUnique(unique);
		const resolver = new UmbDocumentItemDataResolver(this);
		resolver.setData(item);
		const name = await resolver.getName();
		this.removeUmbController(resolver);
		return name ?? '#general_notFound';
	}

	#pickableFilter = (
		item: UmbDocumentItemModel | UmbDocumentTreeItemModel,
		allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>,
	): boolean => {
		// Check if the user has no access to this item (tree items only)
		if (isDocumentTreeItem(item) && item.noAccess) {
			return false;
		}
		if (allowedContentTypes && allowedContentTypes.length > 0) {
			return allowedContentTypes
				.map((contentTypeReference) => contentTypeReference.unique)
				.includes(item.documentType.unique);
		}
		return true;
	};
}
