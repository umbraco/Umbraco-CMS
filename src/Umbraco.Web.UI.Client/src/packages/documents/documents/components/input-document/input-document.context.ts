import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from '../../modals/types.js';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../../constants.js';
import type { UmbDocumentItemModel } from '../../item/types.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { isDocumentTreeItem } from '../../tree/utils.js';
import { UmbDocumentItemDataResolver } from '../../item/index.js';
import { UmbDocumentTreeRepository } from '../../tree/document-tree.repository.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import { UmbArrayState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import type { UmbAncestorsOfTreeItemModel } from '@umbraco-cms/backoffice/tree';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

export interface UmbDocumentPickerInputSelectedItemAncestorsModel {
	unique: string;
	ancestors: Array<UmbDocumentItemModel>;
}

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
	#treeRepository = new UmbDocumentTreeRepository(this);
	#ancestorChains = new UmbArrayState<UmbAncestorsOfTreeItemModel>([], (x) => x.treeItem.unique);
	#ancestorItems = new UmbRepositoryItemsManager<UmbDocumentItemModel>(this, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS);
	#pendingAncestors = new Set<string>();

	/**
	 * The ancestors of each selected document, ordered from the root down. A document is included once the items of
	 * all its ancestors are resolved.
	 */
	public readonly selectedItemsAncestors = mergeObservables(
		[this.#ancestorChains.asObservable(), this.#ancestorItems.items],
		([chains, items]): Array<UmbDocumentPickerInputSelectedItemAncestorsModel> => {
			const itemByUnique = new Map<UmbEntityUnique, UmbDocumentItemModel>(items.map((item) => [item.unique, item]));
			return chains.flatMap((chain) => {
				const ancestors = chain.ancestors.map((ancestor) => itemByUnique.get(ancestor.unique));
				if (ancestors.some((item) => !item)) return [];
				return [{ unique: chain.treeItem.unique, ancestors: ancestors as Array<UmbDocumentItemModel> }];
			});
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_PICKER_MODAL);

		this.observe(this.selectedItems, (items) => this.#onSelectedItemsChange(items), null);

		this.observe(
			this.#ancestorChains.asObservable(),
			(chains) => {
				const uniques = chains.flatMap((chain) =>
					chain.ancestors.map((ancestor) => ancestor.unique).filter((unique) => unique !== null),
				);
				this.#ancestorItems.setUniques([...new Set(uniques)]);
			},
			null,
		);
	}

	#onSelectedItemsChange(items: Array<UmbEntityModel>) {
		const uniques = items.map((item) => item.unique);
		this.#ancestorChains.filter((chain) => uniques.includes(chain.treeItem.unique));
		const missing = items.filter(
			(item): item is UmbEntityModel & { unique: string } =>
				item.unique !== null &&
				!this.#ancestorChains.getHasOne(item.unique) &&
				!this.#pendingAncestors.has(item.unique),
		);
		this.#requestAncestors(missing.map((item) => ({ unique: item.unique, entityType: item.entityType })));
	}

	async #requestAncestors(treeItems: Array<{ unique: string; entityType: string }>) {
		if (!treeItems.length) return;
		treeItems.forEach((treeItem) => this.#pendingAncestors.add(treeItem.unique));

		const { data } = await this.#treeRepository.requestAncestorsOfItems({ treeItems });

		treeItems.forEach((treeItem) => this.#pendingAncestors.delete(treeItem.unique));
		if (!data) return;

		const uniques = this.getSelectedItems().map((item) => item.unique);
		this.#ancestorChains.append(data.filter((chain) => uniques.includes(chain.treeItem.unique)));
	}

	override async openPicker(
		pickerData?: Partial<UmbDocumentPickerModalData>,
		args?: UmbDocumentPickerInputContextOpenArgs,
	) {
		const combinedPickerData = {
			...pickerData,
		};

		// combine internal allowedContentTypes filter with user-supplied pickableFilter
		combinedPickerData.pickableFilter = this._combinePickableFilters(
			(item) => this.#pickableFilter(item, args?.allowedContentTypes),
			pickerData?.pickableFilter,
		);

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

	/**
	 * Resolves the display name for a picked document, using the variant aware item data resolver.
	 * @param {string} unique The unique identifier of the document.
	 * @returns {Promise<string>} The resolved display name.
	 * @memberof UmbDocumentPickerInputContext
	 */
	protected override async _requestItemName(unique: string): Promise<string> {
		const item = this.getSelectedItemByUnique(unique);

		// A selection can hold uniques that no longer resolve to an item. The resolver cannot name
		// something it has no data for, so let the base implementation supply the fallback label.
		if (item) {
			const resolver = new UmbDocumentItemDataResolver(this);
			resolver.setData(item);
			try {
				const name = await resolver.getName();
				if (name) {
					return name;
				}
			} finally {
				this.removeUmbController(resolver);
			}
		}

		return super._requestItemName(unique);
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
