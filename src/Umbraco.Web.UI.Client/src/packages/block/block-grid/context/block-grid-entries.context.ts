import type { UmbBlockDataType } from '../../block/index.js';
import { UMB_BLOCK_CATALOGUE_MODAL, UmbBlockEntriesContext } from '../../block/index.js';
import {
	UMB_BLOCK_GRID_ENTRY_CONTEXT,
	UMB_BLOCK_GRID_WORKSPACE_MODAL,
	type UmbBlockGridWorkspaceOriginData,
} from '../index.js';
import type { UmbBlockGridLayoutModel, UmbBlockGridTypeAreaType, UmbBlockGridTypeModel } from '../types.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context-token.js';
import type { UmbBlockGridScalableContainerContext } from './block-grid-scale-manager/block-grid-scale-manager.controller.js';
import { UmbArrayState, UmbNumberState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { pathFolderName } from '@umbraco-cms/backoffice/utils';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';

export class UmbBlockGridEntriesContext
	extends UmbBlockEntriesContext<
		typeof UMB_BLOCK_GRID_MANAGER_CONTEXT,
		typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE,
		UmbBlockGridTypeModel,
		UmbBlockGridLayoutModel,
		UmbBlockGridWorkspaceOriginData
	>
	implements UmbBlockGridScalableContainerContext
{
	//
	#catalogueModal: UmbModalRouteRegistrationController<
		typeof UMB_BLOCK_CATALOGUE_MODAL.DATA,
		typeof UMB_BLOCK_CATALOGUE_MODAL.VALUE
	>;
	#workspaceModal;

	#parentEntry?: typeof UMB_BLOCK_GRID_ENTRY_CONTEXT.TYPE;

	#layoutColumns = new UmbNumberState(undefined);
	readonly layoutColumns = this.#layoutColumns.asObservable();

	#areaType?: UmbBlockGridTypeAreaType;

	#parentUnique?: string | null;
	#areaKey?: string | null;

	#rangeLimits = new UmbObjectState<UmbNumberRangeValueType | undefined>(undefined);
	readonly rangeLimits = this.#rangeLimits.asObservable();

	#allowedBlockTypes = new UmbArrayState<UmbBlockGridTypeModel>([], (x) => x.contentElementTypeKey);
	public readonly allowedBlockTypes = this.#allowedBlockTypes.asObservable();
	public readonly amountOfAllowedBlockTypes = this.#allowedBlockTypes.asObservablePart((x) => x.length);
	public readonly canCreate = this.#allowedBlockTypes.asObservablePart((x) => x.length > 0);

	firstAllowedBlockTypeName() {
		if (!this._manager) {
			throw new Error('Manager not ready');
		}

		const nameState = new UmbStringState(undefined);
		this.observe(this.allowedBlockTypes, (x) => {
			if (x.length === 1) {
				this.observe(
					this._manager!.contentTypeNameOf(x[0].contentElementTypeKey),
					(name) => {
						nameState.setValue(name);
					},
					'getFirstAllowedBlockTypeName',
				);
			} else {
				this.removeUmbControllerByAlias('getFirstAllowedBlockTypeName');
			}
		});

		return nameState.asObservable();
	}

	setParentUnique(contentUdi: string | null) {
		this.#parentUnique = contentUdi;
		// Notice pathFolderName can be removed when we have switched to use a proper GUID/ID/KEY. [NL]
		this.#workspaceModal.setUniquePathValue('parentUnique', pathFolderName(contentUdi ?? 'null'));
		this.#catalogueModal.setUniquePathValue('parentUnique', pathFolderName(contentUdi ?? 'null'));
	}

	setAreaKey(areaKey: string | null) {
		this.#areaKey = areaKey;
		this.#workspaceModal.setUniquePathValue('areaKey', areaKey ?? 'null');
		this.#catalogueModal.setUniquePathValue('areaKey', areaKey ?? 'null');
		this.#gotAreaKey();
	}

	setLayoutColumns(columns: number | undefined) {
		this.#layoutColumns.setValue(columns);
	}
	getLayoutColumns() {
		return this.#layoutColumns.getValue();
	}

	getMinAllowed() {
		if (this.#areaKey) {
			return this.#areaType?.minAllowed ?? 0;
		}
		return this._manager?.getMinAllowed() ?? 0;
	}

	getMaxAllowed() {
		if (this.#areaKey) {
			return this.#areaType?.maxAllowed ?? Infinity;
		}
		return this._manager?.getMaxAllowed() ?? Infinity;
	}

	getLayoutContainerElement() {
		return this.getHostElement().shadowRoot?.querySelector('.umb-block-grid__layout-container') as
			| HTMLElement
			| undefined;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT);

		this.consumeContext(UMB_BLOCK_GRID_ENTRY_CONTEXT, (blockGridEntry) => {
			this.#parentEntry = blockGridEntry;
			this.#gotBlockParentEntry(); // is not used at this point. [NL]
		});

		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId', 'parentUnique', 'areaKey'])
			.addAdditionalPath(':view/:index')
			.onSetup((routingInfo) => {
				// Idea: Maybe on setup should be async, so it can retrieve the values when needed? [NL]
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				return {
					data: {
						blocks: this.#allowedBlockTypes.getValue(),
						blockGroups: this._manager?.getBlockGroups() ?? [],
						openClipboard: routingInfo.view === 'clipboard',
						originData: { index: index, areaKey: this.#areaKey, parentUnique: this.#parentUnique },
						createBlockInWorkspace: true,
					},
				};
			})
			.onSubmit(async (value, data) => {
				if (value?.create && data) {
					const created = await this.create(
						value.create.contentElementTypeKey,
						// We can parse an empty object, cause the rest will be filled in by others.
						{} as any,
						data.originData as UmbBlockGridWorkspaceOriginData,
					);
					if (created) {
						this.insert(
							created.layout,
							created.content,
							created.settings,
							data.originData as UmbBlockGridWorkspaceOriginData,
						);
					} else {
						throw new Error('Failed to create block');
					}
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				// TODO: Does it make any sense that this is a state? Check usage and confirm. [NL]
				this._catalogueRouteBuilderState.setValue(routeBuilder);
			});

		this.#workspaceModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_GRID_WORKSPACE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId', 'parentUnique', 'areaKey'])
			.addAdditionalPath('block')
			.onSetup(() => {
				return {
					data: {
						entityType: 'block',
						preset: {},
						originData: { areaKey: this.#areaKey, parentUnique: this.#parentUnique, baseDataPath: this._dataPath },
					},
					modal: { size: 'medium' },
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				const newPath = routeBuilder({});
				this._workspacePath.setValue(newPath);
			});
	}

	protected _gotBlockManager() {
		if (!this._manager) return;

		this.#setupAllowedBlockTypes();
		this.#setupRangeLimits();

		this.observe(
			this._manager.propertyAlias,
			(alias) => {
				this.#catalogueModal.setUniquePathValue('propertyAlias', alias ?? 'null');
				this.#workspaceModal.setUniquePathValue('propertyAlias', alias ?? 'null');
			},
			'observePropertyAlias',
		);

		this.observe(
			this._manager.variantId,
			(variantId) => {
				// TODO: This might not be the property variant ID, but the content variant ID. Check up on what makes most sense?
				this.#catalogueModal.setUniquePathValue('variantId', variantId?.toString());
				this.#workspaceModal.setUniquePathValue('variantId', variantId?.toString());
			},
			'observePropertyAlias',
		);
	}

	#gotAreaKey() {
		if (this.#areaKey === undefined) return;
		this.#gotBlockParentEntry();
	}

	async #gotBlockParentEntry() {
		if (this.#areaKey === undefined) return;

		if (this.#areaKey === null) {
			// Root entries:
			await this._retrieveManager;
			if (!this._manager) return;

			this.removeUmbControllerByAlias('observeParentUnique');
			this.setParentUnique(null);
			this.observe(
				this._manager.layouts,
				(layouts) => {
					this._layoutEntries.setValue(layouts);
				},
				'observeParentLayouts',
			);
			this.observe(
				this.layoutEntries,
				(layouts) => {
					this._manager?.setLayouts(layouts);
				},
				'observeThisLayouts',
			);

			this.removeUmbControllerByAlias('observeAreaType');

			const hostEl = this.getHostElement() as HTMLElement | undefined;
			if (hostEl) {
				hostEl.removeAttribute('data-area-alias');
				hostEl.removeAttribute('data-area-col-span');
				hostEl.removeAttribute('data-area-row-span');
				hostEl.style.removeProperty('--umb-block-grid--grid-columns');
				hostEl.style.removeProperty('--umb-block-grid--area-column-span');
				hostEl.style.removeProperty('--umb-block-grid--area-row-span');
			}

			this.removeUmbControllerByAlias('observeAreaType');
			this.#setupAllowedBlockTypes();
			this.#setupRangeLimits();
		} else {
			if (!this.#parentEntry) return;

			this.observe(
				this.#parentEntry.unique,
				(unique) => {
					this.setParentUnique(unique ?? null);
				},
				'observeParentUnique',
			);
			this.observe(
				this.#parentEntry.layoutsOfArea(this.#areaKey),
				(layouts) => {
					if (layouts) {
						this._layoutEntries.setValue(layouts);
					}
				},
				'observeParentLayouts',
			);

			this.observe(
				this.layoutEntries,
				(layouts) => {
					if (this.#areaKey) {
						this.#parentEntry?.setLayoutsOfArea(this.#areaKey, layouts);
					}
				},
				'observeThisLayouts',
			);

			this.observe(
				this.#parentEntry.areaType(this.#areaKey),
				(areaType) => {
					this.#areaType = areaType;
					const hostEl = this.getHostElement() as HTMLElement | undefined;
					if (!hostEl) return;
					hostEl.setAttribute('data-area-alias', areaType?.alias ?? '');
					hostEl.setAttribute('data-area-col-span', areaType?.columnSpan?.toString() ?? '');
					hostEl.setAttribute('data-area-row-span', areaType?.rowSpan?.toString() ?? '');
					hostEl.style.setProperty('--umb-block-grid--grid-columns', areaType?.columnSpan?.toString() ?? '');
					hostEl.style.setProperty('--umb-block-grid--area-column-span', areaType?.columnSpan?.toString() ?? '');
					hostEl.style.setProperty('--umb-block-grid--area-row-span', areaType?.rowSpan?.toString() ?? '');
					this.#setupAllowedBlockTypes();
					this.#setupRangeLimits();
				},
				'observeAreaType',
			);
		}
	}

	#setupAllowedBlockTypes() {
		if (!this._manager) return;
		this.#allowedBlockTypes.setValue(this.#retrieveAllowedElementTypes());
	}
	#setupRangeLimits() {
		if (!this._manager) return;
		//const range = this.#retrieveRangeLimits();
		if (this.#areaKey != null) {
			this.removeUmbControllerByAlias('observeConfigurationRootLimits');
			// Area entries:
			if (!this.#areaType) return undefined;
			// No need to observe as this method is called every time the area is changed.
			this.#rangeLimits.setValue({
				min: this.#areaType.minAllowed ?? 0,
				max: this.#areaType.maxAllowed ?? Infinity,
			});
		} else if (this.#areaKey === null) {
			if (!this._manager) return undefined;

			this.observe(
				this._manager.editorConfiguration,
				(config) => {
					const min = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit')?.min ?? 0;
					const max = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit')?.max ?? Infinity;
					this.#rangeLimits.setValue({ min, max });
				},
				'observeConfigurationRootLimits',
			);
		}
	}

	getPathForCreateBlock(index: number) {
		return this._catalogueRouteBuilderState.getValue()?.({ view: 'create', index: index });
	}

	getPathForClipboard(index: number) {
		return this._catalogueRouteBuilderState.getValue()?.({ view: 'clipboard', index: index });
	}

	/*
	async setLayouts(layouts: Array<UmbBlockGridLayoutModel>) {
		await this._retrieveManager;
		if (this.#areaKey === null) {
			this._manager?.setLayouts(layouts);
		} else {
			if (!this.#parentUnique || !this.#areaKey) {
				throw new Error('ParentUnique or AreaKey not set');
			}
			this._manager?.setLayoutsOfArea(this.#parentUnique, this.#areaKey, layouts);
		}
	}
	*/

	async create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<UmbBlockGridLayoutModel, 'contentUdi'>,
		originData?: UmbBlockGridWorkspaceOriginData,
	) {
		await this._retrieveManager;
		return this._manager?.create(contentElementTypeKey, partialLayoutEntry, originData);
	}

	// insert Block?

	async insert(
		layoutEntry: UmbBlockGridLayoutModel,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		originData: UmbBlockGridWorkspaceOriginData,
	) {
		await this._retrieveManager;
		// TODO: Insert layout entry at the right spot.
		return this._manager?.insert(layoutEntry, content, settings, originData) ?? false;
	}

	// create Block?
	override async delete(contentUdi: string) {
		// TODO: Loop through children and delete them as well?
		await super.delete(contentUdi);
	}

	/**
	 * @internal
	 * @returns an Array of ElementTypeKeys that are allowed in the current area. Or undefined if not ready jet.
	 */
	#retrieveAllowedElementTypes() {
		if (!this._manager) return [];

		if (this.#areaKey) {
			// Area entries:
			if (!this.#areaType) return [];

			if (this.#areaType.specifiedAllowance && this.#areaType.specifiedAllowance?.length > 0) {
				return (
					this.#areaType.specifiedAllowance
						.flatMap((permission) => {
							if (permission.groupKey) {
								return (
									this._manager?.getBlockTypes().filter((blockType) => blockType.groupKey === permission.groupKey) ?? []
								);
							} else if (permission.elementTypeKey) {
								return (
									this._manager?.getBlockTypes().filter((x) => x.contentElementTypeKey === permission.elementTypeKey) ??
									[]
								);
							}
							return [];
						})
						// Remove duplicates:
						.filter((v, i, a) => a.findIndex((x) => x.contentElementTypeKey === v.contentElementTypeKey) === i)
				);
			}

			// No specific permissions setup, so we will fallback to items allowed in areas:
			return this._manager.getBlockTypes().filter((x) => x.allowInAreas);
		} else if (this.#areaKey === null) {
			// If AreaKey is null, then we are in the root, looking for items allowed as root:
			return this._manager.getBlockTypes().filter((x) => x.allowAtRoot);
		}

		return [];
	}

	/**
	 * Check if given contentUdi is allowed in the current area.
	 * @param contentUdi {string} - The contentUdi of the content to check.
	 * @returns {boolean} - True if the content is allowed in the current area, otherwise false.
	 */
	allowDrop(contentUdi: string) {
		const content = this._manager?.getContentOf(contentUdi);
		const allowedBlocks = this.#allowedBlockTypes.getValue();
		if (!content || !allowedBlocks) return false;

		return allowedBlocks.map((x) => x.contentElementTypeKey).indexOf(content.contentTypeKey) !== -1;
	}

	onDragStart() {
		this._manager?.onDragStart();
	}

	onDragEnd() {
		this._manager?.onDragEnd();
	}
}
