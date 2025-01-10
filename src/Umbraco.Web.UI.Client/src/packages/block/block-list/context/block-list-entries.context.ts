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
import { UMB_CONTENT_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UMB_CLIPBOARD_CONTEXT } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListEntriesContext extends UmbBlockEntriesContext<
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT,
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT.TYPE,
	UmbBlockListTypeModel,
	UmbBlockListLayoutModel,
	UmbBlockListWorkspaceOriginData
> {
	//
	#catalogueModal: UmbModalRouteRegistrationController<
		typeof UMB_BLOCK_CATALOGUE_MODAL.DATA,
		typeof UMB_BLOCK_CATALOGUE_MODAL.VALUE
	>;
	#workspaceModal;

	// We will just say its always allowed for list for now: [NL]
	public readonly canCreate = new UmbBooleanState(true).asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_LIST_MANAGER_CONTEXT);

		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId'])
			.addAdditionalPath(':view/:index')
			.onSetup(async (routingInfo) => {
				await this._retrieveManager;
				if (!this._manager) return false;
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				const clipboardContext = await this.getContext(UMB_CLIPBOARD_CONTEXT);
				const pasteTranslatorManifests = clipboardContext.getPastePropertyValueTranslatorManifests(
					UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
				);
				return {
					data: {
						blocks: this._manager?.getBlockTypes() ?? [],
						blockGroups: [],
						openClipboard: routingInfo.view === 'clipboard',
						clipboardFilter: (clipboardEntryDetailModel) => {
							const hasSupportedTranslator = clipboardContext.hasSupportedPastePropertyValueTranslator(
								pasteTranslatorManifests,
								clipboardEntryDetailModel.values,
							);
							return hasSupportedTranslator;
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
				} else if (value?.pasteFromClipboard && value.pasteFromClipboard.selection?.length && data) {
					const clipboardContext = await this.getContext(UMB_CLIPBOARD_CONTEXT);

					const propertyValues = await clipboardContext.readMultipleForProperty<UmbBlockListValueModel>(
						value.pasteFromClipboard.selection,
						UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
					);

					this.#insertPropertyValues(propertyValues, data.originData as UmbBlockListWorkspaceOriginData);
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				this._catalogueRouteBuilderState.setValue(routeBuilder);
			});

		this.#workspaceModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_LIST_WORKSPACE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId'])
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

		// TODO: This must later be switched out with a smarter Modal Registration System, cause here is a issue with Block Editors in inline mode in Block Editors, cause the hosting Block is also of type Content. [NL]
		this.consumeContext(UMB_CONTENT_PROPERTY_DATASET_CONTEXT, (dataset) => {
			const variantId = dataset.getVariantId();
			this.#catalogueModal.setUniquePathValue('variantId', variantId?.toString());
			this.#workspaceModal.setUniquePathValue('variantId', variantId?.toString());
		});
	}

	#insertPropertyValues(values: Array<UmbBlockListValueModel>, originData: UmbBlockListWorkspaceOriginData) {
		values.forEach((value) => {
			this.#insertPropertyValue(value, originData);
		});
	}

	#insertPropertyValue(values: UmbBlockListValueModel, originData: UmbBlockListWorkspaceOriginData) {
		console.log('insert property value:', values);

		const layoutEntries = values.layout[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS];

		if (!layoutEntries) {
			throw new Error('No layout entries found');
		}

		for (const layoutEntry of layoutEntries) {
			this.#insertBlockFromPropertyValue(layoutEntry, values, originData);
			originData.index++;
		}
	}

	#insertBlockFromPropertyValue(
		layoutEntry: UmbBlockListLayoutModel,
		value: UmbBlockListValueModel,
		originData: UmbBlockListWorkspaceOriginData,
	) {
		const content = value.contentData.find((x) => x.key === layoutEntry.contentKey);
		if (!content) {
			throw new Error('No content found for layout entry');
		}
		const settings = value.settingsData.find((x) => x.key === layoutEntry.settingsKey);
		this.insert(layoutEntry, content, settings, originData);
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

		this.observe(
			this._manager.propertyAlias,
			(alias) => {
				this.#catalogueModal.setUniquePathValue('propertyAlias', alias ?? 'null');
				this.#workspaceModal.setUniquePathValue('propertyAlias', alias ?? 'null');
			},
			'observePropertyAlias',
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
		return this._manager?.create(contentElementTypeKey, partialLayoutEntry, originData);
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

	// create Block?
	override async delete(contentKey: string) {
		// TODO: Loop through children and delete them as well?
		await super.delete(contentKey);
	}
}
