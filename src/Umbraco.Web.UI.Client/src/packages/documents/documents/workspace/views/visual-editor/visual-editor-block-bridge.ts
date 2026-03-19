import type { BlockValue } from './visual-editor-block-helper.js';
import {
	UmbBlockManagerContext,
	UMB_BLOCK_MANAGER_CONTEXT,
	UmbBlockEntriesContext,
	UMB_BLOCK_ENTRIES_CONTEXT,
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
import { UmbBooleanState, UmbStringState, observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { debounceTime } from '@umbraco-cms/backoffice/external/rxjs';

// ---------------------------------------------------------------------------
// Concrete block manager for the visual editor.
// Extends the abstract base so it can be provided under UMB_BLOCK_MANAGER_CONTEXT.
// ---------------------------------------------------------------------------
class VisualEditorBlockManager extends UmbBlockManagerContext<
	UmbBlockTypeBaseModel,
	UmbBlockLayoutBaseModel,
	UmbBlockWorkspaceOriginData
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
		_originData?: UmbBlockWorkspaceOriginData,
	) {
		return await this._createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	insert(
		layoutEntry: UmbBlockLayoutBaseModel,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockWorkspaceOriginData,
	) {
		const index = (originData as { index?: number }).index ?? -1;
		this._layouts.appendOneAt(layoutEntry, index);
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
	UmbBlockWorkspaceOriginData
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

	override getPathForCreateBlock(_index: number): string | undefined {
		return undefined; // Visual editor manages catalogue routing itself
	}

	override getPathForClipboard(_index: number): string | undefined {
		return undefined;
	}

	override async create(
		contentElementTypeKey: string,
		_layoutEntry?: Omit<UmbBlockLayoutBaseModel, 'contentKey'>,
		originData?: UmbBlockWorkspaceOriginData,
	): Promise<UmbBlockDataObjectModel<UmbBlockLayoutBaseModel> | undefined> {
		await this._retrieveManager;
		return await this._manager?.createWithPresets(contentElementTypeKey, _layoutEntry, originData);
	}

	override async insert(
		layoutEntry: UmbBlockLayoutBaseModel,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockWorkspaceOriginData,
	): Promise<boolean> {
		await this._retrieveManager;
		return (
			(this._manager as VisualEditorBlockManager | undefined)?.insert(layoutEntry, content, settings, originData) ??
			false
		);
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected override async _insertFromPropertyValue(_value: any, originData: UmbBlockWorkspaceOriginData) {
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

		// 2. Create entries — consumes UMB_BLOCK_MANAGER_CONTEXT from host
		this.#entries = new VisualEditorBlockEntries(config.host);

		// 3. Register workspace modal route
		new UmbModalRouteRegistrationController(this, UMB_BLOCK_LIST_WORKSPACE_MODAL)
			.addAdditionalPath('veBlock')
			.onSetup(() => {
				return {
					data: { entityType: 'block', preset: {}, baseDataPath: undefined as unknown as string },
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
	 * Open the block workspace to edit an existing block.
	 * @returns true if navigation was initiated.
	 */
	openEdit(blockKey: string): boolean {
		const path = this.#workspacePath.getValue();
		if (!path) {
			console.warn('[VisualEditorBlockBridge] workspace path not ready');
			return false;
		}

		// Navigate to the workspace edit route
		const editPath = `${path}edit/${encodeURIComponent(blockKey)}`;
		history.pushState({}, '', editPath);
		return true;
	}

	/**
	 * Open the block workspace to create a new block.
	 * @returns true if navigation was initiated.
	 */
	openCreate(contentElementTypeKey: string): boolean {
		const path = this.#workspacePath.getValue();
		if (!path) {
			console.warn('[VisualEditorBlockBridge] workspace path not ready');
			return false;
		}

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
