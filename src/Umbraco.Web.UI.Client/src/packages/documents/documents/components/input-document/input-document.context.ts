import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from '../../modals/types.js';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../../constants.js';
import type { UmbDocumentItemModel } from '../../item/types.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

interface UmbDocumentPickerInputContextOpenArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>;
	includeTrashed?: boolean;
}

export class UmbDocumentPickerInputContext extends UmbPickerInputContext<
	UmbDocumentItemModel,
	UmbDocumentTreeItemModel,
	UmbDocumentPickerModalData,
	UmbDocumentPickerModalValue
> {
	#init: Promise<unknown>;
	#propertyDataSetCulture?: UmbVariantId;
	#appCulture?: string;
	#defaultCulture?: string;

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_PICKER_MODAL);

		// TODO: replace this with a UMB_VARIANT_CONTEXT
		// We do not depend on this context because we know is it only available in some cases
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#propertyDataSetCulture = context?.getVariantId();
		});

		// TODO: replace this with a UMB_VARIANT_CONTEXT
		this.#init = this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.observe(context?.appLanguageCulture, (culture) => {
				this.#appCulture = culture;
			});

			this.observe(context?.appDefaultLanguage, (value) => {
				this.#defaultCulture = value?.unique;
			});
		}).asPromise();
	}

	override async openPicker(
		pickerData?: Partial<UmbDocumentPickerModalData>,
		args?: UmbDocumentPickerInputContextOpenArgs,
	) {
		await this.#init;

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

		// pass allowedContentTypes to the search request args
		combinedPickerData.search!.queryParams = {
			allowedContentTypes: args?.allowedContentTypes,
			includeTrashed: args?.includeTrashed,
			culture: this.#propertyDataSetCulture?.culture || this.#appCulture || this.#defaultCulture,
			...pickerData?.search?.queryParams,
		};

		await super.openPicker(combinedPickerData);
	}

	#pickableFilter = (
		item: UmbDocumentItemModel,
		allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>,
	): boolean => {
		if (allowedContentTypes && allowedContentTypes.length > 0) {
			return allowedContentTypes
				.map((contentTypeReference) => contentTypeReference.unique)
				.includes(item.documentType.unique);
		}
		return true;
	};
}
