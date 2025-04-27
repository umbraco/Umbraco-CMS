import type { UmbBlockDataModel, UmbBlockLayoutBaseModel } from '../types.js';
import { UmbBlockWorkspaceEditorElement } from './block-workspace-editor.element.js';
import { UmbBlockElementManager } from './block-element-manager.js';
import {
	UmbSubmittableWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	type ManifestWorkspace,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbBooleanState,
	UmbClassState,
	UmbObjectState,
	UmbStringState,
	observeMultiple,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DISCARD_CHANGES_MODAL, UMB_MODAL_CONTEXT, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { decodeFilePath, UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import {
	UMB_BLOCK_ENTRIES_CONTEXT,
	UMB_BLOCK_MANAGER_CONTEXT,
	type UmbBlockWorkspaceOriginData,
	UMB_BLOCK_ENTRY_CONTEXT,
} from '@umbraco-cms/backoffice/block';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

export type UmbBlockWorkspaceElementManagerNames = 'content' | 'settings';
export class UmbBlockWorkspaceContext<LayoutDataType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel>
	extends UmbSubmittableWorkspaceContextBase<LayoutDataType>
	implements UmbRoutableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_BLOCK_WORKSPACE_CONTEXT = true;
	//

	#blockManager?: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE;
	#retrieveBlockManager;
	#blockEntries?: typeof UMB_BLOCK_ENTRIES_CONTEXT.TYPE;
	#retrieveBlockEntries;
	#originData?: UmbBlockWorkspaceOriginData;
	// Set the origin data for this workspace. Example used by inline editing which setups the workspace context it self.
	setOriginData(data: UmbBlockWorkspaceOriginData) {
		this.#originData = data;
	}
	#modalContext?: typeof UMB_MODAL_CONTEXT.TYPE;
	#retrieveModalContext;

	#entityType: string;

	#liveEditingMode?: boolean;

	#initialLayout?: LayoutDataType;
	#initialContent?: UmbBlockDataModel;
	#initialSettings?: UmbBlockDataModel;

	#layout = new UmbObjectState<LayoutDataType | undefined>(undefined);
	readonly layout = this.#layout.asObservable();
	readonly unique = this.#layout.asObservablePart((x) => x?.contentKey);
	readonly contentKey = this.#layout.asObservablePart((x) => x?.contentKey);

	readonly content = new UmbBlockElementManager(this, 'contentData');

	readonly settings = new UmbBlockElementManager(this, 'settingsData');

	#name = new UmbStringState<string | undefined>(undefined);
	readonly name = this.#name.asObservable();

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	readonly variantId = this.#variantId.asObservable();

	#exposed = new UmbBooleanState<undefined>(undefined);
	readonly exposed = this.#exposed.asObservable();

	public readonly readOnlyState = new UmbReadOnlyVariantStateManager(this);

	constructor(host: UmbControllerHost, workspaceArgs: { manifest: ManifestWorkspace }) {
		super(host, workspaceArgs.manifest.alias);
		const manifest = workspaceArgs.manifest;
		this.#entityType = manifest.meta?.entityType;

		window.addEventListener('willchangestate', this.#onWillNavigate);

		this.addValidationContext(this.content.validation);
		this.addValidationContext(this.settings.validation);

		this.#retrieveModalContext = this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this.#modalContext = context;
			this.#originData = context?.data.originData;
			context.onSubmit().catch(this.#modalRejected);
		}).asPromise();

		this.#retrieveBlockManager = this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
			this.#blockManager = manager;
			this.#gotManager();
		});

		this.#retrieveBlockEntries = this.consumeContext(UMB_BLOCK_ENTRIES_CONTEXT, (context) => {
			this.#blockEntries = context;
		}).asPromise();

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.#name.setValue(context.getName());
		});

		this.observe(this.variantId, (variantId) => {
			this.content.setVariantId(variantId);
			this.settings.setVariantId(variantId);
		});

		this.routes.setRoutes([
			{
				path: 'create/:elementTypeKey',
				component: UmbBlockWorkspaceEditorElement,
				setup: async (component, info) => {
					const elementTypeKey = info.match.params.elementTypeKey;
					await this.create(elementTypeKey);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:key',
				component: UmbBlockWorkspaceEditorElement,
				setup: (component, info) => {
					const key = decodeFilePath(info.match.params.key);
					this.load(key);
				},
			},
		]);
	}

	#gotManager() {
		if (!this.#blockManager) return;
		const manager = this.#blockManager;

		this.observe(
			manager.liveEditingMode,
			(liveEditingMode) => {
				this.#liveEditingMode = liveEditingMode ?? false;
			},
			'observeLiveEditingMode',
		);

		this.observe(
			observeMultiple([
				manager.variantId,
				this.content.structure.variesByCulture,
				this.content.structure.variesBySegment,
			]),
			([variantId, variesByCulture, variesBySegment]) => {
				if (!variantId || variesByCulture === undefined || variesBySegment === undefined) return;

				if (!variesBySegment && !variesByCulture) {
					variantId = UmbVariantId.CreateInvariant();
				} else if (!variesBySegment) {
					variantId = variantId.toSegmentInvariant();
				} else if (!variesByCulture) {
					variantId = variantId.toCultureInvariant();
				}

				this.#variantId.setValue(variantId);
			},
			'observeVariantIds',
		);

		this.removeUmbControllerByAlias('observeHasExpose');
		this.observe(
			observeMultiple([this.contentKey, this.variantId]),
			([contentKey, variantId]) => {
				if (!contentKey || !variantId) return;

				this.observe(
					manager.hasExposeOf(contentKey, variantId),
					(exposed) => {
						this.#exposed.setValue(exposed);
					},
					'observeHasExpose',
				);
			},
			'observeContentKeyAndVariantId',
		);

		this.observe(
			observeMultiple([manager.readOnlyState.isReadOnly, this.variantId]),
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

		this.observe(
			this.content.contentTypeId,
			(contentTypeId) => {
				this.observe(
					contentTypeId ? manager.blockTypeOf(contentTypeId) : undefined,
					(blockType) => {
						if (blockType?.editorSize) {
							this.setEditorSize(blockType.editorSize);
						}
					},
					'observeBlockType',
				);
			},
			'observeContentTypeId',
		);
	}

	#allowNavigateAway = false;
	#onWillNavigate = async (e: CustomEvent) => {
		const newUrl = e.detail.url;

		if (this.#allowNavigateAway) {
			return true;
		}

		if (this._checkWillNavigateAway(newUrl) && this.getHasUnpersistedChanges()) {
			/* Since ours modals are async while events are synchronous, we need to prevent the default behavior of the event, even if the modal hasn’t been resolved yet.
			Once the modal is resolved (the user accepted to discard the changes and navigate away from the route), we will push a new history state.
			This push will make the "willchangestate" event happen again and due to this somewhat "backward" behavior,
			we set an "allowNavigateAway"-flag to prevent the "discard-changes" functionality from running in a loop.*/
			e.preventDefault();
			const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT).catch(() => undefined);
			const modal = modalManager?.open(this, UMB_DISCARD_CHANGES_MODAL);
			if (modal) {
				try {
					// navigate to the new url when discarding changes
					await modal.onSubmit();
					this.#allowNavigateAway = true;
					history.pushState({}, '', e.detail.url);
					return true;
				} catch {
					return false;
				}
			} else {
				console.error('No modal manager found!');
			}
		}

		return true;
	};

	/**
	 * Check if the workspace is about to navigate away.
	 * @protected
	 * @param {string} newUrl The new url that the workspace is navigating to.
	 * @returns { boolean} true if the workspace is navigating away.
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	protected _checkWillNavigateAway(newUrl: string): boolean {
		return !newUrl.includes(this.routes.getActiveLocalPath());
	}

	setEditorSize(editorSize: UUIModalSidebarSize) {
		this.#modalContext?.setModalSize(editorSize);
	}

	protected override resetState() {
		super.resetState();
		this.#name.setValue(undefined);
		this.#layout.setValue(undefined);
		this.#initialLayout = undefined;
		this.#initialContent = undefined;
		this.#initialSettings = undefined;
		this.content.resetState();
		this.settings.resetState();
		this.#allowNavigateAway = false;
		this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
	}

	async load(unique: string) {
		await this.#retrieveBlockManager;
		await this.#retrieveBlockEntries;
		if (!this.#blockManager || !this.#blockEntries) {
			throw new Error('Block manager not found');
		}

		this.observe(
			this.#blockEntries.layoutOf(unique),
			(layoutData) => {
				this.#initialLayout ??= layoutData as LayoutDataType;
				this.removeUmbControllerByAlias('observeLayoutInitially');
			},
			'observeLayoutInitially',
		);

		this.#observeBlockData(unique);
		if (this.#liveEditingMode) {
			this.establishLiveSync();
		}
	}

	async create(contentElementTypeId: string) {
		await this.#retrieveBlockEntries;
		await this.#retrieveModalContext;
		if (!this.#blockEntries) {
			throw new Error('Block Entries not found');
		}
		if (!this.#originData) {
			throw new Error('Origin data not defined');
		}

		// TODO: Missing some way to append more layout data... this could be part of modal data, (or context api?)

		this.setIsNew(true);

		const blockCreated = await this.#blockEntries.create(contentElementTypeId, {}, this.#originData);
		if (!blockCreated) {
			throw new Error('Block Entries could not create block');
		}

		// TODO: We should investigate if it makes sense to gather

		if (this.#liveEditingMode) {
			// Insert already, cause we are in live editing mode:
			const blockInserted = await this.#blockEntries.insert(
				blockCreated.layout,
				blockCreated.content,
				blockCreated.settings,
				this.#originData,
			);
			if (!blockInserted) {
				throw new Error('Block Entries could not insert block');
			}

			const unique = blockCreated.layout.contentKey;

			this.#observeBlockData(unique);
			this.establishLiveSync();
		} else {
			this.#layout.setValue(blockCreated.layout as LayoutDataType);
			this.content.setData(blockCreated.content);
			if (blockCreated.settings) {
				this.settings.setData(blockCreated.settings);
			}
		}
	}

	#observeBlockData(unique: string) {
		if (!this.#blockEntries) {
			throw new Error('Block Entries not found');
		}
		this.observe(
			this.#blockEntries.layoutOf(unique),
			(layoutData) => {
				this.#layout.setValue(layoutData as LayoutDataType);

				// Content:
				const contentKey = layoutData?.contentKey;
				if (!contentKey) {
					return;
				}

				this.observe(
					this.#blockManager!.contentOf(contentKey),
					(contentData) => {
						this.content.setData(contentData);
					},
					'observeContent',
				);
				if (!this.#initialContent) {
					this.observe(
						this.#blockManager!.contentOf(contentKey),
						(contentData) => {
							this.#initialContent ??= contentData;
							this.removeUmbControllerByAlias('observeContentInitially');
						},
						'observeContentInitially',
					);
				}

				// Settings:
				const settingsKey = layoutData?.settingsKey;
				if (settingsKey) {
					this.observe(
						this.#blockManager!.settingsOf(settingsKey),
						(settingsData) => {
							this.settings.setData(settingsData);
						},
						'observeSettings',
					);
					if (!this.#initialSettings) {
						this.observe(
							this.#blockManager!.settingsOf(settingsKey),
							(settingsData) => {
								this.#initialSettings ??= settingsData;
								this.removeUmbControllerByAlias('observeSettingsInitially');
							},
							'observeSettingsInitially',
						);
					}
				}
			},
			'observeLayout',
		);
	}

	/**
	 * Establishes live synchronization of the block's layout, content, and settings data.
	 * This method observes local changes in the layout, content, and settings data and pushes those updates to the block manager.
	 * This method is used in live editing mode to ensure that changes made to the block's data are immediately reflected
	 * in the backoffice UI.
	 */
	establishLiveSync() {
		// Syncing Layout data is not a necessity, but it was an idea that someone might wanted to manipulate that from this workspace, but as it is giving trouble in Block Grid with Inline Editing Live Sync, then its taken out for now. [NL]
		let initialLayoutSet = true;
		this.observe(
			this.layout,
			(layoutData) => {
				if (layoutData) {
					if (initialLayoutSet) {
						initialLayoutSet = false;
						return;
					}
					this.#blockManager?.setOneLayout(layoutData, this.#originData);
				}
			},
			'observeThisLayout',
		);
		this.observe(
			this.content.data,
			(contentData) => {
				if (contentData) {
					this.#blockManager?.setOneContent(contentData);
				}
			},
			'observeThisContent',
		);
		this.observe(
			this.settings.data,
			(settingsData) => {
				if (settingsData) {
					this.#blockManager?.setOneSettings(settingsData);
				}
			},
			'observeThisSettings',
		);
	}

	getData() {
		return this.#layout.getValue();
	}

	getUnique() {
		return this.getData()!.contentKey;
	}

	getEntityType() {
		return this.#entityType;
	}

	getName() {
		return 'block name content element type here...';
	}

	/**
	 * Check if there are unpersisted changes.
	 * @returns { boolean } true if there are unpersisted changes.
	 */
	public getHasUnpersistedChanges(): boolean {
		return this.content.getHasUnpersistedChanges() || this.settings.getHasUnpersistedChanges();
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias - The alias of the property to get the value of.
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>} - The value of the property.
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<propertyAliasType extends keyof LayoutDataType>(propertyAlias: propertyAliasType) {
		return this.#layout.asObservablePart(
			(layout) => layout?.[propertyAlias as keyof LayoutDataType] as LayoutDataType[propertyAliasType],
		);
	}

	getPropertyValue<propertyAliasType extends keyof LayoutDataType>(propertyAlias: propertyAliasType) {
		// TODO: Should be using Content, then we need a toggle or another method for getting settings.
		return this.#layout.getValue()?.[propertyAlias as keyof LayoutDataType] as LayoutDataType[propertyAliasType];
	}

	/**
	 * @function setPropertyValue
	 * @param {string} alias - The alias of the property to set the value of.
	 * @param {unknown} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
	async setPropertyValue(alias: string, value: unknown) {
		const currentData = this.#layout.value;
		if (currentData) {
			this.#layout.update({ ...currentData, [alias]: await value });
		}
	}

	async submit() {
		const layoutData = this.#layout.value;
		const contentData = this.content.getData();
		if (!layoutData || !this.#blockManager || !this.#blockEntries || !contentData || !this.#originData) {
			throw new Error('Missing data');
		}

		const settingsData = this.settings.getData();
		this.content.setPersistedData(contentData);
		if (settingsData) {
			this.settings.setPersistedData(settingsData);
		}

		if (!this.#liveEditingMode) {
			if (this.getIsNew() === true) {
				// Insert (This means the layout entry will be inserted at the desired location):
				const blockInserted = await this.#blockEntries.insert(layoutData, contentData, settingsData, this.#originData);
				if (!blockInserted) {
					throw new Error('Block Entries could not insert block');
				}
			} else {
				// Update data:

				this.#blockManager.setOneLayout(layoutData, this.#originData);
				if (contentData) {
					this.#blockManager.setOneContent(contentData);
				}
				if (settingsData) {
					this.#blockManager.setOneSettings(settingsData);
				}
			}
		}

		this.#expose(layoutData.contentKey);
		this.setIsNew(false);

		this.#reportValidation();
	}

	#reportValidation() {
		this.content.validation.report();
		this.settings.validation.report();
	}

	expose() {
		const contentKey = this.#layout.value?.contentKey;
		if (!contentKey) throw new Error('Failed to expose block, missing content key.');
		this.#expose(contentKey);
	}

	#expose(unique: string) {
		const variantId = this.#variantId.getValue();
		if (!variantId) throw new Error('Failed to expose block, missing variant id.');
		this.#blockManager?.setOneExpose(unique, variantId);
	}

	#modalRejected = () => {
		if (this.#liveEditingMode) {
			// Revert
			// Did it exist before?
			if (this.getIsNew() === true) {
				// Remove the block?
				const contentKey = this.#layout.value?.contentKey;
				if (contentKey) {
					this.#blockEntries?.delete(contentKey);
				}
			} else {
				// Revert the layout, content & settings data to the original state: [NL]
				if (this.#initialLayout) {
					this.#blockManager?.setOneLayout(this.#initialLayout, this.#originData);
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

	public override destroy(): void {
		super.destroy();
		window.removeEventListener('willchangestate', this.#onWillNavigate);
		this.#layout?.destroy();
		this.#name?.destroy();
		this.#layout = undefined as any;
		this.#name = undefined as any;
		this.#blockManager = undefined;
		this.#originData = undefined;
	}
}

export { UmbBlockWorkspaceContext as api };
