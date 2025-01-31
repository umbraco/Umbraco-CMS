import type { UmbBlockDataModel } from '../../block/index.js';
import { UMB_BLOCK_CATALOGUE_MODAL, UmbBlockEntriesContext } from '../../block/index.js';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel, UmbBlockRteValueModel } from '../types.js';
import {
	UMB_BLOCK_RTE_WORKSPACE_MODAL,
	type UmbBlockRteWorkspaceOriginData,
} from '../workspace/block-rte-workspace.modal-token.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from './block-rte-manager.context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/rte';

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
		await super.delete(contentKey);
		this._manager?.deleteLayoutElement(contentKey);
	}

	protected async _insertFromPropertyValue(value: UmbBlockRteValueModel, originData: UmbBlockRteWorkspaceOriginData) {
		const layoutEntries = value.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS];

		if (!layoutEntries) {
			throw new Error('No layout entries found');
		}

		await Promise.all(
			layoutEntries.map(async (layoutEntry) => {
				this._insertBlockFromPropertyValue(layoutEntry, value, originData);
				// TODO: Missing some way to insert a Block HTML Element into the RTE at the current cursor point. (hopefully the responsibilit can be avoided here, but there is some connection missing at this point) [NL]
			}),
		);

		return originData;
	}
}
