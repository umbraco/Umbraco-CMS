import type { UmbBlockDataType } from '../../block/index.js';
import { UMB_BLOCK_CATALOGUE_MODAL, UmbBlockEntriesContext } from '../../block/index.js';
import { UMB_BLOCK_GRID_ENTRY_CONTEXT, type UmbBlockGridWorkspaceData } from '../index.js';
import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

export class UmbBlockGridEntriesContext extends UmbBlockEntriesContext<
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT,
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE,
	UmbBlockGridTypeModel,
	UmbBlockGridLayoutModel
> {
	//
	#catalogueModal: UmbModalRouteRegistrationController<typeof UMB_BLOCK_CATALOGUE_MODAL.DATA, undefined>;
	#catalogueRouteBuilder?: UmbModalRouteBuilder;

	#parentEntry?: typeof UMB_BLOCK_GRID_ENTRY_CONTEXT.TYPE;
	#retrieveParentEntry;

	//#parentUnique?: string;
	#areaKey?: string | null;

	setParentUnique(contentUdi: string | null) {
		this.#catalogueModal.setUniquePathValue('parentUnique', contentUdi ?? 'null');
	}

	setAreaKey(areaKey: string | null) {
		this.#areaKey = areaKey;
		this.#catalogueModal.setUniquePathValue('areaKey', areaKey ?? 'null');
		this.#gotAreaKey();
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT);

		this.#retrieveParentEntry = this.consumeContext(UMB_BLOCK_GRID_ENTRY_CONTEXT, (blockGridEntry) => {
			this.#parentEntry = blockGridEntry;
			this.#gotBlockParentEntry();
		}).asPromise();

		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId', 'parentUnique', 'areaKey'])
			.addAdditionalPath(':view/:index')
			.onSetup((routingInfo) => {
				// Idea: Maybe on setup should be async, so it can retrieve the values when needed? [NL]
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				return {
					data: {
						blocks: [],
						blockGroups: [],
						openClipboard: routingInfo.view === 'clipboard',
						blockOriginData: { index: index },
					},
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#catalogueRouteBuilder = routeBuilder;
				// TODO: Trigger render update?
			});
	}

	protected _gotBlockManager() {
		if (!this._manager) return;

		this.observe(
			this._manager.propertyAlias,
			(alias) => {
				this.#catalogueModal.setUniquePathValue('propertyAlias', alias ?? 'null');
			},
			'observePropertyAlias',
		);

		this.observe(
			this._manager.variantId,
			(variantId) => {
				if (variantId) {
					this.#catalogueModal.setUniquePathValue('variantId', variantId.toString());
				}
			},
			'observePropertyAlias',
		);
	}

	#gotBlockParentEntry() {
		if (!this.#parentEntry) return;
	}

	async #gotAreaKey() {
		if (this.#areaKey === undefined) return;

		if (this.#areaKey === null) {
			// Root entries:
			await this._retrieveManager;
			if (!this._manager) return;

			this.setParentUnique(null);
			this.observe(this._manager.layouts, (layouts) => {
				this._layoutEntries.setValue(layouts);
			});
			this.observe(this.layoutEntries, (layouts) => {
				this._manager?.setLayouts(layouts);
			});
		} else {
			// entries of a area:
			await this.#retrieveParentEntry;
			if (!this.#parentEntry) return;

			this.observe(this.#parentEntry.unique, (unique) => {
				this.setParentUnique(unique ?? null);
			});
			this.observe(this.#parentEntry.layoutsOfArea(this.#areaKey), (layouts) => {
				this._layoutEntries.setValue(layouts);
			});

			this.observe(this.layoutEntries, (layouts) => {
				if (this.#areaKey) {
					this.#parentEntry?.setLayoutsOfArea(this.#areaKey, layouts);
				}
			});
		}
	}

	getPathForCreateBlock(index: number) {
		return this.#catalogueRouteBuilder?.({ view: 'create', index: index });
	}

	getPathForClipboard(index: number) {
		return this.#catalogueRouteBuilder?.({ view: 'clipboard', index: index });
	}

	async create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<UmbBlockGridLayoutModel, 'contentUdi'>,
		modalData?: UmbBlockGridWorkspaceData,
	) {
		await this._retrieveManager;
		return this._manager?.create(contentElementTypeKey, partialLayoutEntry, modalData);
	}

	// insert Block?

	async insert(
		layoutEntry: UmbBlockGridLayoutModel,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		modalData: UmbBlockGridWorkspaceData,
	) {
		await this._retrieveManager;
		// TODO: Insert layout entry at the right spot.
		return this._manager?.insert(layoutEntry, content, settings, modalData) ?? false;
	}

	// create Block?
	async delete(contentUdi: string) {
		// TODO: Loop through children and delete them as well?
		await super.delete(contentUdi);
	}
}
