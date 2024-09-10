import type { UmbBlockDataModel } from '../../block/index.js';
import { UMB_BLOCK_CATALOGUE_MODAL, UmbBlockEntriesContext } from '../../block/index.js';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import {
	UMB_BLOCK_RTE_WORKSPACE_MODAL,
	type UmbBlockRteWorkspaceOriginData,
} from '../workspace/block-rte-workspace.modal-token.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from './block-rte-manager.context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';

export class UmbBlockRteEntriesContext extends UmbBlockEntriesContext<
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT,
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE,
	UmbBlockRteTypeModel,
	UmbBlockRteLayoutModel,
	UmbBlockRteWorkspaceOriginData
> {
	//
	#catalogueModal: UmbModalRouteRegistrationController<
		typeof UMB_BLOCK_CATALOGUE_MODAL.DATA,
		typeof UMB_BLOCK_CATALOGUE_MODAL.VALUE
	>;
	#workspaceModal;

	// We will just say its always allowed for RTE for now: [NL]
	public readonly canCreate = new UmbBooleanState(true).asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_RTE_MANAGER_CONTEXT);

		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId'])
			.addAdditionalPath(':view')
			.onSetup((routingInfo) => {
				return {
					data: {
						blocks: this._manager?.getBlockTypes() ?? [],
						blockGroups: [],
						openClipboard: routingInfo.view === 'clipboard',
						originData: {},
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
						data.originData as UmbBlockRteWorkspaceOriginData,
					);
					if (created) {
						this.insert(
							created.layout,
							created.content,
							created.settings,
							data.originData as UmbBlockRteWorkspaceOriginData,
						);
					} else {
						throw new Error('Failed to create block');
					}
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				this._catalogueRouteBuilderState.setValue(routeBuilder);
			});

		this.#workspaceModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_RTE_WORKSPACE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId'])
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
		return this._manager?.create(contentElementTypeKey, partialLayoutEntry, originData);
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
		// TODO: Loop through children and delete them as well?
		await super.delete(contentKey);
		this._manager?.deleteLayoutElement(contentKey);
	}
}
