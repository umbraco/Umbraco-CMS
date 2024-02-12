import type { Observable } from '@umbraco-cms/backoffice/external/rxjs/index.js';
import type { UmbBlockTypeBaseModel } from '../../block-type/types.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '../types.js';
import type { UmbBlockManagerContext } from '../index.js';
import type { UmbBlockEntriesContext } from './block-entries.context.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbNumberState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { encodeFilePath } from '@umbraco-cms/backoffice/utils';
import { UMB_CONFIRM_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export abstract class UmbBlockEntryContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType>,
	BlockEntriesContextTokenType extends UmbContextToken<BlockEntriesContextType>,
	BlockEntriesContextType extends UmbBlockEntriesContext<
		BlockManagerContextTokenType,
		BlockManagerContextType,
		BlockType,
		BlockLayoutType
	>,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<any> {
	//
	_manager?: BlockManagerContextType;
	_entries?: BlockEntriesContextType;

	#contentUdi?: string;

	#index = new UmbNumberState(undefined);
	readonly index = this.#index.asObservable();
	getIndex() {
		return this.#index.value;
	}
	setIndex(index: number | undefined) {
		this.#index.setValue(index);
	}

	#createPath = new UmbStringState(undefined);
	readonly createPath = this.#createPath.asObservable();

	#contentElementTypeName = new UmbStringState(undefined);
	public readonly contentElementTypeName = this.#contentElementTypeName.asObservable();
	#contentElementTypeAlias = new UmbStringState(undefined);
	public readonly contentElementTypeAlias = this.#contentElementTypeAlias.asObservable();

	// TODO: index state + observable?

	#label = new UmbStringState('');
	public readonly label = this.#label.asObservable();

	#generateWorkspaceEditContentPath = (path?: string) =>
		path ? path + 'edit/' + encodeFilePath(this.getContentUdi() ?? '') + '/view/content' : '';

	#generateWorkspaceEditSettingsPath = (path?: string) =>
		path ? path + 'edit/' + encodeFilePath(this.getContentUdi() ?? '') + '/view/settings' : '';

	#workspacePath = new UmbStringState(undefined);
	public readonly workspacePath = this.#workspacePath.asObservable();
	public readonly workspaceEditContentPath = this.#workspacePath.asObservablePart(
		this.#generateWorkspaceEditContentPath,
	);
	public readonly workspaceEditSettingsPath = this.#workspacePath.asObservablePart(
		this.#generateWorkspaceEditSettingsPath,
	);

	_blockType = new UmbObjectState<BlockType | undefined>(undefined);
	public readonly blockType = this._blockType.asObservable();
	public readonly contentElementTypeKey = this._blockType.asObservablePart((x) => x?.contentElementTypeKey);
	public readonly settingsElementTypeKey = this._blockType.asObservablePart((x) => x?.settingsElementTypeKey);

	_layout = new UmbObjectState<BlockLayoutType | undefined>(undefined);
	public readonly layout = this._layout.asObservable();
	/**
	 * @obsolete Use `unique` instead. Cause we will most likely rename this in the future.
	 */
	public readonly contentUdi = this._layout.asObservablePart((x) => x?.contentUdi);
	public readonly unique = this._layout.asObservablePart((x) => x?.contentUdi);

	#content = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly content = this.#content.asObservable();
	public readonly contentTypeKey = this.#content.asObservablePart((x) => x?.contentTypeKey);

	// TODO: Make sure changes to the Block Content / Settings are reflected back to Manager.

	#settings = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly settings = this.#settings.asObservable();

	abstract readonly showContentEdit: Observable<boolean>;
	/**
	 * Set the contentUdi of this entry.
	 * @method setContentUdi
	 * @param {string} contentUdi the entry content UDI.
	 * @returns {void}
	 */
	setContentUdi(contentUdi: string) {
		this.#contentUdi = contentUdi;
		this.#observeLayout();
	}

	/**
	 * Get the current value of this Blocks label.
	 * @method getLabel
	 * @returns {string}
	 */
	getLabel() {
		return this.#label.value;
	}

	constructor(
		host: UmbControllerHost,
		blockManagerContextToken: BlockManagerContextTokenType,
		blockEntriesContextToken: BlockEntriesContextTokenType,
	) {
		super(host, 'UmbBlockEntryContext');

		// Consume block manager:
		this.consumeContext(blockManagerContextToken, (manager) => {
			this._manager = manager;
			this.#gotManager();
			this._gotManager();
		});

		// Consume block entries:
		this.consumeContext(blockEntriesContextToken, (entries) => {
			this._entries = entries;
			this.#gotEntries();
			this._gotEntries();
		});

		// Observe UDI:
		this.observe(this.unique, (contentUdi) => {
			if (!contentUdi) return;
			this.#observeData();
		});

		// Observe contentElementTypeKey:
		this.observe(this.contentTypeKey, (contentElementTypeKey) => {
			if (!contentElementTypeKey) return;
			this.#observeContentType();
			this.#observeBlockType();
		});

		// Observe blockType:
		this.observe(this.blockType, (blockType) => {
			if (!blockType) return;
			this.#observeBlockTypeLabel();
		});

		this.observe(this.index, () => {
			this.#updateCreatePath();
		});
	}

	getContentUdi() {
		return this._layout.value?.contentUdi;
	}

	#updateCreatePath() {
		const index = this.#index.value;
		if (this._entries && index !== undefined) {
			this.observe(
				this._entries.catalogueRouteBuilder,
				(catalogueRouteBuilder) => {
					if (catalogueRouteBuilder) {
						this.#createPath.setValue(this._entries!.getPathForCreateBlock(index));
					}
				},
				'observeRouteBuilderCreate',
			);
		}
	}

	#observeLayout() {
		if (!this._entries || !this.#contentUdi) return;

		this.observe(
			this._entries.layoutOf(this.#contentUdi),
			(layout) => {
				this._layout.setValue(layout);
			},
			'observeParentLayout',
		);
		this.observe(
			this.layout,
			(layout) => {
				if (layout) {
					this._entries?.setOneLayout(layout);
				}
			},
			'observeThisLayout',
		);
	}

	#gotManager() {
		this.#observeBlockType();
		this.#observeData();
	}

	abstract _gotManager(): void;

	#gotEntries() {
		this.#updateCreatePath();
		this.#observeLayout();
		if (this._entries) {
			this.observe(
				this._entries.workspacePath,
				(workspacePath) => {
					this.#workspacePath.setValue(workspacePath);
				},
				'observeWorkspacePath',
			);
		} else {
			this.removeControllerByAlias('observeWorkspacePath');
		}
	}

	abstract _gotEntries(): void;

	#observeData() {
		if (!this._manager) return;
		const contentUdi = this._layout.value?.contentUdi;
		if (!contentUdi) return;

		// observe content:
		this.observe(
			this._manager.contentOf(contentUdi),
			(content) => {
				this.#content.setValue(content);
			},
			'observeContent',
		);
		this.observe(
			this.content,
			(content) => {
				if (content) {
					this._manager?.setOneContent(content);
				}
			},
			'observeInternalContent',
		);

		// observe settings:
		const settingsUdi = this._layout.value?.settingsUdi;
		if (settingsUdi) {
			this.observe(
				this._manager.contentOf(settingsUdi),
				(content) => {
					this.#settings.setValue(content);
				},
				'observeSettings',
			);
			this.observe(
				this.settings,
				(settings) => {
					if (settings) {
						this._manager?.setOneSettings(settings);
					}
				},
				'observeInternalSettings',
			);
		}
	}

	#observeContentType() {
		if (!this._manager) return;
		const contentTypeKey = this.#content.value?.contentTypeKey;
		if (!contentTypeKey) return;

		// observe blockType:
		this.observe(
			this._manager.contentTypeOf(contentTypeKey),
			(contentType) => {
				this.#contentElementTypeAlias.setValue(contentType?.alias);
				this.#contentElementTypeName.setValue(contentType?.name);
			},
			'observeContentElementType',
		);
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
				this.contentElementTypeName,
				(contentTypeName) => {
					this.#label.setValue(contentTypeName ?? 'no name');
				},
				'observeContentTypeName',
			);
		}
	}

	// Public methods:

	//activate
	public edit() {
		window.location.href = this.#generateWorkspaceEditContentPath(this.#workspacePath.value);
	}
	public editSettings() {
		window.location.href = this.#generateWorkspaceEditSettingsPath(this.#workspacePath.value);
	}

	requestDelete() {
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (modalManager) => {
			const modalContext = modalManager.open(UMB_CONFIRM_MODAL, {
				data: {
					headline: `Delete ${this.getLabel()}`,
					content: 'Are you sure you want to delete this [INSERT BLOCK TYPE NAME]?',
					confirmLabel: 'Delete',
					color: 'danger',
				},
			});
			await modalContext.onSubmit();
			this.delete();
		});
	}
	public delete() {
		if (!this._entries) return;
		const contentUdi = this._layout.value?.contentUdi;
		if (!contentUdi) return;
		this._entries.delete(contentUdi);
	}

	//copy
}
