import { UMB_BLOCK_RTE_WORKSPACE_MODAL } from '../workspace/block-rte-workspace.modal-token.js';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockRteWorkspaceOriginData } from '../workspace/block-rte-workspace.modal-token.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from './block-rte-manager.context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbClipboardPastePropertyValueTranslatorValueResolver,
	UMB_CLIPBOARD_PROPERTY_CONTEXT,
} from '@umbraco-cms/backoffice/clipboard';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_BLOCK_CATALOGUE_MODAL, UmbBlockEntriesContext } from '@umbraco-cms/backoffice/block';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPropertyEditorRteValueType } from '@umbraco-cms/backoffice/rte';

/**
 * Copied from the 'rte' package to avoid a circular dependency.
 * @internal
 */
const UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS = 'Umbraco.RichText';

/**
 * Copied from the 'tiptap' package to avoid a circular dependency.
 * @internal
 */
const UMB_BLOCK_RTE_PROPERTY_EDITOR_UI_ALIAS = 'Umb.PropertyEditorUi.Tiptap';

export class UmbBlockRteEntriesContext extends UmbBlockEntriesContext<
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT,
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE,
	UmbBlockRteTypeModel,
	UmbBlockRteLayoutModel,
	UmbBlockRteWorkspaceOriginData
> {
	//

	// We will just say its always allowed for RTE for now: [NL]
	public readonly canCreate = new UmbBooleanState(true).asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_RTE_MANAGER_CONTEXT);

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addAdditionalPath('_catalogue/:view')
			.onSetup(async (routingInfo) => {
				await this._retrieveManager;
				if (!this._manager) return false;

				const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);
				if (!clipboardContext) {
					throw new Error('Clipboard context not found');
				}

				const pasteTranslatorManifests = clipboardContext.getPasteTranslatorManifests(
					UMB_BLOCK_RTE_PROPERTY_EDITOR_UI_ALIAS,
				);

				// TODO: consider moving some of this logic to the clipboard property context
				const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
				if (!propertyContext) {
					throw new Error('Property context not found');
				}

				const config = propertyContext.getConfig();
				const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver(this);

				return {
					data: {
						blocks: this._manager?.getBlockTypes() ?? [],
						blockGroups: [],
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
								UMB_BLOCK_RTE_PROPERTY_EDITOR_UI_ALIAS,
							);

							if (pasteTranslator.isCompatibleValue) {
								const value = await valueResolver.resolve(
									clipboardEntryDetail.values,
									UMB_BLOCK_RTE_PROPERTY_EDITOR_UI_ALIAS,
								);
								return pasteTranslator.isCompatibleValue(value, config);
							}

							return true;
						},
						originData: {},
						createBlockInWorkspace: true,
					},
				};
			})
			.onSubmit(async (value, data) => {
				if (value?.create && data) {
					const created = await this.create(
						value.create.contentElementTypeKey,
						{},
						data.originData as UmbBlockRteWorkspaceOriginData,
					);
					if (created) {
						await this.insert(
							created.layout,
							created.content,
							created.settings,
							data.originData as UmbBlockRteWorkspaceOriginData,
						);
					} else {
						throw new Error('Failed to create block');
					}
				} else if (value?.clipboard && value.clipboard.selection?.length && data) {
					const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);
					if (!clipboardContext) {
						throw new Error('Clipboard context not found');
					}

					const propertyValues = await clipboardContext.readMultiple<UmbPropertyEditorRteValueType>(
						value.clipboard.selection,
						UMB_BLOCK_RTE_PROPERTY_EDITOR_UI_ALIAS,
					);

					await this.#insertFromRtePropertyValues(propertyValues, data.originData as UmbBlockRteWorkspaceOriginData);
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				this._catalogueRouteBuilderState.setValue(routeBuilder);
			});

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_RTE_WORKSPACE_MODAL)
			.addAdditionalPath('block')
			.onSetup(() => {
				return { data: { entityType: 'block', preset: {}, baseDataPath: this._dataPath }, modal: { size: 'medium' } };
			})
			.observeRouteBuilder((routeBuilder) => {
				const newPath = routeBuilder({});
				this._workspacePath.setValue(newPath);
			});
	}

	protected _gotBlockManager() {
		if (!this._manager) return;

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
	}

	getPathForCreateBlock() {
		return this._catalogueRouteBuilderState.getValue()?.({ view: 'create' });
	}

	getPathForClipboard() {
		return this._catalogueRouteBuilderState.getValue()?.({ view: 'clipboard' });
	}

	override async setLayouts(layouts: Array<UmbBlockRteLayoutModel>) {
		await this._retrieveManager;
		this._manager?.setLayouts(layouts);
	}

	async create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<UmbBlockRteLayoutModel, 'contentKey'>,
		originData?: UmbBlockRteWorkspaceOriginData,
	) {
		await this._retrieveManager;
		return await this._manager?.createWithPresets(contentElementTypeKey, partialLayoutEntry, originData);
	}

	// insert Block?

	async insert(
		layoutEntry: UmbBlockRteLayoutModel,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockRteWorkspaceOriginData,
	) {
		await this._retrieveManager;
		return this._manager?.insert(layoutEntry, content, settings, originData) ?? false;
	}

	// create Block?
	override async delete(contentKey: string) {
		await super.delete(contentKey);
		this._manager?.deleteLayoutElement(contentKey);
	}

	async #insertFromRtePropertyValues(
		values: Array<UmbPropertyEditorRteValueType>,
		originData: UmbBlockRteWorkspaceOriginData,
	) {
		for (const value of values) {
			originData = await this.#insertFromRtePropertyValue(value, originData);
		}
	}

	async #insertFromRtePropertyValue(
		value: UmbPropertyEditorRteValueType,
		originData: UmbBlockRteWorkspaceOriginData,
	): Promise<UmbBlockRteWorkspaceOriginData> {
		if (!value.blocks) {
			throw new Error('No blocks found in property value');
		}

		const layoutEntries = value.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS];

		if (!layoutEntries) {
			throw new Error('No layout entries found');
		}

		await Promise.all(
			layoutEntries.map(async (layoutEntry) => {
				this._insertBlockFromPropertyValue(layoutEntry, value.blocks!, originData);
				// TODO: Missing some way to insert a Block HTML Element into the RTE at the current cursor point. (hopefully the responsibility can be avoided here, but there is some connection missing at this point) [NL]
			}),
		);

		return originData;
	}

	// This method is required by the base class but is not used for RTE blocks.
	// RTE blocks use `#insertFromRtePropertyValue` instead because they expect
	// `UmbPropertyEditorRteValueType` (with markup and blocks) rather than `UmbBlockValueType`.
	protected override _insertFromPropertyValue(
		_value: unknown,
		_originData: UmbBlockRteWorkspaceOriginData,
	): Promise<UmbBlockRteWorkspaceOriginData> {
		throw new Error('Use #insertFromRtePropertyValue for RTE blocks');
	}
}
