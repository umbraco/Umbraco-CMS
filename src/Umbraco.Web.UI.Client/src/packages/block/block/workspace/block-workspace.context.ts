import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '../types.js';
import { UmbBlockElementManager } from './block-element-manager.js';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block';
import { buildUdi } from '@umbraco-cms/backoffice/utils';
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
	#modalContext?: typeof UMB_MODAL_CONTEXT.TYPE;
	#retrieveModalContext;
	#editorConfigPromise?: Promise<unknown>;

	#entityType: string;

	#isNew = new UmbBooleanState<boolean | undefined>(undefined);
	readonly isNew = this.#isNew.asObservable();

	#liveEditingMode?: boolean;

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
			this.#editorConfigPromise = this.observe(context.editorConfiguration, (editorConfigs) => {
				if (editorConfigs) {
					const value = editorConfigs.getValueByAlias<boolean>('useLiveEditing');
					this.#liveEditingMode = value;
				}
			}).asPromise();
			// TODO: Observe or just get the LiveEditing setting?
		}).asPromise();
	}

	async load(unique: string) {
		await this.#retrieveBlockManager;
		await this.#editorConfigPromise;
		if (!this.#blockManager) {
			throw new Error('Block manager not found');
			return;
		}

		this.observe(
			// TODO: Make a general concept of Block Entries Context, use it to retrieve the layout:
			this.#blockManager.layoutOf(unique),
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
				}
			},
			'observeLayout',
		);

		if (this.#liveEditingMode) {
			this.#establishLiveSync();
		}
	}

	async create(contentElementTypeId: string) {
		await this.#retrieveBlockManager;
		await this.#retrieveModalContext;
		if (!this.#blockManager) {
			throw new Error('Block Manager not found');
			return;
		}
		if (!this.#modalContext) {
			throw new Error('Modal Context not found');
			return;
		}
		//
		// TODO: Condense this into some kind of create method?
		const key = UmbId.new();
		const contentUdi = buildUdi('block', key);
		const layoutData: UmbBlockLayoutBaseModel = {
			contentUdi: contentUdi,
		};
		const contentData: UmbBlockDataType = {
			udi: contentUdi,
			contentTypeKey: contentElementTypeId,
		};
		this.content.setData(contentData);

		// TODO: If we have Settings dedicated to this block type, we initiate them here:

		this.setIsNew(true);
		this.#layout.setValue(layoutData as LayoutDataType);

		if (this.#liveEditingMode) {
			const blockCreated = this.#blockManager.create(
				this.#modalContext.data as UmbBlockWorkspaceData,
				layoutData,
				contentElementTypeId,
			);
			if (!blockCreated) {
				throw new Error('Block Manager could not create block');
				return;
			}

			this.#establishLiveSync();
		}
	}

	#establishLiveSync() {
		this.observe(this.layout, (layoutData) => {
			if (layoutData) {
				this.#blockManager?.setOneLayout(layoutData);
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
		if (!layoutData || !this.#blockManager || !contentData || !this.#modalContext) return;

		if (!this.#liveEditingMode) {
			if (this.getIsNew() === true) {
				const blockCreated = this.#blockManager.create(
					this.#modalContext.data as UmbBlockWorkspaceData,
					layoutData,
					contentData.contentTypeKey,
				);
				if (!blockCreated) {
					throw new Error('Block Manager could not create block');
					return;
				}
			}

			// TODO: Save the block, but only in non-live-editing mode.
			this.#blockManager.setOneLayout(layoutData);

			if (contentData) {
				this.#blockManager.setOneContent(contentData);
			}
			const settingsData = this.settings.getData();
			if (settingsData) {
				this.#blockManager.setOneSettings(settingsData);
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
					this.#blockManager?.deleteBlock(contentUdi);
				}
			}
		}
	};

	public destroy(): void {
		this.#layout.destroy();
	}
}

export default UmbBlockWorkspaceContext;
