import type { UmbBlockManagerContext, UmbBlockWorkspaceOriginData } from '../index.js';
import type {
	UmbBlockLayoutBaseModel,
	UmbBlockDataModel,
	UmbBlockDataType,
	UmbBlockExposeModel,
	UmbBlockDataValueModel,
} from '../types.js';
import type { UmbBlockEntriesContext } from './block-entries.context.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbBooleanState,
	UmbClassState,
	UmbNumberState,
	UmbObjectState,
	UmbStringState,
	mergeObservables,
	observeMultiple,
} from '@umbraco-cms/backoffice/observable-api';
import { encodeFilePath, UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbUfmVirtualRenderController } from '@umbraco-cms/backoffice/ufm';
import { UmbRoutePathAddendumContext } from '@umbraco-cms/backoffice/router';

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
> extends UmbContextBase<unknown> {
	//
	_manager?: BlockManagerContextType;
	_entries?: BlockEntriesContextType;

	#contentKey?: string;

	#pathAddendum = new UmbRoutePathAddendumContext(this);
	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	protected readonly _variantId = this.#variantId.asObservable();

	#hasExpose = new UmbBooleanState(undefined);
	readonly hasExpose = this.#hasExpose.asObservable();

	public readonly readOnlyState = new UmbReadOnlyVariantStateManager(this);

	// Workspace alike methods, to enables editing of data without the need of a workspace (Custom views and block grid inline editing mode for example).
	getEntityType() {
		return 'block';
	}
	getUnique() {
		return this.getContentKey();
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
	//public readonly contentElementType = this.#contentElementType.asObservable();
	public readonly contentElementTypeName = this.#contentElementType.asObservablePart((x) => x?.name);
	public readonly contentElementTypeAlias = this.#contentElementType.asObservablePart((x) => x?.alias);
	public readonly contentElementTypeIcon = this.#contentElementType.asObservablePart((x) => x?.icon);

	/**
	 * Get the name of the content element type.
	 * @returns {string | undefined} - the name of the content element type.
	 */
	public getContentElementTypeName(): string | undefined {
		return this.#contentElementType.getValue()?.name;
	}

	/**
	 * Get the alias of the content element type.
	 * @returns {string | undefined} - the alias of the content element type.
	 */
	public getContentElementTypeAlias(): string | undefined {
		return this.#contentElementType.getValue()?.alias;
	}

	/**
	 * Get the icon of the content element type.
	 * @returns {string | undefined} - the icon of the content element type.
	 */
	public getContentElementTypeIcon(): string | undefined {
		return this.#contentElementType.getValue()?.icon;
	}

	_blockType = new UmbObjectState<BlockType | undefined>(undefined);
	public readonly blockType = this._blockType.asObservable();
	public readonly contentElementTypeKey = this._blockType.asObservablePart((x) => x?.contentElementTypeKey);
	public readonly settingsElementTypeKey = this._blockType.asObservablePart((x) =>
		x ? (x.settingsElementTypeKey ?? undefined) : null,
	);

	_layout = new UmbObjectState<BlockLayoutType | undefined>(undefined);
	public readonly layout = this._layout.asObservable();
	public readonly contentKey = this._layout.asObservablePart((x) => x?.contentKey);
	public readonly settingsKey = this._layout.asObservablePart((x) => (x ? (x.settingsKey ?? null) : undefined));
	public readonly unique = this._layout.asObservablePart((x) => x?.contentKey);

	/**
	 * Get the layout of the block.
	 * @returns {BlockLayoutType | undefined} - the layout of the block.
	 */
	public getLayout(): BlockLayoutType | undefined {
		return this._layout.getValue();
	}

	#label = new UmbStringState('');
	public readonly label = this.#label.asObservable();
	public getLabel() {
		return this.#label.getValue();
	}

	#labelRender = new UmbUfmVirtualRenderController(this);

	#generateWorkspaceEditContentPath = (path?: string, contentKey?: string) =>
		path && contentKey ? path + 'edit/' + encodeFilePath(contentKey) + '/view/content' : '';

	#generateWorkspaceEditSettingsPath = (path?: string, contentKey?: string) =>
		path && contentKey ? path + 'edit/' + encodeFilePath(contentKey) + '/view/settings' : '';

	#workspacePath = new UmbStringState(undefined);
	public readonly workspacePath = this.#workspacePath.asObservable();
	public readonly workspaceEditContentPath = mergeObservables(
		[this.contentKey, this.workspacePath],
		([contentKey, path]) => this.#generateWorkspaceEditContentPath(path, contentKey),
	);
	public readonly workspaceEditSettingsPath = mergeObservables(
		[this.contentKey, this.workspacePath],
		([contentKey, path]) => this.#generateWorkspaceEditSettingsPath(path, contentKey),
	);

	#contentStructure?: UmbContentTypeStructureManager;
	#contentStructurePromiseResolve?: () => void;
	#contentStructurePromise = new Promise((resolve) => {
		this.#contentStructurePromiseResolve = () => {
			resolve(undefined);
			this.#contentStructurePromiseResolve = undefined;
		};
	});

	#contentStructureHasProperties = new UmbBooleanState(undefined);
	_contentStructureHasProperties = this.#contentStructureHasProperties.asObservable();

	#settingsStructure?: UmbContentTypeStructureManager;
	#settingsStructurePromiseResolve?: () => void;
	#settingsStructurePromise = new Promise((resolve) => {
		this.#settingsStructurePromiseResolve = () => {
			resolve(undefined);
			this.#settingsStructurePromiseResolve = undefined;
		};
	});

	#createPropertyVariantId(property: UmbPropertyTypeModel, variantId: UmbVariantId) {
		return UmbVariantId.Create({
			culture: property.variesByCulture ? variantId.culture : null,
			segment: property.variesBySegment ? variantId.segment : null,
		});
	}

	async propertyVariantId(structure: UmbContentTypeStructureManager, propertyAlias: string) {
		return mergeObservables(
			[await structure.propertyStructureByAlias(propertyAlias), this._variantId],
			([property, variantId]) =>
				property && variantId ? this.#createPropertyVariantId(property, variantId) : undefined,
		);
	}

	setContentPropertyValue(propertyAlias: string, value: unknown) {
		if (!this.#contentKey) throw new Error('No content key set.');
		this._manager?.setOneContentProperty(this.#contentKey, propertyAlias, value);
	}
	setSettingsPropertyValue(propertyAlias: string, value: unknown) {
		const settingsKey = this._layout.getValue()?.settingsKey;
		if (!settingsKey) throw new Error('Settings key was not available.');
		this._manager?.setOneSettingsProperty(settingsKey, propertyAlias, value);
	}

	async contentPropertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string) {
		await this.#contentStructurePromise;
		return mergeObservables(
			[
				this.#content.asObservablePart((data) => data?.values?.filter((x) => x?.alias === propertyAlias)),
				await this.propertyVariantId(this.#contentStructure!, propertyAlias),
			],
			([propertyValues, propertyVariantId]) => {
				if (!propertyValues || !propertyVariantId) return;

				return propertyValues.find((x) => propertyVariantId.compare(x))?.value as PropertyValueType;
			},
		);
	}
	async settingsPropertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string) {
		await this.#settingsStructurePromise;
		return mergeObservables(
			[
				this.#content.asObservablePart((data) => data?.values?.filter((x) => x?.alias === propertyAlias)),
				await this.propertyVariantId(this.#settingsStructure!, propertyAlias),
			],
			([propertyValues, propertyVariantId]) => {
				if (!propertyValues || !propertyVariantId) return;

				return propertyValues.find((x) => propertyVariantId.compare(x))?.value as PropertyValueType;
			},
		);
	}

	#content = new UmbObjectState<UmbBlockDataModel | undefined>(undefined);
	protected readonly _contentValueArray = this.#content.asObservablePart((x) => x?.values);
	public readonly contentTypeKey = this.#content.asObservablePart((x) => x?.contentTypeKey);
	#contentValuesObservable?: Observable<UmbBlockDataType | undefined>;
	public async contentValues() {
		await this.#contentStructurePromise;
		if (!this.#contentValuesObservable) {
			this.#contentValuesObservable = mergeObservables(
				[this._contentValueArray, this.#contentStructure!.contentTypeProperties, this._variantId],
				this.#propertyValuesToObjectCallback,
			);
		}
		return this.#contentValuesObservable;
	}

	/**
	 * Get the content of the block.
	 * @returns {UmbBlockDataModel | undefined} - the content of the block.
	 */
	public getContent(): UmbBlockDataModel | undefined {
		return this.#content.getValue();
	}

	#settings = new UmbObjectState<UmbBlockDataModel | undefined>(undefined);
	//public readonly settings = this.#settings.asObservable();
	protected readonly _settingsValueArray = this.#settings.asObservablePart((x) => x?.values);
	private readonly settingsDataContentTypeKey = this.#settings.asObservablePart((x) =>
		x ? (x.contentTypeKey ?? undefined) : null,
	);
	#settingsValuesObservable?: Observable<UmbBlockDataType | undefined>;
	public async settingsValues() {
		await this.#settingsStructurePromise;
		if (!this.#settingsValuesObservable) {
			this.#settingsValuesObservable = mergeObservables(
				[this._settingsValueArray, this.#settingsStructure!.contentTypeProperties, this._variantId],
				this.#propertyValuesToObjectCallback,
			);
		}
		return this.#settingsValuesObservable;
	}

	#propertyValuesToObjectCallback = ([propertyValues, properties, variantId]: [
		UmbBlockDataValueModel<unknown>[] | undefined,
		UmbPropertyTypeModel[],
		UmbVariantId | undefined,
	]) => {
		if (!propertyValues || !properties || !variantId) return;

		return properties.reduce((acc, property) => {
			const propertyVariantId = this.#createPropertyVariantId(property, variantId);
			acc[property.alias] = propertyValues.find(
				(x) => x.alias === property.alias && propertyVariantId.compare(x),
			)?.value;
			return acc;
		}, {} as UmbBlockDataType);
	};

	/**
	 * Get the settings of the block.
	 * @returns {UmbBlockDataModel | undefined} - the settings of the block.
	 */
	public getSettings(): UmbBlockDataModel | undefined {
		return this.#settings.getValue();
	}

	abstract readonly showContentEdit: Observable<boolean>;

	constructor(
		host: UmbControllerHost,
		blockManagerContextToken: BlockManagerContextTokenType,
		blockEntriesContextToken: BlockEntriesContextTokenType,
	) {
		super(host, 'UmbBlockEntryContext');

		this.observe(this.label, (label) => {
			this.#labelRender.markdown = label;
		});
		this.#watchContentForLabelRender();

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

		// Observe key:
		this.observe(
			this.unique,
			(contentKey) => {
				this.#pathAddendum.setAddendum(contentKey);
				if (!contentKey) return;
				this.#observeContentData();
			},
			null,
		);

		// Observe contentElementTypeKey:
		this.observe(
			this.contentTypeKey,
			(contentElementTypeKey) => {
				if (!contentElementTypeKey) return;

				this.#getContentStructure();
				this.#observeBlockType();
			},
			null,
		);
		this.observe(
			this.settingsDataContentTypeKey,
			(settingsElementTypeKey) => {
				if (!settingsElementTypeKey) return;
				this.#getSettingsStructure(settingsElementTypeKey);
			},
			null,
		);

		// Observe blockType:
		this.observe(
			this.blockType,
			(blockType) => {
				if (!blockType) return;
				this.#observeBlockTypeLabel();
			},
			null,
		);

		// Correct settings data, accordingly to configuration of the BlockType: [NL]
		this.observe(
			observeMultiple([this.settingsElementTypeKey, this.settingsDataContentTypeKey]),
			([settingsElementTypeKey, settingsDataContentTypeKey]) => {
				// Notice the values are only undefined while we are missing the source of these observables. [NL]
				if (settingsElementTypeKey === undefined || settingsDataContentTypeKey === undefined) return;
				// Is there a difference between configuration and actual data key:
				if (settingsElementTypeKey !== settingsDataContentTypeKey) {
					// We need to update our key for the settings data [NL]
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
			null,
		);

		this.observe(
			observeMultiple([this.settingsKey, this.blockType]),
			([settingsKey, blockType]) => {
				if (!this.#contentKey || settingsKey === undefined || !blockType) return;
				if (settingsKey == null && blockType.settingsElementTypeKey) {
					// We have a settings ElementType in config but not in data, so lets create the scaffold for that: [NL]
					const settingsData = this._manager!.createBlockSettingsData(blockType.contentElementTypeKey); // Yes its on purpose we use the contentElementTypeKey here, as this is our identifier for a BlockType. [NL]
					this._manager?.setOneSettings(settingsData);
					this._layout.update({ settingsKey: settingsData.key } as Partial<BlockLayoutType>);
				} else if (settingsKey && blockType.settingsElementTypeKey === undefined) {
					// We no longer have settings ElementType in config. So we remove the settingsData and settings key from the layout. [NL]
					this._manager?.removeOneSettings(settingsKey);
					this._layout.update({ settingsKey: undefined } as Partial<BlockLayoutType>);
				}
			},
			null,
		);
	}

	async #watchContentForLabelRender() {
		this.observe(await this.contentValues(), (content) => {
			this.#labelRender.value = content;
		});
	}

	getContentKey() {
		return this._layout.value?.contentKey;
	}

	/**
	 * Set the contentKey of this entry.
	 * @function setContentKey
	 * @param {string} contentKey the entry content key.
	 * @returns {void}
	 */
	setContentKey(contentKey: string) {
		this.#contentKey = contentKey;
		this.#observeLayout();
	}

	/**
	 * Get the current value of this Blocks label.
	 * @function getName
	 * @returns {string} - the value of the label.
	 */
	getName() {
		return this.#labelRender.toString();
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
		if (!this._entries || !this.#contentKey) return;

		this.observe(
			this._entries.layoutOf(this.#contentKey),
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
		this.#observeVariantId();
		this.#observeBlockType();
		this.#observeContentData();
		this.#observeSettingsData();
		this.#observeReadOnlyState();
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

	#observeContentData() {
		if (!this._manager || !this.#contentKey) return;

		// observe content:
		this.observe(
			this._manager.contentOf(this.#contentKey),
			(content) => {
				this.#content.setValue(content);
			},
			'observeContent',
		);
	}
	#observeSettingsData() {
		// observe settings:
		this.observe(
			this._manager ? this.settingsKey : undefined,
			(settingsKey) => {
				if (settingsKey) {
					this.observe(
						this._manager?.settingsOf(settingsKey),
						(settings) => {
							this.#settings.setValue(settings);
						},
						'observeSettings',
					);
				}
			},
			'observeSettingsKey',
		);
	}

	abstract _gotContentType(contentType: UmbContentTypeModel | undefined): void;

	async #observeVariantId() {
		if (!this._manager) return;
		await this.#contentStructurePromise;
		if (!this.#contentStructure) {
			throw new Error('No contentStructure found');
		}

		// observe blockType:
		this.observe(
			observeMultiple([
				this._manager.variantId,
				this.#contentStructure?.ownerContentTypeObservablePart((x) => x?.variesByCulture),
				this.#contentStructure?.ownerContentTypeObservablePart((x) => x?.variesBySegment),
			]),
			([variantId, variesByCulture, variesBySegment]) => {
				if (!variantId || variesByCulture === undefined || variesBySegment === undefined) return;
				this.#variantId.setValue(variantId.toVariant(variesByCulture, variesBySegment));
				this.#gotVariantId();
			},
			'observeBlockType',
		);
	}

	#observeReadOnlyState() {
		if (!this._manager) return;

		this.observe(
			observeMultiple([this._manager.readOnlyState.isReadOnly, this._manager.variantId]),
			([isReadOnly, variantId]) => {
				const unique = 'UMB_BLOCK_MANAGER_CONTEXT';
				if (variantId === undefined) return;

				if (isReadOnly) {
					const state = {
						unique,
						variantId,
						message: '',
					};

					this.readOnlyState?.addState(state);
				} else {
					this.readOnlyState?.removeState(unique);
				}
			},
			'observeIsReadOnly',
		);
	}

	#getContentStructure() {
		if (!this._manager) return;

		const contentTypeKey = this.#content.getValue()?.contentTypeKey;
		if (!contentTypeKey) return;

		// observe blockType:
		this.#contentStructure = this._manager.getStructure(contentTypeKey);
		this.#contentStructurePromiseResolve?.();

		this.observe(
			this.#contentStructure?.ownerContentType,
			(contentType) => {
				/**
				 * currently only using:
				 * Name, Alias, Icon
				 */
				this.#contentElementType.setValue(contentType);
				this._gotContentType(contentType);
			},
			'observeContentElementType',
		);

		this.observe(
			this.#contentStructure?.contentTypeHasProperties,
			(has) => {
				this.#contentStructureHasProperties.setValue(has);
			},
			'observeContentTypeHasProperties',
		);
	}

	#getSettingsStructure(contentTypeKey: string | undefined) {
		if (!this._manager || !contentTypeKey) return;

		// observe blockType:
		this.#settingsStructure = this._manager.getStructure(contentTypeKey);
		this.#settingsStructurePromiseResolve?.();
	}

	#observeBlockType() {
		if (!this._manager) return;
		const contentTypeKey = this.#content.getValue()?.contentTypeKey;
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
		const blockType = this._blockType.getValue();
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

	#gotVariantId() {
		const variantId = this.#variantId.getValue();
		if (!variantId || !this.#contentKey) return;
		this.observe(
			this._manager?.hasExposeOf(this.#contentKey, variantId),
			(hasExpose) => {
				this.#hasExpose.setValue(hasExpose);
			},
			'observeExpose',
		);
	}

	// Public methods:

	//activate
	public edit() {
		window.history.pushState(
			{},
			'',
			this.#generateWorkspaceEditContentPath(this.#workspacePath.value, this.getContentKey()),
		);
	}
	public editSettings() {
		window.history.pushState(
			{},
			'',
			this.#generateWorkspaceEditSettingsPath(this.#workspacePath.value, this.getContentKey()),
		);
	}

	async requestDelete() {
		const blockName = this.getName();
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
		const contentKey = this._layout.value?.contentKey;
		if (!contentKey) return;
		this._entries.delete(contentKey);
	}

	public expose() {
		const variantId = this.#variantId.getValue();
		if (!variantId || !this.#contentKey) return;
		this._manager?.setOneExpose(this.#contentKey, variantId);
	}

	/**
	 * Get the expose of the block.
	 * @returns {UmbBlockExposeModel | undefined} - the expose of the block.
	 */
	public getExpose(): UmbBlockExposeModel | undefined {
		const exposes = this._manager?.getExposes();
		return exposes?.find((x) => x.contentKey === this.#contentKey);
	}
}
