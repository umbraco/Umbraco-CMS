import type { BlockValue } from './visual-editor-block-helper.js';
import type {
	UmbBlockDataModel,
	UmbBlockLayoutBaseModel,
	UmbBlockExposeModel,
	UmbBlockWorkspaceOriginData,
} from '@umbraco-cms/backoffice/block';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_LIST_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/block-list';
import { UmbBlockListManagerContext } from '@umbraco-cms/backoffice/block-list';
import { UMB_BLOCK_GRID_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/block-grid';
import { UmbBlockGridManagerContext } from '@umbraco-cms/backoffice/block-grid';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/block-grid';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbStringState,
	observeMultiple,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { debounceTime, firstValueFrom, filter } from '@umbraco-cms/backoffice/external/rxjs';

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

/**
 * Thin factory wrapper that creates the appropriate block manager (list or grid)
 * and entries context for the visual editor. Replaces the previous custom
 * VisualEditorBlockManager/VisualEditorBlockEntries subclasses.
 */
export class VisualEditorBlockManager extends UmbControllerBase {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	readonly #manager: UmbBlockManagerContext<any, any, any>;
	readonly #propertyAlias: string;
	readonly #editorSchemaAlias: string;
	readonly #onValueChanged: BlockBridgeValueChangedCallback;

	#workspacePath = new UmbStringState(undefined);
	#initialized = false;

	constructor(config: BlockBridgeConfig) {
		super(config.host, 'visualEditorBlockManager_' + config.propertyAlias);

		this.#propertyAlias = config.propertyAlias;
		this.#editorSchemaAlias = config.editorSchemaAlias;
		this.#onValueChanged = config.onValueChanged;

		const isGrid = config.editorSchemaAlias === UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS;

		// Create the appropriate manager based on the editor schema alias
		if (isGrid) {
			this.#manager = new UmbBlockGridManagerContext(config.host);
		} else {
			this.#manager = new UmbBlockListManagerContext(config.host);
		}

		this.#manager.setPropertyAlias(config.propertyAlias);
		this.#manager.setBlockTypes(config.blockTypes);
		this.#manager.setEditorConfiguration(new UmbPropertyEditorConfigCollection(config.config));
		this.#manager.setVariantId(config.variantId ?? UmbVariantId.CreateInvariant());

		// Register workspace modal route
		const workspaceModal = isGrid ? UMB_BLOCK_GRID_WORKSPACE_MODAL : UMB_BLOCK_LIST_WORKSPACE_MODAL;
		new UmbModalRouteRegistrationController(this, workspaceModal)
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

		// Observe manager state and sync back on changes
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
				if (!this.#initialized) return;

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
		super.destroy();
	}
}
