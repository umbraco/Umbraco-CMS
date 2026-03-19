import type { BlockValue } from './visual-editor-block-helper.js';
import {
	UmbBlockManagerContext,
	UMB_BLOCK_MANAGER_CONTEXT,
	UmbBlockEntriesContext,
} from '@umbraco-cms/backoffice/block';
import type {
	UmbBlockDataModel,
	UmbBlockLayoutBaseModel,
	UmbBlockExposeModel,
	UmbBlockWorkspaceOriginData,
	UmbBlockDataObjectModel,
} from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_LIST_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/block-list';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	appendToFrozenArray,
	pushAtToUniqueArray,
	UmbBooleanState,
	UmbStringState,
	observeMultiple,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { debounceTime, firstValueFrom, filter } from '@umbraco-cms/backoffice/external/rxjs';

// ---------------------------------------------------------------------------
// Grid layout types (mirrors the block-grid types but kept local to avoid
// importing from an internal package path)
// ---------------------------------------------------------------------------
interface GridLayoutAreaItem {
	key: string;
	items: Array<GridLayoutModel>;
}

interface GridLayoutModel extends UmbBlockLayoutBaseModel {
	columnSpan: number;
	rowSpan: number;
	areas?: Array<GridLayoutAreaItem>;
}

/** Origin data that includes grid area targeting. */
export interface VisualEditorBlockOriginData extends UmbBlockWorkspaceOriginData {
	index: number;
	parentUnique?: string | null;
	areaKey?: string | null;
}

// ---------------------------------------------------------------------------
// Concrete block manager for the visual editor.
//
// Extends the abstract base so it can be provided under UMB_BLOCK_MANAGER_CONTEXT.
// Supports both block list (flat layout) and block grid (nested area layout).
// ---------------------------------------------------------------------------
class VisualEditorBlockManager extends UmbBlockManagerContext<
	UmbBlockTypeBaseModel,
	UmbBlockLayoutBaseModel,
	VisualEditorBlockOriginData
> {
	#inlineEditingMode = new UmbBooleanState(false);
	readonly inlineEditingMode = this.#inlineEditingMode.asObservable();

	setInlineEditingMode(mode: boolean) {
		this.#inlineEditingMode.setValue(mode);
	}
	getInlineEditingMode() {
		return this.#inlineEditingMode.getValue();
	}

	override async createWithPresets(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<UmbBlockLayoutBaseModel, 'contentKey'>,
		_originData?: VisualEditorBlockOriginData,
	) {
		return await this._createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	// ---- Grid-aware setOneLayout -------------------------------------------
	// If originData carries parentUnique + areaKey, insert into that area.
	// Otherwise fall through to root-level insertion.
	// ------------------------------------------------------------------------
	override setOneLayout(layoutEntry: UmbBlockLayoutBaseModel, originData?: VisualEditorBlockOriginData) {
		const index = originData?.index ?? -1;

		if (originData?.parentUnique && originData?.areaKey) {
			const updated = this.#appendLayoutEntryToArea(
				layoutEntry as GridLayoutModel,
				this._layouts.getValue() as Array<GridLayoutModel>,
				originData.parentUnique,
				originData.areaKey,
				index,
			);
			if (updated) {
				this._layouts.setValue(updated as Array<UmbBlockLayoutBaseModel>);
				return;
			}
			// Parent not found — fall through to root
			console.warn('[VisualEditorBlockManager] parent block not found for area insert, inserting at root');
		}

		// For blocks that may be nested inside grid areas, try updating in-place
		// before falling through to root-level append (which would duplicate).
		const current = this._layouts.getValue() as Array<GridLayoutModel>;
		const updatedInPlace = this.#updateLayoutEntryInPlace(
			layoutEntry as GridLayoutModel,
			current,
		);
		if (updatedInPlace) {
			this._layouts.setValue(updatedInPlace as Array<UmbBlockLayoutBaseModel>);
			return;
		}

		this._layouts.appendOneAt(layoutEntry, index);
	}

	/**
	 * Recursively find an existing layout entry by contentKey and replace it
	 * in-place, preserving its position within any nested area structure.
	 */
	#updateLayoutEntryInPlace(
		entry: GridLayoutModel,
		entries: Array<GridLayoutModel>,
	): Array<GridLayoutModel> | undefined {
		for (let i = 0; i < entries.length; i++) {
			const current = entries[i];
			if (current.contentKey === entry.contentKey) {
				// Found at this level — replace via appendToFrozenArray
				return appendToFrozenArray(entries, entry, (x) => x.contentKey === entry.contentKey);
			}
			if (current.areas) {
				for (let y = 0; y < current.areas.length; y++) {
					const updatedItems = this.#updateLayoutEntryInPlace(entry, current.areas[y].items);
					if (updatedItems) {
						const area = current.areas[y];
						return appendToFrozenArray(
							entries,
							{
								...current,
								areas: appendToFrozenArray(
									current.areas,
									{ ...area, items: updatedItems },
									(z) => z.key === area.key,
								),
							},
							(x) => x.contentKey === current.contentKey,
						);
					}
				}
			}
		}
		return undefined;
	}

	/**
	 * Recursively walk layout entries to find the parent block, then insert
	 * the new entry into the matching area's items array.
	 *
	 * Mirrors UmbBlockGridManagerContext.#appendLayoutEntryToArea.
	 */
	#appendLayoutEntryToArea(
		insert: GridLayoutModel,
		entries: Array<GridLayoutModel>,
		parentId: string,
		areaKey: string,
		index: number,
	): Array<GridLayoutModel> | undefined {
		let i: number = entries.length;
		while (i--) {
			const currentEntry = entries[i];
			if (currentEntry.contentKey === parentId) {
				const areas =
					currentEntry.areas?.map((x) =>
						x.key === areaKey
							? {
									...x,
									items: pushAtToUniqueArray(
										[...x.items],
										insert,
										(x) => x.contentKey === insert.contentKey,
										index,
									),
								}
							: x,
					) ?? [];
				return appendToFrozenArray(
					entries,
					{ ...currentEntry, areas },
					(x) => x.contentKey === currentEntry.contentKey,
				);
			}
			if (currentEntry.areas) {
				let y: number = currentEntry.areas.length;
				while (y--) {
					const correctedAreaItems = this.#appendLayoutEntryToArea(
						insert,
						currentEntry.areas[y].items,
						parentId,
						areaKey,
						index,
					);
					if (correctedAreaItems) {
						const area = currentEntry.areas[y];
						return appendToFrozenArray(
							entries,
							{
								...currentEntry,
								areas: appendToFrozenArray(
									currentEntry.areas,
									{ ...area, items: correctedAreaItems },
									(z) => z.key === area.key,
								),
							},
							(x) => x.contentKey === currentEntry.contentKey,
						);
					}
				}
			}
		}
		return undefined;
	}

	insert(
		layoutEntry: UmbBlockLayoutBaseModel,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: VisualEditorBlockOriginData,
	) {
		this.setOneLayout(layoutEntry, originData);
		this.insertBlockData(layoutEntry, content, settings, originData);
		this.notifyBlockInserted(layoutEntry, originData);
		return true;
	}
}

// ---------------------------------------------------------------------------
// Concrete block entries context for the visual editor.
// Provides the minimum surface the block workspace needs:
//   - layoutOf(key): observable of a single layout entry
//   - create(typeKey, ...): creates a new block
//   - insert(layout, content, settings, origin): inserts into manager
// ---------------------------------------------------------------------------
class VisualEditorBlockEntries extends UmbBlockEntriesContext<
	typeof UMB_BLOCK_MANAGER_CONTEXT,
	typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE,
	UmbBlockTypeBaseModel,
	UmbBlockLayoutBaseModel,
	VisualEditorBlockOriginData
> {
	public readonly canCreate = new UmbBooleanState(true).asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_MANAGER_CONTEXT);
	}

	protected override _gotBlockManager(): void {
		if (!this._manager) return;

		// Two-way sync between entries and manager layouts
		this.observe(
			this._manager.layouts,
			(layouts: Array<UmbBlockLayoutBaseModel>) => {
				this._layoutEntries.setValue(layouts);
			},
			'observeParentLayouts',
		);
		this.observe(
			this.layoutEntries,
			(layouts: Array<UmbBlockLayoutBaseModel>) => {
				this._manager?.setLayouts(layouts);
			},
			'observeThisLayouts',
		);
	}

	/**
	 * Override layoutOf to recursively search through block grid areas.
	 * The base implementation only searches the top-level array, which misses
	 * blocks nested inside area items.
	 */
	override layoutOf(contentKey: string) {
		return this._layoutEntries.asObservablePart((source) => this.#findLayoutRecursive(source, contentKey));
	}
	override getLayoutOf(contentKey: string) {
		return this.#findLayoutRecursive(this._layoutEntries.getValue(), contentKey);
	}

	#findLayoutRecursive(
		entries: Array<UmbBlockLayoutBaseModel>,
		contentKey: string,
	): UmbBlockLayoutBaseModel | undefined {
		for (const entry of entries) {
			if (entry.contentKey === contentKey) return entry;
			const gridEntry = entry as GridLayoutModel;
			if (gridEntry.areas) {
				for (const area of gridEntry.areas) {
					const found = this.#findLayoutRecursive(area.items as Array<UmbBlockLayoutBaseModel>, contentKey);
					if (found) return found;
				}
			}
		}
		return undefined;
	}

	override getPathForCreateBlock(_index: number): string | undefined {
		return undefined; // Visual editor manages catalogue routing itself
	}

	override getPathForClipboard(_index: number): string | undefined {
		return undefined;
	}

	override async create(
		contentElementTypeKey: string,
		_layoutEntry?: Omit<UmbBlockLayoutBaseModel, 'contentKey'>,
		originData?: VisualEditorBlockOriginData,
	): Promise<UmbBlockDataObjectModel<UmbBlockLayoutBaseModel> | undefined> {
		await this._retrieveManager;
		return await this._manager?.createWithPresets(contentElementTypeKey, _layoutEntry, originData);
	}

	override async insert(
		layoutEntry: UmbBlockLayoutBaseModel,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: VisualEditorBlockOriginData,
	): Promise<boolean> {
		await this._retrieveManager;
		return (
			(this._manager as VisualEditorBlockManager | undefined)?.insert(layoutEntry, content, settings, originData) ??
			false
		);
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected override async _insertFromPropertyValue(_value: any, originData: VisualEditorBlockOriginData) {
		// Not used in the visual editor flow
		return originData;
	}
}

// ---------------------------------------------------------------------------
// Visual Editor Block Bridge
//
// Creates a per-property block manager + entries context on a host element,
// registers the workspace modal route, and syncs manager state back to the
// visual editor via a callback.
//
// Supports both block list (flat) and block grid (nested areas).
// Accepts an optional variant ID for multilingual content.
// ---------------------------------------------------------------------------

export type BlockBridgeValueChangedCallback = (propertyAlias: string, value: BlockValue) => void;

export interface BlockBridgeConfig {
	/** Host element — contexts are provided here so the workspace modal can find them. */
	host: UmbControllerHost;
	/** Document property alias this bridge manages. */
	propertyAlias: string;
	/** Layout schema alias (e.g. "Umbraco.BlockList" or "Umbraco.BlockGrid"). */
	editorSchemaAlias: string;
	/** Block type configurations from the property editor config. */
	blockTypes: Array<UmbBlockTypeBaseModel>;
	/** Full property editor config (passed to the manager). */
	config: UmbPropertyEditorConfig;
	/** Called when the manager state changes after editing. */
	onValueChanged: BlockBridgeValueChangedCallback;
	/** Optional variant ID for culture/segment-aware blocks. */
	variantId?: UmbVariantId;
}

export class VisualEditorBlockBridge extends UmbControllerBase {
	readonly #manager: VisualEditorBlockManager;
	readonly #entries: VisualEditorBlockEntries;
	readonly #propertyAlias: string;
	readonly #editorSchemaAlias: string;
	readonly #onValueChanged: BlockBridgeValueChangedCallback;

	#workspacePath = new UmbStringState(undefined);
	#initialized = false;

	constructor(config: BlockBridgeConfig) {
		super(config.host, 'visualEditorBlockBridge_' + config.propertyAlias);

		this.#propertyAlias = config.propertyAlias;
		this.#editorSchemaAlias = config.editorSchemaAlias;
		this.#onValueChanged = config.onValueChanged;

		// 1. Create manager — auto-provides UMB_BLOCK_MANAGER_CONTEXT on host
		this.#manager = new VisualEditorBlockManager(config.host);
		this.#manager.setPropertyAlias(config.propertyAlias);
		this.#manager.setBlockTypes(config.blockTypes);
		this.#manager.setEditorConfiguration(new UmbPropertyEditorConfigCollection(config.config));

		// 1b. Set variant ID — the workspace context requires a non-undefined variantId
		// from the manager to initialize. Default to invariant if none is provided.
		this.#manager.setVariantId(config.variantId ?? UmbVariantId.CreateInvariant());

		// 2. Create entries — consumes UMB_BLOCK_MANAGER_CONTEXT from host
		this.#entries = new VisualEditorBlockEntries(config.host);

		// 3. Register workspace modal route
		new UmbModalRouteRegistrationController(this, UMB_BLOCK_LIST_WORKSPACE_MODAL)
			.addAdditionalPath('veBlock')
			.onSetup(() => {
				return {
					data: {
						entityType: 'block',
						preset: {},
						baseDataPath: undefined as unknown as string,
						originData: { index: -1 },
					},
					modal: { size: 'medium' },
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				const path = routeBuilder({});
				this.#workspacePath.setValue(path);
			});

		// 4. Observe manager state and sync back on changes
		this.observe(
			(
				observeMultiple([
					this.#manager.layouts,
					this.#manager.contents,
					this.#manager.settings,
					this.#manager.exposes,
				]) as Observable<
					[
						Array<UmbBlockLayoutBaseModel>,
						Array<UmbBlockDataModel>,
						Array<UmbBlockDataModel>,
						Array<UmbBlockExposeModel>,
					]
				>
			).pipe(debounceTime(60)),
			([layouts, contents, settings, exposes]) => {
				if (!this.#initialized) return; // Skip the initial load

				const value = {
					layout: { [this.#editorSchemaAlias]: layouts },
					contentData: contents,
					settingsData: settings,
					expose: exposes,
				} as unknown as BlockValue;

				this.#onValueChanged(this.#propertyAlias, value);
			},
			'observeManagerState',
		);
	}

	/**
	 * Push the current property value into the manager.
	 * Call this before opening a workspace to ensure the manager has fresh data.
	 */
	loadValue(blockValue: BlockValue) {
		this.#initialized = false;

		const layouts = blockValue.layout[this.#editorSchemaAlias] ?? [];
		this.#manager.setLayouts(layouts as Array<UmbBlockLayoutBaseModel>);
		this.#manager.setContents(blockValue.contentData as Array<UmbBlockDataModel>);
		this.#manager.setSettings(blockValue.settingsData as Array<UmbBlockDataModel>);
		this.#manager.setExposes(blockValue.expose as Array<UmbBlockExposeModel>);

		// Allow the debounced observer to start forwarding changes
		// after a short delay so the initial load doesn't trigger a callback.
		setTimeout(() => {
			this.#initialized = true;
		}, 100);
	}

	/**
	 * Update the variant ID (e.g. when the user switches culture in the workspace).
	 */
	setVariantId(variantId: UmbVariantId | undefined) {
		this.#manager.setVariantId(variantId ?? UmbVariantId.CreateInvariant());
	}

	/**
	 * Wait for the workspace path to become available.
	 * The modal route registration is async (consumes UMB_ROUTE_CONTEXT),
	 * so the path may not be ready immediately after bridge construction.
	 */
	async #waitForWorkspacePath(): Promise<string> {
		const current = this.#workspacePath.getValue();
		if (current) return current;
		return firstValueFrom(
			(this.#workspacePath.asObservable() as Observable<string | undefined>).pipe(
				filter((p): p is string => !!p),
			),
		);
	}

	/**
	 * Open the block workspace to edit an existing block.
	 * @returns true if navigation was initiated.
	 */
	async openEdit(blockKey: string): Promise<boolean> {
		const path = await this.#waitForWorkspacePath();

		// Navigate to the workspace edit route — must include /view/content
		// so the workspace's internal router matches the content view.
		const editPath = `${path}edit/${encodeURIComponent(blockKey)}/view/content`;
		history.pushState({}, '', editPath);
		return true;
	}

	/**
	 * Open the block workspace to create a new block.
	 * @returns true if navigation was initiated.
	 */
	async openCreate(contentElementTypeKey: string): Promise<boolean> {
		const path = await this.#waitForWorkspacePath();

		const createPath = `${path}create/${encodeURIComponent(contentElementTypeKey)}`;
		history.pushState({}, '', createPath);
		return true;
	}

	/**
	 * Get the current block value from the manager (synchronous snapshot).
	 */
	getValue(): BlockValue {
		return {
			layout: { [this.#editorSchemaAlias]: this.#manager.getLayouts() },
			contentData: this.#manager.getContents(),
			settingsData: this.#manager.getSettings(),
			expose: this.#manager.getExposes(),
		} as unknown as BlockValue;
	}

	override destroy() {
		this.#manager.destroy();
		this.#entries.destroy();
		super.destroy();
	}
}
