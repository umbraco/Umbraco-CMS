import type { UmbBlockWorkspaceOriginData } from '../workspace/block-workspace.modal-token.js';
import type { UmbBlockDataModel, UmbBlockLayoutBaseModel, UmbBlockValueType } from '../types.js';
import type { UmbBlockDataObjectModel, UmbBlockManagerContext } from './block-manager.context.js';
import { UMB_BLOCK_ENTRIES_CONTEXT } from './block-entries.context-token.js';
import { type Observable, UmbArrayState, UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

export abstract class UmbBlockEntriesContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType, BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType, BlockOriginData>,
	BlockType extends UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel,
	BlockOriginData extends UmbBlockWorkspaceOriginData,
> extends UmbContextBase {
	//
	protected _manager?: BlockManagerContextType;
	protected _retrieveManager;

	protected _catalogueRouteBuilderState = new UmbBasicState<UmbModalRouteBuilder | undefined>(undefined);
	readonly catalogueRouteBuilder = this._catalogueRouteBuilderState.asObservable();

	protected _workspacePath = new UmbStringState(undefined);
	workspacePath = this._workspacePath.asObservable();

	protected _dataPath?: string;

	public abstract readonly canCreate: Observable<boolean>;

	protected _layoutEntries = new UmbArrayState<BlockLayoutType>([], (x) => x.key);
	readonly layoutEntries = this._layoutEntries.asObservable();
	readonly layoutEntriesLength = this._layoutEntries.asObservablePart((x) => x.length);

	getLength() {
		return this._layoutEntries.getValue().length;
	}

	constructor(host: UmbControllerHost, blockManagerContextToken: BlockManagerContextTokenType) {
		super(host, UMB_BLOCK_ENTRIES_CONTEXT);

		// TODO: Observe Blocks of the layout entries of this component.
		this._retrieveManager = this.consumeContext(blockManagerContextToken, (blockGridManager) => {
			this._manager = blockGridManager;
			this._gotBlockManager();
		}).asPromise({ preventTimeout: true });
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

	layoutOf(contentKey: string) {
		return this._layoutEntries.asObservablePart((source) => source.find((x) => x.contentKey === contentKey));
	}
	byKey(key: string) {
		return this._layoutEntries.asObservablePart((source) => source.find((x) => x.key === key));
	}
	getLayoutOf(contentKey: string) {
		return this._layoutEntries.getValue().find((x) => x.contentKey === contentKey);
	}
	getByKey(key: string) {
		return this._layoutEntries.getValue().find((x) => x.key === key);
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
		layoutEntry?: Omit<BlockLayoutType, 'contentKey' | 'key'>,
		originData?: BlockOriginData,
	): Promise<UmbBlockDataObjectModel<BlockLayoutType> | undefined>;

	abstract insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: BlockOriginData,
	): Promise<boolean>;

	public async delete(key: string) {
		await this._retrieveManager;
		const layout = this._layoutEntries.value.find((x) => x.key === key);
		if (!layout) {
			throw new Error(`Cannot delete block, missing layout for ${key}`);
		}
		this._layoutEntries.removeOne(key);

		// Only remove content/settings/exposes if no other layout references the same contentKey
		const hasOtherReference = this._layoutEntries.value.some(
			(x) => x.key !== key && x.contentKey === layout.contentKey,
		);
		if (!hasOtherReference) {
			this._manager!.removeOneContent(layout.contentKey);
			if (layout.settingsKey) {
				this._manager!.removeOneSettings(layout.settingsKey);
			}
			this._manager!.removeExposesOf(layout.contentKey);
		}
	}

	// insert/paste from property value methods:

	protected async _insertFromPropertyValues(values: Array<UmbBlockValueType>, originData: BlockOriginData) {
		await Promise.all(
			values.map(async (value) => {
				originData = await this._insertFromPropertyValue(value, originData);
			}),
		);
	}

	protected abstract _insertFromPropertyValue(
		values: UmbBlockValueType,
		originData: BlockOriginData,
	): Promise<BlockOriginData>;

	protected async _insertBlockFromPropertyValue(
		layoutEntry: BlockLayoutType,
		value: UmbBlockValueType,
		originData: BlockOriginData,
	) {
		// Library-element references have no inline contentData — insert as a reference instead.
		if (layoutEntry.isSharedContent) {
			await this._manager?.insertLibraryElement(layoutEntry.contentKey, originData);
			return;
		}

		const content = value.contentData.find((x) => x.key === layoutEntry.contentKey);
		if (!content) {
			throw new Error('No content found for layout entry');
		}
		const settings = value.settingsData.find((x) => x.key === layoutEntry.settingsKey);
		await this.insert(layoutEntry, content, settings, originData);
	}
}
