import type { UmbBlockTypeBaseModel } from '../../block-type/types.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '../types.js';
import { UMB_BLOCK_MANAGER_CONTEXT, type UmbBlockManagerContext } from '../manager/index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { encodeFilePath } from '@umbraco-cms/backoffice/utils';

export abstract class UmbBlockContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType, BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType>,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<
	UmbBlockContext<BlockManagerContextTokenType, BlockManagerContextType, BlockType, BlockLayoutType>
> {
	//
	_manager?: BlockManagerContextType;

	#blockTypeName = new UmbStringState(undefined);
	public readonly blockTypeName = this.#blockTypeName.asObservable();

	#label = new UmbStringState('');
	public readonly label = this.#label.asObservable();

	#workspacePath = new UmbStringState(undefined);
	public readonly workspacePath = this.#workspacePath.asObservable();
	public readonly workspaceEditPath = this.#workspacePath.asObservablePart(
		(path) => path + 'edit/' + encodeFilePath(this.getContentUdi() ?? ''),
	);
	public readonly workspaceEditSettingsPath = this.#workspacePath.asObservablePart(
		(path) => path + 'edit/' + encodeFilePath(this.getContentUdi() ?? '') + '/view/settings',
	);

	_blockType = new UmbObjectState<BlockType | undefined>(undefined);
	public readonly blockType = this._blockType.asObservable();
	public readonly blockTypeContentElementTypeKey = this._blockType.asObservablePart((x) => x?.contentElementTypeKey);
	public readonly blockTypeSettingsElementTypeKey = this._blockType.asObservablePart((x) => x?.settingsElementTypeKey);

	#layout = new UmbObjectState<BlockLayoutType | undefined>(undefined);
	public readonly layout = this.#layout.asObservable();
	public readonly contentUdi = this.#layout.asObservablePart((x) => x?.contentUdi);

	#content = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly content = this.#content.asObservable();
	public readonly contentTypeKey = this.#content.asObservablePart((x) => x?.contentTypeKey);

	#settings = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly settings = this.#settings.asObservable();

	/**
	 * Set the layout entry object.
	 * @method setLayout
	 * @param {BlockLayoutType | undefined} layout entry object.
	 * @returns {void}
	 */
	setLayout(layout: BlockLayoutType | undefined) {
		this.#layout.setValue(layout);
	}

	constructor(host: UmbControllerHost, blockManagerContextToken: BlockManagerContextTokenType) {
		super(host, UMB_BLOCK_CONTEXT.toString());

		// Consume block manager:
		this.consumeContext(blockManagerContextToken, (manager) => {
			this._manager = manager;
			this._gotManager();
		});

		// Observe UDI:
		this.observe(this.contentUdi, (contentUdi) => {
			if (!contentUdi) return;
			this.#observeData();
		});

		// Observe contentElementTypeKey:
		this.observe(this.contentTypeKey, (contentElementTypeKey) => {
			if (!contentElementTypeKey) return;
			this.#observeBlockType();
		});

		// Observe blockType:
		this.observe(this.blockType, (blockType) => {
			if (!blockType) return;
			this.#observeBlockTypeContentElementName();
			this.#observeBlockTypeLabel();
		});
	}

	getContentUdi() {
		return this.#layout.value?.contentUdi;
	}

	_gotManager() {
		if (this._manager) {
			this.observe(
				this._manager.workspacePath,
				(workspacePath) => {
					this.#workspacePath.setValue(workspacePath);
				},
				'observeWorkspacePath',
			);
		} else {
			this.removeControllerByAlias('observeWorkspacePath');
		}
		this.#observeBlockType();
		this.#observeData();
	}

	#observeData() {
		if (!this._manager) return;
		const contentUdi = this.#layout.value?.contentUdi;
		if (!contentUdi) return;

		// observe content:
		this.observe(
			this._manager.contentOf(contentUdi),
			(content) => {
				this.#content.setValue(content);
			},
			'observeContent',
		);

		// observe settings:
		const settingsUdi = this.#layout.value?.settingsUdi;
		if (settingsUdi) {
			this.observe(
				this._manager.contentOf(settingsUdi),
				(content) => {
					this.#settings.setValue(content);
				},
				'observeSettings',
			);
		}
	}

	#observeBlockType() {
		if (!this._manager) return;
		const contentTypeKey = this.#content.value?.contentTypeKey;
		if (!contentTypeKey) return;

		// observe blockType:
		this.observe(
			this._manager.blockTypeOf(contentTypeKey),
			(blockType) => {
				this._blockType.setValue(blockType as BlockType);
			},
			'observeBlockType',
		);
	}

	#observeBlockTypeContentElementName() {
		if (!this._manager) return;
		const contentElementTypeKey = this._blockType.value?.contentElementTypeKey;
		if (!contentElementTypeKey) return;

		// observe blockType:
		this.observe(
			this._manager.contentTypeNameOf(contentElementTypeKey),
			(contentTypeName) => {
				this.#blockTypeName.setValue(contentTypeName);
			},
			'observeBlockTypeContentElementTypeName',
		);
	}

	#observeBlockTypeLabel() {
		if (!this._manager) return;
		const blockType = this._blockType.value;
		if (!blockType) return;

		if (blockType.label) {
			this.removeControllerByAlias('observeContentTypeName');
			// Missing part for label syntax, as we need to store the syntax, interpretive it and then set the label: (here we are just parsing the label syntax)
			this.#label.setValue(blockType.label);
			return;
		} else {
			// TODO: Maybe this could be skipped if we had a fallback label which was set to get the content element type name?
			// Get the name of the content element type for label:
			this.observe(
				this.blockTypeName,
				(contentTypeName) => {
					this.#label.setValue(contentTypeName ?? 'no name');
				},
				'observeBlockTypeName',
			);
		}
	}

	// Public methods:

	public delete() {
		if (!this._manager) return;
		const contentUdi = this.#layout.value?.contentUdi;
		if (!contentUdi) return;
		this._manager.deleteBlock(contentUdi);
	}
}

export const UMB_BLOCK_CONTEXT = new UmbContextToken<
	UmbBlockContext<typeof UMB_BLOCK_MANAGER_CONTEXT, typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE>
>('UmbBlockContext');
