import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_MEDIA_PICKER_MODAL } from '../../modals/index.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from '../../modals/index.js';
import { UMB_MEDIA_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';
import type { UmbMediaTreeItemModel } from '../../tree/types.js';
import { isMediaTreeItem } from '../../tree/utils.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMediaTypeStructureRepository, type UmbMediaTypeEntityType } from '@umbraco-cms/backoffice/media-type';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';

export enum UmbMediaPickerFolderFilter {
	FILES_ONLY = 'filesOnly',
	FOLDERS_ONLY = 'foldersOnly',
	FILES_AND_FOLDERS = 'filesAndFolders',
}

interface UmbMediaPickerInputContextOpenArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbMediaTypeEntityType }>;
	includeTrashed?: boolean;
	folderFilter?: UmbMediaPickerFolderFilter;
}

export class UmbMediaPickerInputContext extends UmbPickerInputContext<
	UmbMediaItemModel,
	UmbMediaTreeItemModel,
	UmbMediaPickerModalData,
	UmbMediaPickerModalValue
> {
	#mediaTypeStructureRepository;
	#folderTypeUniques = new Set<string>();
	#folderTypesPromise: Promise<void> | null = null;

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_PICKER_MODAL);
		this.#mediaTypeStructureRepository = new UmbMediaTypeStructureRepository(host);
	}

	override async openPicker(pickerData?: Partial<UmbMediaPickerModalData>, args?: UmbMediaPickerInputContextOpenArgs) {
		// Load folder types before opening the picker so the filter is ready
		await this.#loadFolderTypes();
		const combinedPickerData = {
			...pickerData,
		};

		// combine internal allowedContentTypes filter with user-supplied pickableFilter
		combinedPickerData.pickableFilter = this._combinePickableFilters(
			(item) => this.#pickableFilter(item, args?.allowedContentTypes, args?.folderFilter),
			pickerData?.pickableFilter,
		);

		// set default search data
		if (!pickerData?.search) {
			combinedPickerData.search = {
				providerAlias: UMB_MEDIA_SEARCH_PROVIDER_ALIAS,
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

	async #loadFolderTypes() {
		if (!this.#folderTypesPromise) {
			this.#folderTypesPromise = this.#mediaTypeStructureRepository.requestMediaTypesOfFolders().then((folderTypes) => {
				this.#folderTypeUniques = new Set(folderTypes.map((ft) => ft.unique).filter((u): u is string => u != null));
			});
		}
		return this.#folderTypesPromise;
	}

	#pickableFilter = (
		item: UmbMediaItemModel | UmbMediaTreeItemModel,
		allowedContentTypes?: Array<{ unique: string; entityType: UmbMediaTypeEntityType }>,
		folderFilter: UmbMediaPickerFolderFilter = UmbMediaPickerFolderFilter.FILES_ONLY,
	): boolean => {
		// Check if the user has no access to this item (tree items only)
		if (isMediaTreeItem(item) && item.noAccess) {
			return false;
		}

		const isFolder = this.#folderTypeUniques.has(item.mediaType.unique);

		// Apply folder filter
		if (folderFilter === UmbMediaPickerFolderFilter.FILES_ONLY && isFolder) {
			return false;
		}
		if (folderFilter === UmbMediaPickerFolderFilter.FOLDERS_ONLY && !isFolder) {
			return false;
		}
		// FILES_AND_FOLDERS — no folder-level filtering

		if (allowedContentTypes && allowedContentTypes.length > 0) {
			return allowedContentTypes
				.map((contentTypeReference) => contentTypeReference.unique)
				.includes(item.mediaType.unique);
		}
		return true;
	};
}
