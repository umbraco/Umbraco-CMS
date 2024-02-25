import type { UmbBlockDataType, UmbBlockLayoutBaseModel } from '../types.js';
import { UMB_BLOCK_WORKSPACE_MODAL } from '../workspace/block-workspace.modal-token.js';
import type { UmbBlockDataObjectModel, UmbBlockManagerContext } from './block-manager.context.js';
import { UMB_BLOCK_ENTRIES_CONTEXT } from './block-entries.context-token.js';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

export abstract class UmbBlockEntriesContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType, BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType>,
	BlockType extends UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel,
> extends UmbContextBase<
	UmbBlockEntriesContext<BlockManagerContextTokenType, BlockManagerContextType, BlockType, BlockLayoutType>
> {
	//
	_manager?: BlockManagerContextType;
	_retrieveManager;

	_workspaceModal: UmbModalRouteRegistrationController;

	protected _catalogueRouteBuilderState = new UmbBasicState<UmbModalRouteBuilder | undefined>(undefined);
	readonly catalogueRouteBuilder = this._catalogueRouteBuilderState.asObservable();

	#workspacePath = new UmbStringState(undefined);
	workspacePath = this.#workspacePath.asObservable();

	protected _layoutEntries = new UmbArrayState<BlockLayoutType>([], (x) => x.contentUdi);
	readonly layoutEntries = this._layoutEntries.asObservable();
	readonly layoutEntriesLength = this._layoutEntries.asObservablePart((x) => x.length);

	getLength() {
		return this._layoutEntries.getValue().length;
	}

	constructor(host: UmbControllerHost, blockManagerContextToken: BlockManagerContextTokenType) {
		super(host, UMB_BLOCK_ENTRIES_CONTEXT.toString());

		// TODO: Observe Blocks of the layout entries of this component.
		this._retrieveManager = this.consumeContext(blockManagerContextToken, (blockGridManager) => {
			this._manager = blockGridManager;
			this._gotBlockManager();

			this.observe(
				this._manager.propertyAlias,
				(alias) => {
					this._workspaceModal.setUniquePathValue('propertyAlias', alias);
				},
				'observePropertyAlias',
			);
			this.observe(
				this._manager.variantId,
				(variantId) => {
					// TODO: This might not be the property variant ID, but the content variant ID. Check up on what makes most sense?
					this._workspaceModal.setUniquePathValue('variantId', variantId?.toString());
				},
				'observePropertyVariantId',
			);
		}).asPromise();

		this._workspaceModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_WORKSPACE_MODAL)
			.addUniquePaths(['propertyAlias', 'variantId'])
			.addAdditionalPath('block')
			.onSetup(() => {
				return { data: { entityType: 'block', preset: {} }, modal: { size: 'medium' } };
			})
			.observeRouteBuilder((routeBuilder) => {
				const newPath = routeBuilder({});
				this.#workspacePath.setValue(newPath);
			});
	}

	async getManager() {
		await this._retrieveManager;
		return this._manager!;
	}

	protected abstract _gotBlockManager(): void;

	// Public methods:

	layoutOf(contentUdi: string) {
		return this._layoutEntries.asObservablePart((source) => source.find((x) => x.contentUdi === contentUdi));
	}
	setLayouts(layouts: Array<BlockLayoutType>) {
		return this._layoutEntries.setValue(layouts);
	}
	setOneLayout(layoutData: BlockLayoutType) {
		return this._layoutEntries.appendOne(layoutData);
	}

	public abstract getPathForCreateBlock(index: number): string | undefined;
	public abstract getPathForClipboard(index: number): string | undefined;

	public abstract create(
		contentElementTypeKey: string,
		layoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
		modalData?: typeof UMB_BLOCK_WORKSPACE_MODAL.DATA,
	): Promise<UmbBlockDataObjectModel<BlockLayoutType> | undefined>;

	abstract insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		modalData: typeof UMB_BLOCK_WORKSPACE_MODAL.DATA,
	): Promise<boolean>;
	//edit?
	//editSettings

	// Idea: should we return true if it was successful?
	public async delete(contentUdi: string) {
		await this._retrieveManager;
		const layout = this._layoutEntries.value.find((x) => x.contentUdi === contentUdi);
		if (!layout) {
			throw new Error(`Cannot delete block, missing layout for ${contentUdi}`);
		}

		if (layout.settingsUdi) {
			this._manager?.removeOneSettings(layout.settingsUdi);
		}

		this._manager?.removeOneContent(contentUdi);

		this._layoutEntries.removeOne(contentUdi);
	}
	//copy
}
