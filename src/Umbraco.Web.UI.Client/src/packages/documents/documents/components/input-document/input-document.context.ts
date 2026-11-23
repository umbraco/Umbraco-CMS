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
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDocumentPickerInputSelectedItemAncestorsModel {
	treeItem: { unique: string; entityType: string };
	/**
	 * The ancestors ordered from the root down, or `null` when they were requested but none were returned.
	 */
	ancestors: Array<UmbDocumentTreeItemModel> | null;
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
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#ancestorChains = new UmbArrayState<UmbDocumentPickerInputSelectedItemAncestorsModel>([], (x) => x.treeItem.unique);
	#pendingAncestors = new Set<string>();
	#dataType?: { unique: string };

	/**
	 * The ancestors of each selected document whose ancestors have been requested. A document whose ancestors have not
	 * been requested yet has no entry.
	 */
	public readonly selectedItemsAncestors = this.#ancestorChains.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_PICKER_MODAL);

		this.observe(this.selectedItems, (items) => this.#onSelectedItemsChange(items), null);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext?.removeEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureForEntityRequest as EventListener,
			);
			this.#actionEventContext = instance;
			this.#actionEventContext?.addEventListener(
				UmbRequestReloadStructureForEntityEvent.TYPE,
				this.#onReloadStructureForEntityRequest as EventListener,
			);
		});

		// TODO: Receive the data type from the consumer instead, so the picker does not depend on property contexts. The
		// picker modal's tree and picker contexts consume it the same way and need a fallback and deprecation period to
		// move with it. [MR]
		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(
				context?.dataType,
				(dataType) => {
					if (dataType?.unique === this.#dataType?.unique) return;
					this.#dataType = dataType;
					this.#requestAncestors(this.#ancestorChains.getValue().map((chain) => chain.treeItem));
				},
				'umbObserveDataType',
			);
		});
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

	readonly #onReloadStructureForEntityRequest = (event: UmbRequestReloadStructureForEntityEvent) => {
		const entityType = event.getEntityType();
		const unique = event.getUnique();
		const isEntity = (entity: UmbEntityModel) => entity.entityType === entityType && entity.unique === unique;

		const affected = this.#ancestorChains
			.getValue()
			.filter((chain) => isEntity(chain.treeItem) || chain.ancestors?.some(isEntity))
			.map((chain) => chain.treeItem);
		this.#requestAncestors(affected);
	};

	async #requestAncestors(treeItems: Array<{ unique: string; entityType: string }>) {
		if (!treeItems.length) return;
		treeItems.forEach((treeItem) => this.#pendingAncestors.add(treeItem.unique));

		const { data } = await this.#treeRepository.requestAncestorsOfItems({ treeItems, dataType: this.#dataType });

		treeItems.forEach((treeItem) => this.#pendingAncestors.delete(treeItem.unique));
		if (!data) return;

		const uniques = this.getSelectedItems().map((item) => item.unique);
		const ancestorsByUnique = new Map(data.map((chain) => [chain.treeItem.unique, chain.ancestors]));
		this.#ancestorChains.append(
			treeItems
				.filter((treeItem) => uniques.includes(treeItem.unique))
				.map((treeItem) => ({ treeItem, ancestors: ancestorsByUnique.get(treeItem.unique) ?? null })),
		);
	}

	override destroy() {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadStructureForEntityRequest as EventListener,
		);
		this.#actionEventContext = undefined;
		super.destroy();
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
