import type { UmbBlockDataType, UmbBlockLayoutBaseModel } from '../types.js';
import { UmbBlockElementManager } from './block-element-manager.js';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_BLOCK_ENTRIES_CONTEXT,
	UMB_BLOCK_MANAGER_CONTEXT,
	type UmbBlockWorkspaceData,
} from '@umbraco-cms/backoffice/block';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';

export type UmbBlockWorkspaceElementManagerNames = 'content' | 'settings';
export class UmbBlockWorkspaceContext<
	LayoutDataType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbEditableWorkspaceContextBase<LayoutDataType> {
	// Just for context token safety:
	public readonly IS_BLOCK_WORKSPACE_CONTEXT = true;
	//
	readonly workspaceAlias;

	#blockManager?: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE;
	#retrieveBlockManager;
	#blockEntries?: typeof UMB_BLOCK_ENTRIES_CONTEXT.TYPE;
	#retrieveBlockEntries;
	#modalContext?: typeof UMB_MODAL_CONTEXT.TYPE;
	#retrieveModalContext;
	#editorConfigPromise?: Promise<unknown>;

	#entityType: string;

	#isNew = new UmbBooleanState<boolean | undefined>(undefined);
	readonly isNew = this.#isNew.asObservable();

	#liveEditingMode?: boolean;

	#initialLayout?: LayoutDataType;
	#initialContent?: UmbBlockDataType;
	#initialSettings?: UmbBlockDataType;

	#layout = new UmbObjectState<LayoutDataType | undefined>(undefined);
	readonly layout = this.#layout.asObservable();
	//readonly unique = this.#layout.asObservablePart((x) => x?.contentUdi);
	readonly contentUdi = this.#layout.asObservablePart((x) => x?.contentUdi);

	readonly content = new UmbBlockElementManager(this);

	readonly settings = new UmbBlockElementManager(this);

	// TODO: Get the name of the contentElementType..
	#label = new UmbStringState<string | undefined>(undefined);
	readonly name = this.#label.asObservable();

	constructor(host: UmbControllerHost, workspaceArgs: { manifest: ManifestWorkspace }) {
		super(host, workspaceArgs.manifest.alias);
		this.#entityType = workspaceArgs.manifest.meta?.entityType;
		this.workspaceAlias = workspaceArgs.manifest.alias;

		this.#retrieveModalContext = this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this.#modalContext = context;
			context.onSubmit().catch(this.#modalRejected);
		}).asPromise();

		this.#retrieveBlockManager = this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (context) => {
			this.#blockManager = context;
			this.#editorConfigPromise = this.observe(
				context.editorConfiguration,
				(editorConfigs) => {
					if (editorConfigs) {
						const value = editorConfigs.getValueByAlias<boolean>('useLiveEditing');
						this.#liveEditingMode = value;
					}
				},
				'observeEditorConfig',
			).asPromise();
		}).asPromise();

		this.#retrieveBlockEntries = this.consumeContext(UMB_BLOCK_ENTRIES_CONTEXT, (context) => {
			this.#blockEntries = context;
		}).asPromise();
	}

	async load(unique: string) {
		await this.#retrieveBlockManager;
		await this.#retrieveBlockEntries;
		await this.#editorConfigPromise;
		if (!this.#blockManager || !this.#blockEntries) {
			throw new Error('Block manager not found');
			return;
		}

		this.observe(
			this.#blockEntries.layoutOf(unique),
			(layoutData) => {
				this.#initialLayout ??= layoutData as LayoutDataType;
				this.removeControllerByAlias('observeLayoutInitially');
			},
			'observeLayoutInitially',
		);

		this.observe(
			this.#blockEntries.layoutOf(unique),
			(layoutData) => {
				this.#layout.setValue(layoutData as LayoutDataType);

				// Content:
				const contentUdi = layoutData?.contentUdi;
				if (contentUdi) {
					this.observe(
						this.#blockManager!.contentOf(contentUdi),
						(contentData) => {
							this.content.setData(contentData);
						},
						'observeContent',
					);
					if (!this.#initialContent) {
						this.observe(
							this.#blockManager!.contentOf(contentUdi),
							(contentData) => {
								this.#initialContent ??= contentData;
								this.removeControllerByAlias('observeContentInitially');
							},
							'observeContentInitially',
						);
					}
				}

				// Settings:
				const settingsUdi = layoutData?.settingsUdi;
				if (settingsUdi) {
					this.observe(
						this.#blockManager!.settingsOf(settingsUdi),
						(settingsData) => {
							this.settings.setData(settingsData);
						},
						'observeSettings',
					);
					if (!this.#initialSettings) {
						this.observe(
							this.#blockManager!.contentOf(settingsUdi),
							(settingsData) => {
								this.#initialSettings ??= settingsData;
								this.removeControllerByAlias('observeSettingsInitially');
							},
							'observeSettingsInitially',
						);
					}
				}
			},
			'observeLayout',
		);

		if (this.#liveEditingMode) {
			this.#establishLiveSync();
		}
	}

	async create(contentElementTypeId: string) {
		await this.#retrieveBlockEntries;
		await this.#retrieveModalContext;
		if (!this.#blockEntries) {
			throw new Error('Block Entries not found');
			return;
		}
		if (!this.#modalContext) {
			throw new Error('Modal Context not found');
			return;
		}

		// TODO: Missing some way to append more layout data... this could be part of modal data, (or context api?)

		this.setIsNew(true);

		const blockCreated = await this.#blockEntries.create(
			contentElementTypeId,
			{},
			this.#modalContext.data as UmbBlockWorkspaceData,
		);
		if (!blockCreated) {
			throw new Error('Block Entries could not create block');
		}

		this.#layout.setValue(blockCreated.layout as LayoutDataType);
		this.content.setData(blockCreated.content);
		if (blockCreated.settings) {
			this.settings.setData(blockCreated.settings);
		}

		if (this.#liveEditingMode) {
			// Insert already, cause we are in live editing mode:
			const blockInserted = await this.#blockEntries.insert(
				blockCreated.layout,
				blockCreated.content,
				blockCreated.settings,
				this.#modalContext.data as UmbBlockWorkspaceData,
			);
			if (!blockInserted) {
				throw new Error('Block Entries could not insert block');
			}
			this.#establishLiveSync();
		}
	}

	#establishLiveSync() {
		this.observe(this.layout, (layoutData) => {
			if (layoutData) {
				this.#blockEntries?.setOneLayout(layoutData);
			}
		});
		this.observe(this.content.data, (contentData) => {
			if (contentData) {
				this.#blockManager?.setOneContent(contentData);
			}
		});
		this.observe(this.settings.data, (settingsData) => {
			if (settingsData) {
				this.#blockManager?.setOneSettings(settingsData);
			}
		});
	}

	getIsNew() {
		return this.#isNew.value;
	}
	setIsNew(value: boolean): void {
		this.#isNew.setValue(value);
	}

	getData() {
		return this.#layout.getValue();
	}

	getEntityId() {
		return this.getData()!.contentUdi;
	}

	getEntityType() {
		return this.#entityType;
	}

	getName() {
		return 'block name content element type here...';
	}

	// NOTICE currently the property methods are for layout, but this could be seen as wrong, we might need to dedicate a data manager for the layout as well.

	async propertyValueByAlias<propertyAliasType extends keyof LayoutDataType>(propertyAlias: propertyAliasType) {
		return this.#layout.asObservablePart(
			(layout) => layout?.[propertyAlias as keyof LayoutDataType] as LayoutDataType[propertyAliasType],
		);
	}

	getPropertyValue<propertyAliasType extends keyof LayoutDataType>(propertyAlias: propertyAliasType) {
		// TODO: Should be using Content, then we need a toggle or another method for getting settings.
		return this.#layout.getValue()?.[propertyAlias as keyof LayoutDataType] as LayoutDataType[propertyAliasType];
	}

	async setPropertyValue(alias: string, value: unknown) {
		const currentData = this.#layout.value;
		if (currentData) {
			this.#layout.update({ ...currentData, [alias]: value });
		}
	}

	async save() {
		const layoutData = this.#layout.value;
		const contentData = this.content.getData();
		if (!layoutData || !this.#blockManager || !this.#blockEntries || !contentData || !this.#modalContext) return;

		const settingsData = this.settings.getData();

		if (!this.#liveEditingMode) {
			if (this.getIsNew() === true) {
				// Insert (This means the layout entry will be inserted at the desired location):
				const blockInserted = await this.#blockEntries.insert(
					layoutData,
					contentData,
					settingsData,
					this.#modalContext.data as UmbBlockWorkspaceData,
				);
				if (!blockInserted) {
					throw new Error('Block Entries could not insert block');
				}
			} else {
				// Update data:

				this.#blockEntries.setOneLayout(layoutData);
				if (contentData) {
					this.#blockManager.setOneContent(contentData);
				}
				if (settingsData) {
					this.#blockManager.setOneSettings(settingsData);
				}
			}
		}

		this.saveComplete(layoutData);
	}

	#modalRejected = () => {
		if (this.#liveEditingMode) {
			// Revert
			// Did it exist before?
			if (this.getIsNew() === true) {
				// Remove the block?
				const contentUdi = this.#layout.value?.contentUdi;
				if (contentUdi) {
					this.#blockEntries?.delete(contentUdi);
				}
			} else {
				// TODO: Revert the layout, content & settings data to the original state.
				if (this.#initialLayout) {
					this.#blockEntries?.setOneLayout(this.#initialLayout);
				}
				if (this.#initialContent) {
					this.#blockManager?.setOneContent(this.#initialContent);
				}
				if (this.#initialSettings) {
					this.#blockManager?.setOneContent(this.#initialSettings);
				}
			}
		}
	};

	public destroy(): void {
		super.destroy();
		this.#layout.destroy();
	}
}

export default UmbBlockWorkspaceContext;
