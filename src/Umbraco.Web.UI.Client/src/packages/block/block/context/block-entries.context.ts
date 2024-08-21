import type { UmbBlockWorkspaceOriginData } from '../workspace/block-workspace.modal-token.js';
import type { UmbBlockDataType, UmbBlockLayoutBaseModel } from '../types.js';
import type { UmbBlockDataObjectModel, UmbBlockManagerContext } from './block-manager.context.js';
import { UMB_BLOCK_ENTRIES_CONTEXT } from './block-entries.context-token.js';
import { type Observable, UmbArrayState, UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

export abstract class UmbBlockEntriesContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType, BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType, BlockOriginData>,
	BlockType extends UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel,
	BlockOriginData extends UmbBlockWorkspaceOriginData,
> extends UmbContextBase<
	UmbBlockEntriesContext<
		BlockManagerContextTokenType,
		BlockManagerContextType,
		BlockType,
		BlockLayoutType,
		BlockOriginData
	>
> {
	//
	_manager?: BlockManagerContextType;
	_retrieveManager;

	protected _catalogueRouteBuilderState = new UmbBasicState<UmbModalRouteBuilder | undefined>(undefined);
	readonly catalogueRouteBuilder = this._catalogueRouteBuilderState.asObservable();

	protected _workspacePath = new UmbStringState(undefined);
	workspacePath = this._workspacePath.asObservable();

	protected _dataPath?: string;

	public abstract readonly canCreate: Observable<boolean>;

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
		}).asPromise();
	}

	async getManager() {
		await this._retrieveManager;
		return this._manager!;
	}

	setDataPath(path: string) {
		this._dataPath = path;
	}

	protected abstract _gotBlockManager(): void;

	// Public methods:

	layoutOf(contentUdi: string) {
		return this._layoutEntries.asObservablePart((source) => source.find((x) => x.contentUdi === contentUdi));
	}
	getLayoutOf(contentUdi: string) {
		return this._layoutEntries.getValue().find((x) => x.contentUdi === contentUdi);
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
		originData?: BlockOriginData,
	): Promise<UmbBlockDataObjectModel<BlockLayoutType> | undefined>;

	abstract insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		originData: BlockOriginData,
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
			this._manager!.removeOneSettings(layout.settingsUdi);
		}
		this._manager!.removeOneContent(contentUdi);

		this._layoutEntries.removeOne(contentUdi);
	}
	//copy
}
