import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../constants.js';
import {
	findBlockInValues,
	updateBlockPropertyValues,
	updateBlockSettingsValues,
	ensureBlockSettings,
	addBlockToValue,
	addBlockToArea,
	moveBlock,
	reorderBlockInValue,
	removeBlockFromValue,
	mergeBlockValueInto,
} from './visual-editor-block-helper.js';
import type { BlockValue } from './visual-editor-block-helper.js';
import { VisualEditorBlockBridge } from './visual-editor-block-bridge.js';
import { UMB_VISUAL_EDITOR_PROPERTY_MODAL } from './visual-editor-property-modal.token.js';
import type {
	UmbVisualEditorPropertyGroup,
	UmbVisualEditorPropertyInfo,
	UmbVisualEditorPropertyModalData,
	UmbVisualEditorPropertyModalValue,
} from './visual-editor-property-modal.token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_BLOCK_CATALOGUE_MODAL } from '@umbraco-cms/backoffice/block';
import type { UmbBlockCatalogueModalData, UmbBlockCatalogueModalValue } from '@umbraco-cms/backoffice/block';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import {
	UMB_CLIPBOARD_CONTEXT,
	UmbClipboardPastePropertyValueTranslatorValueResolver,
} from '@umbraco-cms/backoffice/clipboard';
import { UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/block-list';
import { UmbPreviewRepository } from '@umbraco-cms/backoffice/preview';
import { HubConnectionBuilder } from '@umbraco-cms/backoffice/external/signalr';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { DataTypeService, DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { HubConnection } from '@umbraco-cms/backoffice/external/signalr';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * Visual editor workspace view — renders as a full-screen overlay on top of the backoffice.
 * Lives inside the document workspace so all property editors have access to the
 * full context hierarchy (workspace, entity, variant, property dataset, etc.).
 * @element umb-document-workspace-view-visual-editor
 */
@customElement('umb-document-workspace-view-visual-editor')
export class UmbDocumentWorkspaceViewVisualEditorElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;
	#previewRepository = new UmbPreviewRepository(this);
	#connection?: HubConnection;
	#serverUrl = '';
	#suppressRefresh = false;

	// Track selection for restore after iframe refresh
	#selectedPropertyAlias?: string;
	#selectedBlockKey?: string;

	// Track which property/block is being edited so submit/reject handlers can reference it
	#editingPropertyAlias?: string;
	#editingBlockKey?: string;

	// Cache resolved block property structures by content type key to avoid repeated API calls
	#blockStructureCache = new Map<
		string,
		{ name: string; properties: UmbVisualEditorPropertyInfo[]; groups: UmbVisualEditorPropertyGroup[] }
	>();

	// --- Block bridge (per-property block manager + entries for workspace integration) ---
	#blockBridge?: VisualEditorBlockBridge;
	#blockBridgePropertyAlias?: string;
	#currentVariantId?: UmbVariantId;

	/**
	 * Ensure a block bridge exists for the given property.
	 * Creates a new bridge (or reinitializes the existing one) with the property's
	 * current block types, config, and value so the standard block workspace can be opened.
	 */
	#ensureBridge(propertyAlias: string, blockValue: BlockValue): VisualEditorBlockBridge {
		const propStructure = this.#propertyStructures.find((p) => p.alias === propertyAlias);
		const config = (propStructure?.config ?? []) as UmbPropertyEditorConfig;

		const blocksConfig =
			(config as Array<{ alias: string; value: unknown }>)?.find((c) => c.alias === 'blocks')?.value as
				| Array<{ contentElementTypeKey: string; label?: string; settingsElementTypeKey?: string }>
				| undefined;

		const blockTypes = (blocksConfig ?? []).map((b) => ({
			contentElementTypeKey: b.contentElementTypeKey,
			label: b.label,
			settingsElementTypeKey: b.settingsElementTypeKey,
			forceHideContentEditorInOverlay: false,
		}));

		// Determine the editor schema alias from the layout keys
		const layoutKey = Object.keys(blockValue.layout)[0] ?? UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS;

		if (this.#blockBridge && this.#blockBridgePropertyAlias === propertyAlias) {
			// Reuse existing bridge — just reload with fresh data
			this.#blockBridge.setVariantId(this.#currentVariantId);
			this.#blockBridge.loadValue(blockValue);
			return this.#blockBridge;
		}

		// Destroy previous bridge if switching properties
		this.#blockBridge?.destroy();

		this.#blockBridge = new VisualEditorBlockBridge({
			host: this,
			propertyAlias,
			editorSchemaAlias: layoutKey,
			blockTypes,
			config,
			variantId: this.#currentVariantId,
			onValueChanged: async (alias, value) => {
				await this.#setPropertyValue(alias, value);
				await this.#saveAndRefresh();
			},
		});
		this.#blockBridgePropertyAlias = propertyAlias;
		this.#blockBridge.loadValue(blockValue);

		return this.#blockBridge;
	}

	@state() private _iframeReady = false;
	@state() private _previewUrl?: string;
	@state() private _hasRegions = false;

	// --- Routed modal registrations ---

	#propertyModalRegistration = new UmbModalRouteRegistrationController<
		UmbVisualEditorPropertyModalData,
		UmbVisualEditorPropertyModalValue
	>(this, UMB_VISUAL_EDITOR_PROPERTY_MODAL, 'vePropertyModal')
		.addAdditionalPath('property/:propertyAlias')
		.onSetup((params) => {
			const alias = params.propertyAlias as string;
			this.#editingPropertyAlias = alias;

			const structure = this.#propertyStructures.find((p) => p.alias === alias);
			if (!structure?.editorUiAlias) {
				return false;
			}

			return {
				data: {
					headline: 'Edit property',
					properties: [
						{
							alias: structure.alias,
							name: structure.name,
							description: structure.description,
							editorUiAlias: structure.editorUiAlias,
							config: structure.config,
						},
					],
					values: [{ alias, value: this.#getPropertyValue(alias) ?? '' }],
				},
				value: { values: [] },
			};
		})
		.onSubmit((value) => {
			if (this.#editingPropertyAlias) {
				this.#handlePropertySubmit(this.#editingPropertyAlias, value);
			}
		})
		.onReject(() => {
			this.#editingPropertyAlias = undefined;
		});

	#blockModalRegistration = new UmbModalRouteRegistrationController<
		UmbVisualEditorPropertyModalData,
		UmbVisualEditorPropertyModalValue
	>(this, UMB_VISUAL_EDITOR_PROPERTY_MODAL, 'veBlockModal')
		.addAdditionalPath('block/:blockKey')
		.onSetup(async (params) => {
			const blockKey = params.blockKey as string;
			this.#editingBlockKey = blockKey;
			const allValues = this.#getAllValues();
			const found = findBlockInValues(allValues, blockKey);
			if (!found) {
				return false;
			}

			const {
				name: blockName,
				properties: blockProperties,
				groups: blockGroups,
			} = await this.#resolveBlockPropertyStructures(found.block.contentTypeKey);


			const blockValues = (found.block.values || []).map((v: { alias: string; value: unknown }) => ({
				alias: v.alias,
				value: v.value,
			}));

			// Resolve settings if the block type has a settings element type
			const blockTypeConfig = this.#findBlockTypeConfig(found.propertyAlias, found.block.contentTypeKey);
			let settingsProperties: UmbVisualEditorPropertyInfo[] | undefined;
			let settingsGroups: UmbVisualEditorPropertyGroup[] | undefined;
			let settingsValues: Array<{ alias: string; value: unknown }> | undefined;

			if (blockTypeConfig?.settingsElementTypeKey) {
				const settingsResult = await this.#resolveBlockPropertyStructures(blockTypeConfig.settingsElementTypeKey);
				if (settingsResult.properties.length > 0) {
					let blockValue = found.blockValue;
					let settingsKey: string | undefined;
					if (!found.layoutEntry?.settingsKey) {
						const ensured = ensureBlockSettings(blockValue, blockKey, blockTypeConfig.settingsElementTypeKey);
						blockValue = ensured.updatedValue;
						settingsKey = ensured.settingsKey;
						await this.#setPropertyValue(found.propertyAlias, blockValue);
					} else {
						settingsKey = found.layoutEntry.settingsKey as string;
					}

					settingsProperties = settingsResult.properties as UmbVisualEditorPropertyInfo[];
					settingsGroups = settingsResult.groups;

					const settingsData = blockValue.settingsData.find((s) => s.key === settingsKey);
					settingsValues = (settingsData?.values || []).map((v) => ({
						alias: v.alias,
						value: v.value,
					}));
				}
			}

			// Skip if nothing to edit (no content properties and no settings)
			if (blockProperties.length === 0 && !settingsProperties?.length) {
				return false;
			}

			const result = {
				modal: { size: 'medium' as const },
				data: {
					headline: `Edit ${blockName}`,
					properties: blockProperties as UmbVisualEditorPropertyInfo[],
					groups: blockGroups,
					values: blockValues,
					settingsProperties,
					settingsGroups,
					settingsValues,
				},
				value: { values: [] },
			};
			return result;
		})
		.onSubmit((value) => {
			if (this.#editingBlockKey) {
				this.#handleBlockSubmit(this.#editingBlockKey, value);
			}
		})
		.onReject(() => {
			this.#editingBlockKey = undefined;
		});

	// --- Pending add context (set before opening catalogue, read in onSubmit) ---
	#pendingAdd?: {
		propertyAlias: string;
		propertyValue: import('./visual-editor-block-helper.js').BlockValue;
		insertIndex: number;
		parentBlockKey?: string;
		areaAlias?: string;
		areaConfigs?: Array<{ key: string; alias: string }>;
	};

	#catalogueModalRegistration = new UmbModalRouteRegistrationController<
		UmbBlockCatalogueModalData,
		UmbBlockCatalogueModalValue
	>(this, UMB_BLOCK_CATALOGUE_MODAL, 'veCatalogue')
		.addAdditionalPath('_catalogue/:insertIndex')
		.onSetup(async (params) => {
			if (!this.#pendingAdd) {
				return false;
			}

			const propStructure = this.#propertyStructures.find((p) => p.alias === this.#pendingAdd!.propertyAlias);

			if (!propStructure?.config) return false;

			const blocksConfig = (propStructure.config as Array<{ alias: string; value: unknown }>)?.find(
				(c) => c.alias === 'blocks',
			)?.value as
				| Array<{ contentElementTypeKey: string; label?: string; forceHideContentEditorInOverlay?: boolean }>
				| undefined;

			if (!blocksConfig || blocksConfig.length === 0) return false;

			const blockTypes = blocksConfig.map((b) => ({
				contentElementTypeKey: b.contentElementTypeKey,
				label: b.label,
				forceHideContentEditorInOverlay: b.forceHideContentEditorInOverlay ?? false,
			}));

			// Resolve which block types have editable properties so the catalogue can show workspace links
			const contentTypeHasProperties: Record<string, boolean> = {};
			for (const b of blockTypes) {
				contentTypeHasProperties[b.contentElementTypeKey] = await this.#blockHasEditableFields(
					b.contentElementTypeKey,
					this.#pendingAdd!.propertyAlias,
				);
			}

			const modalSize = blockTypes.length > 12 ? 'large' : blockTypes.length > 8 ? 'medium' : 'small';

			// Build clipboard filter using paste translator resolver
			const editorUiAlias = propStructure.editorUiAlias;
			const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver(this);
			const clipboardFilter = editorUiAlias
				? async (clipboardEntryDetail: { values: Array<{ type: string; value: unknown }> }) => {
						try {
							const pasteTranslator = await valueResolver.getPasteTranslator(
								clipboardEntryDetail.values,
								editorUiAlias,
							);
							if (pasteTranslator.isCompatibleValue) {
								const resolved = await valueResolver.resolve(
									clipboardEntryDetail.values,
									editorUiAlias,
								);
								return pasteTranslator.isCompatibleValue(resolved, propStructure.config);
							}
							return true;
						} catch {
							return false;
						}
					}
				: undefined;

			return {
				modal: { size: modalSize },
				data: {
					blocks: blockTypes,
					blockGroups: [],
					originData: { index: parseInt(params.insertIndex as string) || 0 },
					contentTypeHasProperties,
					clipboardFilter,
				},
				value: undefined,
			};
		})
		.onSubmit(async (value) => {
			if (!this.#pendingAdd) return;

			const { propertyAlias, propertyValue, insertIndex, parentBlockKey, areaAlias, areaConfigs } = this.#pendingAdd;
			this.#pendingAdd = undefined;

			if (value?.clipboard?.selection?.length) {
				await this.#handleClipboardPaste(propertyAlias, propertyValue, insertIndex, value.clipboard.selection);
				return;
			}

			if (!value?.create?.contentElementTypeKey) return;
			const contentTypeKey = value.create.contentElementTypeKey;

			if (parentBlockKey && areaAlias && areaConfigs) {
				// Add to area
				const { updatedValue, contentKey } = addBlockToArea(
					propertyValue,
					parentBlockKey,
					areaAlias,
					contentTypeKey,
					insertIndex,
					areaConfigs,
				);
				await this.#setPropertyValue(propertyAlias, updatedValue);

				const hasProperties = await this.#blockHasEditableFields(contentTypeKey, propertyAlias);
				if (hasProperties) {
					this.#onBlockClicked(contentKey, contentTypeKey);
				} else {
					await this.#saveAndRefresh();
				}
			} else {
				// Add to root
				const selectedBlockConfig = this.#findBlockTypeConfig(propertyAlias, contentTypeKey);
				const { updatedValue, contentKey } = addBlockToValue(
					propertyValue,
					contentTypeKey,
					insertIndex,
					undefined,
					selectedBlockConfig?.areas,
					selectedBlockConfig?.settingsElementTypeKey,
				);
				await this.#setPropertyValue(propertyAlias, updatedValue);

				const hasProperties = await this.#blockHasEditableFields(contentTypeKey, propertyAlias);
				if (hasProperties) {
					this.#onBlockClicked(contentKey, contentTypeKey);
				} else {
					await this.#saveAndRefresh();
				}
			}
		})
		.onReject(() => {
			this.#pendingAdd = undefined;
		});

	// --- Property structures cache (resolved from content type) ---
	#propertyStructures: UmbVisualEditorPropertyInfo[] = [];

	constructor() {
		super();

		// Register the visual editor property modal (idempotent)
		if (!umbExtensionsRegistry.isRegistered('Umb.Modal.VisualEditorProperty')) {
			umbExtensionsRegistry.register({
				type: 'modal',
				alias: 'Umb.Modal.VisualEditorProperty',
				name: 'Visual Editor Property Modal',
				element: () => import('./visual-editor-property-modal.element.js'),
			});
		}

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#initialize();
		});

		this.consumeContext(UMB_SERVER_CONTEXT, (serverContext) => {
			this.#serverUrl = serverContext?.getServerUrl() ?? '';
		});

		// Observe the variant context so the bridge can set the correct culture/segment
		this.consumeContext(UMB_VARIANT_CONTEXT, (variantContext) => {
			this.observe(
				variantContext?.displayVariantId,
				(variantId) => {
					this.#currentVariantId = variantId;
					// Keep the active bridge in sync
					this.#blockBridge?.setVariantId(variantId);
				},
				'observeVariantId',
			);
		});
	}

	async #initialize() {
		if (!this.#workspaceContext) return;

		const unique = this.#workspaceContext.getUnique();
		if (!unique) return;

		// Enter preview mode (sets cookie)
		try {
			await this.#previewRepository.getPreviewUrl(unique, 'umbDocumentUrlProvider');
		} catch (e) {
			console.error('[VisualEditor] Failed to enter preview mode', e);
		}

		// Build preview URL
		this.#setPreviewUrl();

		// Resolve property structures from content type
		await this.#fetchPropertyStructures();

		// Connect SignalR for live refresh
		this.#initSignalR();
	}

	// --- Property value access (delegates to workspace context) ---

	#getPropertyValue(alias: string): unknown {
		return this.#workspaceContext?.getPropertyValue(alias);
	}

	async #setPropertyValue(alias: string, value: unknown) {
		await this.#workspaceContext?.setPropertyValue(alias, value);
	}

	#getAllValues(): Array<{ alias: string; value: unknown }> {
		const values = this.#workspaceContext?.getValues();
		if (!values) return [];
		return values.map((v) => ({ alias: v.alias, value: v.value }));
	}

	// --- Property structures ---

	async #fetchPropertyStructures() {
		if (!this.#workspaceContext) return;

		const contentTypeUnique = this.#workspaceContext.getContentTypeUnique();
		if (!contentTypeUnique) return;

		// Recursively resolve the full composition chain
		const fetchedIds = new Set<string>();
		const toFetch = [contentTypeUnique];
		const allProperties: Array<{
			alias: string;
			name: string;
			description?: string | null;
			dataType: { id: string };
			validation: any;
		}> = [];

		while (toFetch.length > 0) {
			const batch = [...toFetch];
			toFetch.length = 0;

			for (const id of batch) {
				if (fetchedIds.has(id)) continue;
				fetchedIds.add(id);

				const { data } = await tryExecute(this, DocumentTypeService.getDocumentTypeById({ path: { id } }));
				if (!data) continue;

				allProperties.push(...(data.properties ?? []));

				for (const comp of data.compositions ?? []) {
					if (!fetchedIds.has(comp.documentType.id)) {
						toFetch.push(comp.documentType.id);
					}
				}
			}
		}

		this.#propertyStructures = [];
		for (const prop of allProperties) {
			let editorUiAlias = '';
			let config: UmbPropertyEditorConfig | undefined;

			if (prop.dataType.id) {
				const { data: dtData } = await tryExecute(
					this,
					DataTypeService.getDataTypeById({ path: { id: prop.dataType.id } }),
				);
				if (dtData) {
					editorUiAlias = dtData.editorUiAlias ?? '';
					config = dtData.values as UmbPropertyEditorConfig;
				}
			}

			this.#propertyStructures.push({
				alias: prop.alias,
				name: prop.name ?? prop.alias,
				description: prop.description ?? undefined,
				editorUiAlias,
				config,
				validation: prop.validation,
			});
		}
	}

	async #resolveBlockPropertyStructures(
		contentTypeKey: string,
	): Promise<{ name: string; properties: UmbVisualEditorPropertyInfo[]; groups: UmbVisualEditorPropertyGroup[] }> {
		const cached = this.#blockStructureCache.get(contentTypeKey);
		if (cached) return cached;

		const { data } = await tryExecute(
			this,
			DocumentTypeService.getDocumentTypeById({ path: { id: contentTypeKey } }),
		);
		if (!data?.properties) return { name: 'Block', properties: [], groups: [] };

		const groups: UmbVisualEditorPropertyGroup[] = (data.containers ?? [])
			.filter((c) => c.type === 'Group')
			.map((c) => ({ id: c.id, name: c.name ?? '', sortOrder: c.sortOrder }))
			.sort((a, b) => a.sortOrder - b.sortOrder);

		const result: UmbVisualEditorPropertyInfo[] = [];
		for (const prop of data.properties) {
			let editorUiAlias = '';
			let config: UmbPropertyEditorConfig | undefined;

			if (prop.dataType.id) {
				const { data: dtData } = await tryExecute(
					this,
					DataTypeService.getDataTypeById({ path: { id: prop.dataType.id } }),
				);
				if (dtData) {
					editorUiAlias = dtData.editorUiAlias ?? '';
					config = dtData.values as UmbPropertyEditorConfig;
				}
			}

			result.push({
				alias: prop.alias,
				name: prop.name ?? prop.alias,
				description: prop.description ?? undefined,
				editorUiAlias: editorUiAlias || 'Umb.PropertyEditorUi.TextBox',
				config,
				validation: prop.validation,
				containerId: prop.container?.id ?? null,
			});
		}

		const resolved = { name: data.name ?? 'Block', properties: result, groups };
		this.#blockStructureCache.set(contentTypeKey, resolved);
		return resolved;
	}

	// --- Iframe ---

	#onIframeLoad() {
		this._iframeReady = true;
		this.#injectGuestScript();
		this.#restoreSelection();
		this.#sendNonEditableTypes();
	}

	#injectGuestScript() {
		const iframe = this.shadowRoot?.querySelector('iframe') as HTMLIFrameElement | null;
		if (!iframe) return;

		try {
			const doc = iframe.contentDocument;
			if (!doc?.body) return;
			if (doc.querySelector('script[data-umb-visual-editor]')) return;

			const script = doc.createElement('script');
			script.setAttribute('data-umb-visual-editor', '');
			script.textContent = 'console.warn("[VisualEditor] Fallback guest script — server injection expected.");';
			doc.body.appendChild(script);
		} catch {
			// Cross-origin — server-injected script handles it
		}
	}

	/**
	 * Tell the guest script which block content type aliases have no editable fields.
	 * The guest script hides the edit button on those blocks' action bars.
	 */
	async #sendNonEditableTypes() {
		const allValues = this.#getAllValues();
		const contentTypeKeys = new Set<string>();

		// Collect all unique content type keys from block values
		for (const val of allValues) {
			const raw = val.value as any;
			if (!raw?.contentData) continue;
			for (const block of raw.contentData) {
				if (block.contentTypeKey) contentTypeKeys.add(block.contentTypeKey);
			}
		}

		// Resolve which ones have no editable fields, collecting their aliases
		const nonEditableAliases: string[] = [];
		for (const key of contentTypeKeys) {
			const hasFields = await this.#blockHasEditableFields(key, '');
			if (!hasFields) {
				// Look up the alias from the document type
				const { data } = await tryExecute(this, DocumentTypeService.getDocumentTypeById({ path: { id: key } }));
				if (data?.alias) nonEditableAliases.push(data.alias);
			}
		}

		if (nonEditableAliases.length > 0) {
			// Small delay to ensure the guest script is initialized
			setTimeout(() => {
				this.#postToIframe({ type: 'umb:ve:non-editable-types', aliases: nonEditableAliases });
			}, 200);
		}
	}

	#restoreSelection() {
		if (this.#selectedPropertyAlias) {
			setTimeout(() => {
				this.#postToIframe({ type: 'umb:ve:select-region', regionId: 'prop:' + this.#selectedPropertyAlias });
			}, 100);
		} else if (this.#selectedBlockKey) {
			setTimeout(() => {
				this.#postToIframe({ type: 'umb:ve:select-region', regionId: 'block:' + this.#selectedBlockKey });
			}, 100);
		}
	}

	#postToIframe(message: Record<string, unknown>) {
		const iframe = this.shadowRoot?.querySelector('iframe') as HTMLIFrameElement | null;
		if (!iframe?.contentWindow) return;
		iframe.contentWindow.postMessage(message, '*');
	}

	// --- PostMessage handling ---

	override connectedCallback() {
		super.connectedCallback();
		window.addEventListener('message', this.#onMessage);
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		window.removeEventListener('message', this.#onMessage);
		this.#connection?.stop();
		this.#connection = undefined;
	}

	#onMessage = (event: MessageEvent) => {
		const data = event.data;
		if (!data || data.source !== 'umb-visual-editor-guest') return;

		switch (data.type) {
			case 'umb:ve:property-selected':
				this.#onPropertyClicked(data.propertyAlias);
				break;
			case 'umb:ve:block-selected':
				this.#onBlockClicked(data.blockKey, data.contentTypeAlias);
				break;
			case 'umb:ve:block-add':
				this.#onBlockAdd(data.siblingBlockKey, data.insertIndex ?? 0);
				break;
			case 'umb:ve:block-add-to-area':
				this.#onBlockAddToArea(data.parentBlockKey, data.areaAlias, data.insertIndex ?? 0);
				break;
			case 'umb:ve:block-move':
				this.#onBlockMove(data.blockKey, data.targetIndex ?? 0, data.targetParentBlockKey, data.targetAreaAlias);
				break;
			case 'umb:ve:block-delete':
				this.#onBlockDelete(data.blockKey);
				break;
			case 'umb:ve:block-reorder':
				this.#onBlockReorder(data.blockKey, data.toIndex ?? 0);
				break;
			case 'umb:ve:region-map':
				this._hasRegions = (data.regions?.length ?? 0) > 0;
				break;
		}
	};

	// --- Property click → open routed modal ---

	#onPropertyClicked(alias: string) {
		this.#selectedPropertyAlias = alias;
		this.#selectedBlockKey = undefined;

		const structure = this.#propertyStructures.find((p) => p.alias === alias);
		if (!structure?.editorUiAlias) return;

		this.#propertyModalRegistration.open({ propertyAlias: alias });
	}

	// --- Block config helpers ---

	#findBlockTypeConfig(
		propertyAlias: string,
		contentTypeKey: string,
	): { contentElementTypeKey: string; settingsElementTypeKey?: string; areas?: Array<{ key: string }> } | undefined {
		const propStructure = this.#propertyStructures.find((p) => p.alias === propertyAlias);
		if (!propStructure?.config) return undefined;

		const blocksConfig = (propStructure.config as Array<{ alias: string; value: unknown }>)?.find(
			(c) => c.alias === 'blocks',
		)?.value as Array<{ contentElementTypeKey: string; settingsElementTypeKey?: string }> | undefined;

		return blocksConfig?.find((b) => b.contentElementTypeKey === contentTypeKey) as
			| { contentElementTypeKey: string; settingsElementTypeKey?: string; areas?: Array<{ key: string }> }
			| undefined;
	}

	/**
	 * Check if a block type has any editable fields (content properties or settings).
	 * Layout blocks (e.g., grid layouts with only areas) return false.
	 * When propertyAlias is empty, searches all property structures for the block type config.
	 */
	async #blockHasEditableFields(contentTypeKey: string, propertyAlias?: string): Promise<boolean> {
		const { properties } = await this.#resolveBlockPropertyStructures(contentTypeKey);
		if (properties.length > 0) return true;

		// Find settings element type key from block type config
		let settingsElementTypeKey: string | undefined;
		if (propertyAlias) {
			settingsElementTypeKey = this.#findBlockTypeConfig(propertyAlias, contentTypeKey)?.settingsElementTypeKey;
		} else {
			// Search all property structures for a matching block type config
			for (const prop of this.#propertyStructures) {
				const config = this.#findBlockTypeConfig(prop.alias, contentTypeKey);
				if (config) {
					settingsElementTypeKey = config.settingsElementTypeKey;
					break;
				}
			}
		}

		if (settingsElementTypeKey) {
			const { properties: settingsProps } = await this.#resolveBlockPropertyStructures(settingsElementTypeKey);
			if (settingsProps.length > 0) return true;
		}

		return false;
	}

	// --- Block click → open block workspace via bridge ---

	async #onBlockClicked(blockKey: string, _contentTypeAlias: string) {
		this.#selectedBlockKey = blockKey;
		this.#selectedPropertyAlias = undefined;

		const allValues = this.#getAllValues();
		const found = findBlockInValues(allValues, blockKey);
		if (!found) {
			console.warn('[VisualEditor] block not found in values', blockKey);
			return;
		}

		const bridge = this.#ensureBridge(found.propertyAlias, found.blockValue);
		await bridge.openEdit(blockKey);
	}

	async #handlePropertySubmit(alias: string, result: { values: Array<{ alias: string; value: unknown }> }) {
		const entry = result.values.find((v) => v.alias === alias);
		if (entry !== undefined) {
			await this.#setPropertyValue(alias, entry.value);

			// Optimistic text update for immediate feedback
			this.#postToIframe({
				type: 'umb:ve:update-property-text',
				propertyAlias: alias,
				value: typeof entry.value === 'string' ? entry.value : '',
			});

			await this.#saveAndRefresh();
		}
	}

	async #handleBlockSubmit(
		blockKey: string,
		result: {
			values: Array<{ alias: string; value: unknown }>;
			settingsValues?: Array<{ alias: string; value: unknown }>;
		},
	) {
		const allValues = this.#getAllValues();
		const found = findBlockInValues(allValues, blockKey);
		if (found) {
			let updatedBlockValue = updateBlockPropertyValues(found.blockValue, blockKey, result.values);

			const settingsKey = found.layoutEntry?.settingsKey;
			if (settingsKey && result.settingsValues?.length) {
				updatedBlockValue = updateBlockSettingsValues(updatedBlockValue, settingsKey, result.settingsValues);
			}

			await this.#setPropertyValue(found.propertyAlias, updatedBlockValue);
		}

		await this.#saveAndRefresh();
	}

	// --- Block add ---

	async #onBlockAdd(siblingBlockKey: string, insertIndex: number) {
		const allValues = this.#getAllValues();
		const found = findBlockInValues(allValues, siblingBlockKey);
		if (!found) return;

		const propertyAlias = found.propertyAlias;
		const propertyValue = found.blockValue;
		if (!propertyValue?.contentData) return;

		const propStructure = this.#propertyStructures.find((p) => p.alias === propertyAlias);
		if (!propStructure?.config) return;

		const blocksConfig = (propStructure.config as Array<{ alias: string; value: unknown }>)?.find(
			(c) => c.alias === 'blocks',
		)?.value as
			| Array<{ contentElementTypeKey: string; label?: string; forceHideContentEditorInOverlay?: boolean }>
			| undefined;

		if (!blocksConfig || blocksConfig.length === 0) return;

		if (blocksConfig.length === 1) {
			// Single block type — skip catalogue, create directly
			const contentTypeKey = blocksConfig[0].contentElementTypeKey;
			const selectedBlockConfig = this.#findBlockTypeConfig(propertyAlias, contentTypeKey);

			const { updatedValue, contentKey } = addBlockToValue(
				propertyValue,
				contentTypeKey,
				insertIndex,
				undefined,
				selectedBlockConfig?.areas,
				selectedBlockConfig?.settingsElementTypeKey,
			);
			await this.#setPropertyValue(propertyAlias, updatedValue);

			const hasProperties = await this.#blockHasEditableFields(contentTypeKey, propertyAlias);
			if (hasProperties) {
				this.#onBlockClicked(contentKey, contentTypeKey);
			} else {
				await this.#saveAndRefresh();
			}
		} else {
			// Multiple block types — open routed catalogue modal
			this.#pendingAdd = { propertyAlias, propertyValue, insertIndex };
			this.#catalogueModalRegistration.open({ insertIndex });
		}
	}

	// --- Block add to area ---

	async #onBlockAddToArea(parentBlockKey: string, areaAlias: string, insertIndex: number) {
		const allValues = this.#getAllValues();
		const found = findBlockInValues(allValues, parentBlockKey);
		if (!found) return;

		const propertyAlias = found.propertyAlias;
		const propertyValue = found.blockValue;
		const parentBlock = found.block;
		const propStructure = this.#propertyStructures.find((p) => p.alias === propertyAlias);
		if (!propStructure?.config) return;

		const blocksConfig = (propStructure.config as Array<{ alias: string; value: unknown }>)?.find(
			(c) => c.alias === 'blocks',
		)?.value as Array<{ contentElementTypeKey: string; areas?: Array<{ key: string; alias: string }> }> | undefined;

		const parentBlockConfig = blocksConfig?.find((b) => b.contentElementTypeKey === parentBlock.contentTypeKey);
		const areaConfigs = parentBlockConfig?.areas ?? [];

		if (areaConfigs.length === 0) return;

		// Open routed catalogue modal
		this.#pendingAdd = { propertyAlias, propertyValue, insertIndex, parentBlockKey, areaAlias, areaConfigs };
		this.#catalogueModalRegistration.open({ insertIndex });
	}

	// --- Clipboard paste ---

	async #handleClipboardPaste(
		propertyAlias: string,
		propertyValue: BlockValue,
		insertIndex: number,
		clipboardSelection: Array<string>,
	) {
		const propStructure = this.#propertyStructures.find((p) => p.alias === propertyAlias);
		if (!propStructure?.editorUiAlias) return;

		const editorUiAlias = propStructure.editorUiAlias;
		const clipboardContext = await this.getContext(UMB_CLIPBOARD_CONTEXT);
		if (!clipboardContext) return;
		const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver<BlockValue>(this);

		// Determine the schema alias from the existing layout key
		const layoutKey = Object.keys(propertyValue.layout)[0] ?? UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS;

		let updatedValue = propertyValue;

		for (const unique of clipboardSelection) {
			const entry = await clipboardContext.read(unique);
			if (!entry) continue;

			// Translate clipboard entry to block value via paste translator
			const pastedValue = await valueResolver.resolve(entry.values, editorUiAlias);
			if (!pastedValue) continue;

			// Clone the value to generate new unique keys
			const cloner = new UmbPropertyValueCloneController(this);
			const cloned = await cloner.clone<BlockValue>({
				editorAlias: layoutKey,
				alias: editorUiAlias,
				value: pastedValue as BlockValue,
			});

			if (cloned.value) {
				// Expose all pasted blocks so they render in preview.
				// Use culture: null / segment: null (invariant) since block element
				// types typically don't vary. The block manager normally checks
				// variesByCulture/variesBySegment on the content type structure,
				// but that isn't available here. Invariant expose entries work for
				// both invariant and variant blocks.
				const pastedBlocks = cloned.value as BlockValue;
				for (const content of pastedBlocks.contentData) {
					pastedBlocks.expose.push({ contentKey: content.key, culture: null, segment: null });
				}

				updatedValue = mergeBlockValueInto(updatedValue, pastedBlocks, insertIndex);
				// Advance insert index so subsequent pastes go after the previous
				const pastedLayoutCount =
					pastedBlocks.layout[Object.keys(pastedBlocks.layout)[0] ?? '']?.length ?? 0;
				insertIndex += pastedLayoutCount;
			}
		}

		await this.#setPropertyValue(propertyAlias, updatedValue);
		await this.#saveAndRefresh();
	}

	// --- Block delete ---

	async #onBlockDelete(blockKey: string) {
		const allValues = this.#getAllValues();
		const found = findBlockInValues(allValues, blockKey);
		if (!found) return;

		const { name: blockName } = await this.#resolveBlockPropertyStructures(found.block.contentTypeKey);
		try {
			await umbConfirmModal(this, {
				headline: this.localize.term('blockEditor_confirmDeleteBlockTitle', blockName),
				content: this.localize.term('blockEditor_confirmDeleteBlockMessage', blockName),
				confirmLabel: this.localize.term('general_delete'),
				color: 'danger',
			});
		} catch {
			return; // User cancelled
		}

		const updatedValue = removeBlockFromValue(found.blockValue, blockKey);
		await this.#setPropertyValue(found.propertyAlias, updatedValue);

		await this.#saveAndRefresh();
	}

	// --- Block reorder / move ---

	async #onBlockMove(blockKey: string, targetIndex: number, targetParentBlockKey?: string, targetAreaAlias?: string) {
		const allValues = this.#getAllValues();
		const found = findBlockInValues(allValues, blockKey);
		if (!found) return;

		let areaConfigs: Array<{ key: string; alias: string }> | undefined;
		if (targetParentBlockKey && targetAreaAlias) {
			const propStructure = this.#propertyStructures.find((p) => p.alias === found.propertyAlias);
			const blocksConfig = (propStructure?.config as Array<{ alias: string; value: unknown }>)?.find(
				(c) => c.alias === 'blocks',
			)?.value as Array<{ contentElementTypeKey: string; areas?: Array<{ key: string; alias: string }> }> | undefined;

			const parentBlock = found.blockValue.contentData.find((b) => b.key === targetParentBlockKey);
			const parentConfig = blocksConfig?.find((b) => b.contentElementTypeKey === parentBlock?.contentTypeKey);
			areaConfigs = parentConfig?.areas;
		}

		const updatedValue = moveBlock(
			found.blockValue,
			blockKey,
			targetIndex,
			targetParentBlockKey,
			targetAreaAlias,
			areaConfigs,
		);
		await this.#setPropertyValue(found.propertyAlias, updatedValue);

		await this.#save();
	}

	async #onBlockReorder(blockKey: string, toIndex: number) {
		const allValues = this.#getAllValues();
		const found = findBlockInValues(allValues, blockKey);
		if (!found) return;

		const updatedValue = reorderBlockInValue(found.blockValue, blockKey, toIndex);
		await this.#setPropertyValue(found.propertyAlias, updatedValue);

		await this.#save();
	}

	// --- Save ---

	async #save() {
		this.#suppressRefresh = true;
		try {
			await this.#workspaceContext?.requestSave();
		} catch {
			// Save may fail due to validation — workspace shows notifications
		}
		setTimeout(() => {
			this.#suppressRefresh = false;
		}, 2000);
	}

	async #saveAndRefresh() {
		await this.#save();
		this.#refreshIframe();
	}

	// --- Preview URL ---

	#setPreviewUrl() {
		const unique = this.#workspaceContext?.getUnique();
		if (!unique || !this.#serverUrl) return;

		const url = new URL(unique, this.#serverUrl);
		url.searchParams.set('rnd', Date.now().toString());
		this._previewUrl = url.toString();
	}

	#refreshIframe() {
		this._iframeReady = false;
		this.#setPreviewUrl();
	}

	// --- SignalR ---

	async #initSignalR() {
		if (!this.#serverUrl) return;

		if (this.#connection) {
			await this.#connection.stop();
			this.#connection = undefined;
		}

		const hubUrl = `${this.#serverUrl}/umbraco/PreviewHub`;
		this.#connection = new HubConnectionBuilder().withUrl(hubUrl).build();

		this.#connection.on('refreshed', (payload: string) => {
			const unique = this.#workspaceContext?.getUnique();
			if (payload === unique && !this.#suppressRefresh) {
				this.#refreshIframe();
			}
		});

		try {
			await this.#connection.start();
		} catch (e) {
			console.error('[VisualEditor] SignalR connection failed', e);
		}
	}

	// --- Render ---

	override render() {
		return html`
			<div id="visual-editor-view">
				${!this._iframeReady ? html`<div id="loading"><uui-loader-circle></uui-loader-circle></div>` : nothing}
				${this._previewUrl
					? html`<iframe src=${this._previewUrl} title="Visual Editor" @load=${this.#onIframeLoad}></iframe>`
					: nothing}
				${this._iframeReady && !this._hasRegions
					? html`<div id="hint-bar">
							<small>Click any highlighted property or block on the page to edit it.</small>
						</div>`
					: nothing}
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				height: 100%;
			}

			#visual-editor-view {
				display: flex;
				flex-direction: column;
				height: 100%;
				position: relative;
			}

			#loading {
				display: flex;
				align-items: center;
				justify-content: center;
				position: absolute;
				inset: 0;
				font-size: 6rem;
				backdrop-filter: blur(var(--uui-size-1, 3px));
				z-index: 10;
			}

			iframe {
				flex: 1;
				width: 100%;
				border: none;
			}

			#hint-bar {
				position: absolute;
				bottom: 12px;
				left: 50%;
				transform: translateX(-50%);
				background: var(--uui-color-surface);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				padding: var(--uui-size-space-2) var(--uui-size-space-4);
				opacity: 0.7;
				z-index: 2;
				max-width: 400px;
				text-align: center;
			}
		`,
	];
}

export default UmbDocumentWorkspaceViewVisualEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-visual-editor': UmbDocumentWorkspaceViewVisualEditorElement;
	}
}
