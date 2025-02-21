import type { UmbBlockDataModel } from '../../block/index.js';
import { UMB_BLOCK_CATALOGUE_MODAL, UmbBlockEntriesContext } from '../../block/index.js';
import type { UmbBlockListWorkspaceOriginData } from '../index.js';
import {
	UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS,
	UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
	UMB_BLOCK_LIST_WORKSPACE_MODAL,
} from '../index.js';
import type { UmbBlockListLayoutModel, UmbBlockListTypeModel, UmbBlockListValueModel } from '../types.js';
import { UMB_BLOCK_LIST_MANAGER_CONTEXT } from './block-list-manager.context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import {
	UMB_CLIPBOARD_PROPERTY_CONTEXT,
	UmbClipboardPastePropertyValueTranslatorValueResolver,
} from '@umbraco-cms/backoffice/clipboard';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

export class UmbBlockListEntriesContext extends UmbBlockEntriesContext<
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT,
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT.TYPE,
	UmbBlockListTypeModel,
	UmbBlockListLayoutModel,
	UmbBlockListWorkspaceOriginData
> {
	//

	// We will just say its always allowed for list for now: [NL]
	public readonly canCreate = new UmbBooleanState(true).asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_LIST_MANAGER_CONTEXT);

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addAdditionalPath('_catalogue/:view/:index')
			.onSetup(async (routingInfo) => {
				await this._retrieveManager;
				if (!this._manager) return false;
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);

				const pasteTranslatorManifests = clipboardContext.getPasteTranslatorManifests(
					UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
				);

				// TODO: consider moving some of this logic to the clipboard property context
				const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
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
								UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
							);

							if (pasteTranslator.isCompatibleValue) {
								const value = await valueResolver.resolve(
									clipboardEntryDetail.values,
									UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
								);
								return pasteTranslator.isCompatibleValue(value, config);
							}

							return true;
						},
						originData: { index: index },
						createBlockInWorkspace: this._manager.getInlineEditingMode() === false,
					},
				};
			})
			.onSubmit(async (value, data) => {
				if (value?.create && data) {
					const created = await this.create(
						value.create.contentElementTypeKey,
						{},
						data.originData as UmbBlockListWorkspaceOriginData,
					);
					if (created) {
						this.insert(
							created.layout,
							created.content,
							created.settings,
							data.originData as UmbBlockListWorkspaceOriginData,
						);
					} else {
						throw new Error('Failed to create block');
					}
				} else if (value?.clipboard && value.clipboard.selection?.length && data) {
					const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);

					const propertyValues = await clipboardContext.readMultiple<UmbBlockListValueModel>(
						value.clipboard.selection,
						UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
					);

					this._insertFromPropertyValues(propertyValues, data.originData as UmbBlockListWorkspaceOriginData);
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				this._catalogueRouteBuilderState.setValue(routeBuilder);
			});

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_LIST_WORKSPACE_MODAL)
			.addAdditionalPath('block')
			.onSetup(() => {
				return {
					data: { entityType: 'block', preset: {}, baseDataPath: this._dataPath },
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

	getPathForCreateBlock(index: number) {
		return this._catalogueRouteBuilderState.getValue()?.({ view: 'create', index: index });
	}

	getPathForClipboard(index: number) {
		return this._catalogueRouteBuilderState.getValue()?.({ view: 'clipboard', index: index });
	}

	override async setLayouts(layouts: Array<UmbBlockListLayoutModel>) {
		await this._retrieveManager;
		this._manager?.setLayouts(layouts);
	}

	async create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<UmbBlockListLayoutModel, 'contentKey'>,
		originData?: UmbBlockListWorkspaceOriginData,
	) {
		await this._retrieveManager;
		return await this._manager?.create(contentElementTypeKey, partialLayoutEntry, originData);
	}

	// insert Block?

	async insert(
		layoutEntry: UmbBlockListLayoutModel,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockListWorkspaceOriginData,
	) {
		await this._retrieveManager;

		return this._manager?.insert(layoutEntry, content, settings, originData) ?? false;
	}

	protected async _insertFromPropertyValue(value: UmbBlockListValueModel, originData: UmbBlockListWorkspaceOriginData) {
		const layoutEntries = value.layout[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS];

		if (!layoutEntries) {
			throw new Error('No layout entries found');
		}

		await Promise.all(
			layoutEntries.map(async (layoutEntry) => {
				this._insertBlockFromPropertyValue(layoutEntry, value, originData);
				if (originData.index !== -1) {
					originData = { ...originData, index: originData.index + 1 };
				}
			}),
		);

		return originData;
	}
}
