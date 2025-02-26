import type { UmbBlockDataModel } from '../../block/index.js';
import { UMB_BLOCK_CATALOGUE_MODAL, UmbBlockEntriesContext } from '../../block/index.js';
import {
	UMB_BLOCK_GRID_ENTRY_CONTEXT,
	UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
	UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
	UMB_BLOCK_GRID_WORKSPACE_MODAL,
	type UmbBlockGridWorkspaceOriginData,
} from '../index.js';
import type {
	UmbBlockGridLayoutModel,
	UmbBlockGridTypeAreaType,
	UmbBlockGridTypeModel,
	UmbBlockGridValueModel,
} from '../types.js';
import { forEachBlockLayoutEntryOf } from '../utils/index.js';
import type { UmbBlockGridPropertyEditorConfig } from '../property-editors/block-grid-editor/types.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context-token.js';
import type { UmbBlockGridScalableContainerContext } from './block-grid-scale-manager/block-grid-scale-manager.controller.js';
import {
	UmbArrayState,
	UmbBooleanState,
	UmbNumberState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalRouteRegistrationController, UmbRoutePathAddendumContext } from '@umbraco-cms/backoffice/router';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import {
	UMB_CLIPBOARD_PROPERTY_CONTEXT,
	UmbClipboardPastePropertyValueTranslatorValueResolver,
} from '@umbraco-cms/backoffice/clipboard';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

interface UmbBlockGridAreaTypeInvalidRuleType {
	groupKey?: string;
	key?: string;
	name: string;
	amount: number;
	minRequirement: number;
	maxRequirement: number;
}

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
	#pathAddendum = new UmbRoutePathAddendumContext(this);

	#parentEntry?: typeof UMB_BLOCK_GRID_ENTRY_CONTEXT.TYPE;

	#layoutColumns = new UmbNumberState(undefined);
	readonly layoutColumns = this.#layoutColumns.asObservable();

	#areaType = new UmbObjectState<UmbBlockGridTypeAreaType | undefined>(undefined);
	areaType = this.#areaType.asObservable();
	areaTypeCreateLabel = this.#areaType.asObservablePart((x) => x?.createLabel);

	#parentUnique?: string | null;
	#areaKey?: string | null;

	#rangeLimits = new UmbObjectState<UmbNumberRangeValueType | undefined>(undefined);
	readonly rangeLimits = this.#rangeLimits.asObservable();

	#allowedBlockTypes = new UmbArrayState<UmbBlockGridTypeModel>([], (x) => x.contentElementTypeKey);
	public readonly allowedBlockTypes = this.#allowedBlockTypes.asObservable();
	public readonly amountOfAllowedBlockTypes = this.#allowedBlockTypes.asObservablePart((x) => x.length);
	public readonly canCreate = this.#allowedBlockTypes.asObservablePart((x) => x.length > 0);

	#hasTypeLimits = new UmbBooleanState(undefined);
	public readonly hasTypeLimits = this.#hasTypeLimits.asObservable();

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

	setParentUnique(contentKey: string | null) {
		this.#parentUnique = contentKey;
	}

	getParentUnique(): string | null | undefined {
		return this.#parentUnique;
	}

	setAreaKey(areaKey: string | null) {
		this.#areaKey = areaKey;
		this.#pathAddendum.setAddendum(areaKey ?? '');
		this.#gotAreaKey();

		// Idea: If we need to parse down a validation data path to target the specific layout object: [NL]
		// If we have a areaKey, we want to inherit our layoutDataPath from nearest blockGridEntry context.
		// If not, we want to set the layoutDataPath to a base one.
	}

	getAreaKey(): string | null | undefined {
		return this.#areaKey;
	}

	setLayoutColumns(columns: number | undefined) {
		this.#layoutColumns.setValue(columns);
	}
	getLayoutColumns() {
		return this.#layoutColumns.getValue();
	}

	getMinAllowed() {
		if (this.#areaKey) {
			return this.#areaType.getValue()?.minAllowed ?? 0;
		}
		return this._manager?.getMinAllowed() ?? 0;
	}

	getMaxAllowed() {
		if (this.#areaKey) {
			return this.#areaType.getValue()?.maxAllowed ?? Infinity;
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

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addAdditionalPath('_catalogue/:view/:index')
			.onSetup(async (routingInfo) => {
				if (!this._manager) return false;
				// Idea: Maybe on setup should be async, so it can retrieve the values when needed? [NL]
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);
				const pasteTranslatorManifests = clipboardContext.getPasteTranslatorManifests(
					UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
				);

				// TODO: consider moving some of this logic to the clipboard property context
				const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
				const config = propertyContext.getConfig() as UmbBlockGridPropertyEditorConfig;
				const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver(this);

				return {
					data: {
						blocks: this.#allowedBlockTypes.getValue(),
						blockGroups: this._manager.getBlockGroups() ?? [],
						openClipboard: routingInfo.view === 'clipboard',
						clipboardFilter: async (clipboardEntryDetail) => {
							const hasSupportedPasteTranslator = clipboardContext.hasSupportedPasteTranslator(
								pasteTranslatorManifests,
								clipboardEntryDetail.values,
							);

							if (!hasSupportedPasteTranslator) {
								return false;
							}

							const pasteTranslator = await valueResolver.getPasteTranslator(
								clipboardEntryDetail.values,
								UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
							);

							if (pasteTranslator.isCompatibleValue) {
								const value = await valueResolver.resolve(
									clipboardEntryDetail.values,
									UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
								);

								return pasteTranslator.isCompatibleValue(value, config, (value) => this.#clipboardEntriesFilter(value));
							}

							return true;
						},
						originData: {
							index: index,
							areaKey: this.#areaKey,
							parentUnique: this.#parentUnique,
						} as UmbBlockGridWorkspaceOriginData,
						createBlockInWorkspace: this._manager.getInlineEditingMode() === false,
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
						await this.insert(
							created.layout,
							created.content,
							created.settings,
							data.originData as UmbBlockGridWorkspaceOriginData,
						);
					} else {
						throw new Error('Failed to create block');
					}
				} else if (value?.clipboard && value.clipboard.selection?.length && data) {
					const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);

					const propertyValues = await clipboardContext.readMultiple<UmbBlockGridValueModel>(
						value.clipboard.selection,
						UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
					);

					this._insertFromPropertyValues(propertyValues, data.originData as UmbBlockGridWorkspaceOriginData);
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				// TODO: Does it make any sense that this is a state? Check usage and confirm. [NL]
				this._catalogueRouteBuilderState.setValue(routeBuilder);
			});

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_GRID_WORKSPACE_MODAL)
			.addAdditionalPath('block')
			.onSetup(() => {
				return {
					data: {
						entityType: 'block',
						preset: {},
						originData: {
							index: -1,
							areaKey: this.#areaKey,
							parentUnique: this.#parentUnique,
							baseDataPath: this._dataPath,
						} as UmbBlockGridWorkspaceOriginData,
					},
					modal: { size: 'medium' },
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				const newPath = routeBuilder({});
				this._workspacePath.setValue(newPath);
			});
	}

	async #clipboardEntriesFilter(propertyValue: UmbBlockGridValueModel) {
		const allowedElementTypeKeys = this.#retrieveAllowedElementTypes().map((x) => x.contentElementTypeKey);

		const rootContentKeys = propertyValue.layout['Umbraco.BlockGrid']?.map((block) => block.contentKey) ?? [];
		const rootContentTypeKeys = propertyValue.contentData
			.filter((content) => rootContentKeys.includes(content.key))
			.map((content) => content.contentTypeKey);

		const allContentTypesAllowed = rootContentTypeKeys.every((contentKey) =>
			allowedElementTypeKeys.includes(contentKey),
		);

		return allContentTypesAllowed;
	}

	protected _gotBlockManager() {
		if (!this._manager) return;

		this.#setupAllowedBlockTypes();
		this.#setupRangeLimits();
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
					this.#areaType.setValue(areaType);
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
		this.#setupAllowedBlockTypesLimits();
	}
	#setupRangeLimits() {
		if (!this._manager) return;
		//const range = this.#retrieveRangeLimits();
		if (this.#areaKey != null) {
			const areaType = this.#areaType.getValue();
			this.removeUmbControllerByAlias('observeConfigurationRootLimits');
			// Area entries:
			if (!areaType) return undefined;
			// No need to observe as this method is called every time the area is changed.
			this.#rangeLimits.setValue({
				min: areaType.minAllowed ?? 0,
				max: areaType.maxAllowed ?? Infinity,
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

	isBlockTypeAllowed(contentTypeKey: string) {
		return this.#allowedBlockTypes.asObservablePart((types) =>
			types.some((x) => x.contentElementTypeKey === contentTypeKey),
		);
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
		partialLayoutEntry?: Omit<UmbBlockGridLayoutModel, 'contentKey'>,
		originData?: UmbBlockGridWorkspaceOriginData,
	) {
		await this._retrieveManager;
		return await this._manager?.createWithPresets(contentElementTypeKey, partialLayoutEntry, originData);
	}

	// insert Block?

	async insert(
		layoutEntry: UmbBlockGridLayoutModel,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockGridWorkspaceOriginData,
	) {
		await this._retrieveManager;
		return this._manager?.insert(layoutEntry, content, settings, originData) ?? false;
	}

	// create Block?
	override async delete(contentKey: string) {
		// TODO: Loop through children and delete them as well?
		// Find layout entry:
		const layout = this._layoutEntries.getValue().find((x) => x.contentKey === contentKey);
		if (!layout) {
			throw new Error(`Cannot delete block, missing layout for ${contentKey}`);
		}
		// The following loop will only delete the referenced data of sub Layout Entries, as the Layout entry is part of the main Layout Entry they will go away when that is removed. [NL]
		forEachBlockLayoutEntryOf(layout, async (entry) => {
			if (entry.settingsKey) {
				this._manager!.removeOneSettings(entry.settingsKey);
			}
			this._manager!.removeOneContent(contentKey);
			this._manager!.removeExposesOf(contentKey);
		});

		await super.delete(contentKey);
	}

	protected async _insertFromPropertyValue(value: UmbBlockGridValueModel, originData: UmbBlockGridWorkspaceOriginData) {
		const layoutEntries = value.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS];

		if (!layoutEntries) {
			throw new Error('No layout entries found');
		}

		await Promise.all(
			layoutEntries.map(async (layoutEntry) => {
				await this._insertBlockFromPropertyValue(layoutEntry, value, originData);
				if (originData.index !== -1) {
					originData = { ...originData, index: originData.index + 1 };
				}
			}),
		);

		return originData;
	}

	protected override async _insertBlockFromPropertyValue(
		layoutEntry: UmbBlockGridLayoutModel,
		value: UmbBlockGridValueModel,
		originData: UmbBlockGridWorkspaceOriginData,
	) {
		await super._insertBlockFromPropertyValue(layoutEntry, value, originData);

		// Handle inserting of the inner blocks..
		await forEachBlockLayoutEntryOf(layoutEntry, async (entry, parentUnique, areaKey) => {
			const localOriginData = { index: -1, parentUnique, areaKey };
			await this._insertBlockFromPropertyValue(entry, value, localOriginData);
		});
	}

	/**
	 * @internal
	 * @returns {Array<UmbBlockGridTypeModel>} an Array of ElementTypeKeys that are allowed in the current area. Or undefined if not ready jet.
	 */
	#retrieveAllowedElementTypes() {
		if (!this._manager) return [];

		if (this.#areaKey) {
			const areaType = this.#areaType.getValue();
			// Area entries:
			if (!areaType) return [];

			if (areaType.specifiedAllowance && areaType.specifiedAllowance?.length > 0) {
				return (
					areaType.specifiedAllowance
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
	 * @internal
	 */
	#setupAllowedBlockTypesLimits() {
		if (!this._manager) return;

		if (this.#areaKey) {
			const areaType = this.#areaType.getValue();
			// Area entries:
			if (!areaType) return;

			if (areaType.specifiedAllowance && areaType.specifiedAllowance?.length > 0) {
				this.#hasTypeLimits.setValue(true);
			}
		} else if (this.#areaKey === null) {
			// RESET
		}
	}

	// Property to hold the result of the check, used to make a meaningful Validation Message
	#invalidBlockTypeLimits?: Array<UmbBlockGridAreaTypeInvalidRuleType>;

	getInvalidBlockTypeLimits() {
		return this.#invalidBlockTypeLimits ?? [];
	}
	/**
	 * @internal
	 * @returns {boolean} - True if the block type limits are valid, otherwise false.
	 */
	checkBlockTypeLimitsValidity(): boolean {
		const areaType = this.#areaType.getValue();
		if (!areaType || !areaType.specifiedAllowance) return false;

		const layoutEntries = this._layoutEntries.getValue();

		this.#invalidBlockTypeLimits = areaType.specifiedAllowance
			.map((rule) => {
				const minAllowed = rule.minAllowed || 0;
				const maxAllowed = rule.maxAllowed || 0;

				// For block groups:
				if (rule.groupKey) {
					const groupElementTypeKeys =
						this._manager
							?.getBlockTypes()
							.filter((blockType) => blockType.groupKey === rule.groupKey && blockType.allowInAreas === true)
							.map((x) => x.contentElementTypeKey) ?? [];
					const groupAmount = layoutEntries.filter((entry) => {
						const contentTypeKey = this._manager!.getContentTypeKeyOfContentKey(entry.contentKey);
						return contentTypeKey ? groupElementTypeKeys.indexOf(contentTypeKey) !== -1 : false;
					}).length;

					if (groupAmount < minAllowed || (maxAllowed > 0 && groupAmount > maxAllowed)) {
						return {
							groupKey: rule.groupKey,
							name: this._manager!.getBlockGroupName(rule.groupKey) ?? '?',
							amount: groupAmount,
							minRequirement: minAllowed,
							maxRequirement: maxAllowed,
						};
					}
					return undefined;
				}
				// For specific elementTypes:
				else if (rule.elementTypeKey) {
					const amount = layoutEntries.filter((entry) => {
						const contentTypeKey = this._manager!.getContentOf(entry.contentKey)?.contentTypeKey;
						return contentTypeKey === rule.elementTypeKey;
					}).length;
					if (amount < minAllowed || (maxAllowed > 0 ? amount > maxAllowed : false)) {
						return {
							key: rule.elementTypeKey,
							name: this._manager!.getContentTypeNameOf(rule.elementTypeKey) ?? '?',
							amount: amount,
							minRequirement: minAllowed,
							maxRequirement: maxAllowed,
						};
					}
					return undefined;
				}

				// Lets fail cause the rule was bad.
				console.error('Invalid block type limit rule.', rule);
				return undefined;
			})
			.filter((x) => x !== undefined) as Array<UmbBlockGridAreaTypeInvalidRuleType>;
		return this.#invalidBlockTypeLimits.length === 0;
	}

	#invalidBlockTypeConfigurations?: Array<string>;

	getInvalidBlockTypeConfigurations() {
		return this.#invalidBlockTypeConfigurations ?? [];
	}
	/**
	 * @internal
	 * @returns {boolean} - True if the block type limits are valid, otherwise false.
	 */
	checkBlockTypeConfigurationValidity(): boolean {
		this.#invalidBlockTypeConfigurations = [];

		const layoutEntries = this._layoutEntries.getValue();
		if (layoutEntries.length === 0) return true;

		// Check all layout entries if they are allowed.
		const allowedBlocks = this.#allowedBlockTypes.getValue();
		if (allowedBlocks.length === 0) return false;

		const allowedKeys = allowedBlocks.map((x) => x.contentElementTypeKey);
		// get content for each layout entry:
		const invalidEntries = layoutEntries.filter((entry) => {
			const contentTypeKey = this._manager!.getContentTypeKeyOfContentKey(entry.contentKey);
			if (!contentTypeKey) {
				// We could not find the content type key, so we cant determin if this is valid or not when the content is missing.
				// This should be captured elsewhere as the Block then becomes invalid. So the unsupported Block should capture this.
				return false;
			}
			const isBad = allowedKeys.indexOf(contentTypeKey) === -1;
			if (contentTypeKey && isBad) {
				// if bad, then add the ContentTypeName to the list of invalids (if we could not find the name add the key)
				this.#invalidBlockTypeConfigurations?.push(
					this._manager?.getContentTypeNameOf(contentTypeKey) ?? contentTypeKey,
				);
			}
			return isBad;
		});

		return invalidEntries.length === 0;
	}

	/**
	 * Check if given contentKey is allowed in the current area.
	 * @param {string} contentKey - The contentKey of the content to check.
	 * @returns {boolean} - True if the content is allowed in the current area, otherwise false.
	 */
	allowDrop(contentKey: string) {
		const content = this._manager?.getContentOf(contentKey);
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
