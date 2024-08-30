import type { UmbBlockManagerContext, UmbBlockWorkspaceOriginData } from '../index.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '../types.js';
import type { UmbBlockEntriesContext } from './block-entries.context.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbNumberState,
	UmbObjectState,
	UmbStringState,
	mergeObservables,
	observeMultiple,
} from '@umbraco-cms/backoffice/observable-api';
import { encodeFilePath } from '@umbraco-cms/backoffice/utils';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbBlockEntryContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType>,
	BlockEntriesContextTokenType extends UmbContextToken<BlockEntriesContextType>,
	BlockEntriesContextType extends UmbBlockEntriesContext<
		BlockManagerContextTokenType,
		BlockManagerContextType,
		BlockType,
		BlockLayoutType,
		BlockOriginData
	>,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	BlockOriginData extends UmbBlockWorkspaceOriginData = UmbBlockWorkspaceOriginData,
> extends UmbContextBase<any> {
	//
	_manager?: BlockManagerContextType;
	_entries?: BlockEntriesContextType;

	#contentUdi?: string;

	// Workspace alike methods, to enables editing of data without the need of a workspace (Custom views and block grid inline editing mode for example).
	getEntityType() {
		return 'block';
	}
	getUnique() {
		return this.getContentUdi();
	}

	setContentPropertyValue(propertyAlias: string, value: unknown) {
		if (!this.#contentUdi) throw new Error('No content UDI set.');
		this._manager?.setOneContentProperty(this.#contentUdi, propertyAlias, value);
	}
	setSettingsPropertyValue(propertyAlias: string, value: unknown) {
		const settingsUdi = this._layout.getValue()?.settingsUdi;
		if (!settingsUdi) throw new Error('Settings UDI was not available.');
		this._manager?.setOneSettingsProperty(settingsUdi, propertyAlias, value);
	}

	contentPropertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#content.asObservablePart((x) => x?.[propertyAlias] as ReturnType | undefined);
	}
	settingsPropertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#settings.asObservablePart((x) => x?.[propertyAlias] as ReturnType | undefined);
	}

	#index = new UmbNumberState(undefined);
	readonly index = this.#index.asObservable();
	getIndex() {
		return this.#index.value;
	}
	setIndex(index: number | undefined) {
		this.#index.setValue(index);
	}

	#createBeforePath = new UmbStringState(undefined);
	readonly createBeforePath = this.#createBeforePath.asObservable();
	#createAfterPath = new UmbStringState(undefined);
	readonly createAfterPath = this.#createAfterPath.asObservable();

	#contentElementType = new UmbObjectState<UmbContentTypeModel | undefined>(undefined);
	public readonly contentElementType = this.#contentElementType.asObservable();
	public readonly contentElementTypeName = this.#contentElementType.asObservablePart((x) => x?.name);
	public readonly contentElementTypeAlias = this.#contentElementType.asObservablePart((x) => x?.alias);
	public readonly contentElementTypeIcon = this.#contentElementType.asObservablePart((x) => x?.icon);

	_blockType = new UmbObjectState<BlockType | undefined>(undefined);
	public readonly blockType = this._blockType.asObservable();
	public readonly contentElementTypeKey = this._blockType.asObservablePart((x) => x?.contentElementTypeKey);
	public readonly settingsElementTypeKey = this._blockType.asObservablePart((x) =>
		x ? (x.settingsElementTypeKey ?? undefined) : null,
	);

	_layout = new UmbObjectState<BlockLayoutType | undefined>(undefined);
	public readonly layout = this._layout.asObservable();
	/**
	 * @obsolete Use `unique` instead. Cause we will most likely rename this in the future.
	 */
	public readonly contentUdi = this._layout.asObservablePart((x) => x?.contentUdi);
	public readonly settingsUdi = this._layout.asObservablePart((x) => x?.settingsUdi);
	public readonly unique = this._layout.asObservablePart((x) => x?.contentUdi);

	#label = new UmbStringState('');
	public readonly label = this.#label.asObservable();

	#generateWorkspaceEditContentPath = (path?: string, contentUdi?: string) =>
		path && contentUdi ? path + 'edit/' + encodeFilePath(contentUdi) + '/view/content' : '';

	#generateWorkspaceEditSettingsPath = (path?: string, contentUdi?: string) =>
		path && contentUdi ? path + 'edit/' + encodeFilePath(contentUdi) + '/view/settings' : '';

	#workspacePath = new UmbStringState(undefined);
	public readonly workspacePath = this.#workspacePath.asObservable();
	public readonly workspaceEditContentPath = mergeObservables(
		[this.contentUdi, this.workspacePath],
		([contentUdi, path]) => this.#generateWorkspaceEditContentPath(path, contentUdi),
	);
	public readonly workspaceEditSettingsPath = mergeObservables(
		[this.contentUdi, this.workspacePath],
		([contentUdi, path]) => this.#generateWorkspaceEditSettingsPath(path, contentUdi),
	);

	#content = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly content = this.#content.asObservable();
	public readonly contentTypeKey = this.#content.asObservablePart((x) => x?.contentTypeKey);

	// TODO: Make sure changes to the Block Content / Settings are reflected back to Manager.

	#settings = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly settings = this.#settings.asObservable();
	private readonly settingsDataContentTypeKey = this.#settings.asObservablePart((x) =>
		x ? (x.contentTypeKey ?? undefined) : null,
	);

	abstract readonly showContentEdit: Observable<boolean>;

	/**
	 * Set the contentUdi of this entry.
	 * @function setContentUdi
	 * @param {string} contentUdi the entry content UDI.
	 * @returns {void}
	 */
	setContentUdi(contentUdi: string) {
		this.#contentUdi = contentUdi;
		this.#observeLayout();
	}

	/**
	 * Get the current value of this Blocks label.
	 * @function getLabel
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
			this._gotManager();
			this.#gotManager();
		});

		// Consume block entries:
		this.consumeContext(blockEntriesContextToken, (entries) => {
			this._entries = entries;
			this._gotEntries();
			this.#gotEntries();
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

		// Correct settings data, accordingly to configuration of the BlockType: [NL]
		this.observe(
			observeMultiple([this.settingsElementTypeKey, this.settingsDataContentTypeKey]),
			([settingsElementTypeKey, settingsDataContentTypeKey]) => {
				// Notice the values are only undefined while we are missing the source of these observables. [NL]
				if (settingsElementTypeKey === undefined || settingsDataContentTypeKey === undefined) return;
				// Is there a difference between configuration and actual data udi:
				if (settingsElementTypeKey !== settingsDataContentTypeKey) {
					// We need to update our udi for the settings data [NL]
					if (settingsElementTypeKey != null) {
						// Update the settings model with latest elementTypeKey, so data is up to date with configuration: [NL]
						const currentSettings = this.#settings.getValue();
						if (currentSettings) {
							this._manager?.setOneSettings({ ...currentSettings, contentTypeKey: settingsElementTypeKey });
						}
					}
					// We do not need to remove the settings if configuration is gone, cause another observation handles this. [NL]
				}
			},
		);

		this.observe(observeMultiple([this.layout, this.blockType]), ([layout, blockType]) => {
			if (!this.#contentUdi || !layout || !blockType) return;
			if (layout.settingsUdi == null && blockType.settingsElementTypeKey) {
				// We have a settings ElementType in config but not in data, so lets create the scaffold for that: [NL]
				const settingsData = this._manager!.createBlockSettingsData(blockType.contentElementTypeKey); // Yes its on purpose we use the contentElementTypeKey here, as this is our identifier for a BlockType. [NL]
				this._manager?.setOneSettings(settingsData);
				this._layout.update({ settingsUdi: settingsData.udi } as Partial<BlockLayoutType>);
			} else if (layout.settingsUdi && blockType.settingsElementTypeKey === undefined) {
				// We no longer have settings ElementType in config. So we remove the settingsData and settings UDI from the layout. [NL]
				this._manager?.removeOneSettings(layout.settingsUdi);
				this._layout.update({ settingsUdi: undefined } as Partial<BlockLayoutType>);
			}
		});
	}

	getContentUdi() {
		return this._layout.value?.contentUdi;
	}

	#updateCreatePaths() {
		if (this._entries) {
			this.observe(
				observeMultiple([this.index, this._entries.catalogueRouteBuilder, this._entries.canCreate]),
				([index, catalogueRouteBuilder, canCreate]) => {
					if (index === undefined) return;
					if (catalogueRouteBuilder && canCreate) {
						this.#createBeforePath.setValue(this._entries!.getPathForCreateBlock(index));
						this.#createAfterPath.setValue(this._entries!.getPathForCreateBlock(index + 1));
					} else {
						this.#createBeforePath.setValue(undefined);
						this.#createAfterPath.setValue(undefined);
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
		this.#updateCreatePaths();
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
			this.removeUmbControllerByAlias('observeWorkspacePath');
		}
	}

	abstract _gotEntries(): void;

	#observeData() {
		if (!this._manager || !this.#contentUdi) return;

		// observe content:
		this.observe(
			this._manager.contentOf(this.#contentUdi),
			(content) => {
				this.#content.setValue(content);
			},
			'observeContent',
		);
		/*

		This two way binding does not work well, might need to further investigate what the exact problem is.
		this.observe(
			this.content,
			(content) => {
				if (content) {
					this._manager?.setOneContent(content);
				}
			},
			'observeInternalContent',
		);
		*/

		// observe settings:
		const settingsUdi = this._layout.value?.settingsUdi;
		if (settingsUdi) {
			this.observe(
				this._manager.settingsOf(settingsUdi),
				(settings) => {
					this.#settings.setValue(settings);
				},
				'observeSettings',
			);
			/*

			This two way binding does not work well, might need to further investigate what the exact problem is.
			this.observe(
				this.settings,
				(settings) => {
					if (settings) {
						this._manager?.setOneSettings(settings);
					}
				},
				'observeInternalSettings',
			);*/
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
				this.#contentElementType.setValue(contentType);
				this._gotContentType(contentType);
			},
			'observeContentElementType',
		);
	}

	abstract _gotContentType(contentType: UmbContentTypeModel | undefined): void;

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
			this.removeUmbControllerByAlias('observeContentTypeName');
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
		window.location.href = this.#generateWorkspaceEditContentPath(this.#workspacePath.value, this.getContentUdi());
	}
	public editSettings() {
		window.location.href = this.#generateWorkspaceEditSettingsPath(this.#workspacePath.value, this.getContentUdi());
	}

	async requestDelete() {
		const blockName = this.getLabel();
		// TODO: Localizations missing [NL]
		await umbConfirmModal(this, {
			headline: `Delete ${blockName}`,
			content: `Are you sure you want to delete this ${blockName}?`,
			confirmLabel: 'Delete',
			color: 'danger',
		});
		this.delete();
	}

	public delete() {
		if (!this._entries) return;
		const contentUdi = this._layout.value?.contentUdi;
		if (!contentUdi) return;
		this._entries.delete(contentUdi);
	}

	//copy
}
