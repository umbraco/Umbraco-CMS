import type { ManifestWorkspaceContextMenuStructureKind, UmbVariantStructureItemModel } from './types.js';
import { UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT } from './menu-variant-structure-workspace-context.context-token.js';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from './section-sidebar-menu/section-context/section-sidebar-menu.section-context.token.js';
import type { UmbTreeItemModel, UmbTreeRepository, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbAncestorsEntityContext, UmbParentEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import {
	UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT,
	UMB_VARIANT_WORKSPACE_CONTEXT,
	UMB_WORKSPACE_EDIT_PATH_PATTERN,
	UMB_WORKSPACE_EDIT_VARIANT_PATH_PATTERN,
} from '@umbraco-cms/backoffice/workspace';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

interface UmbMenuVariantTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

// TODO: introduce base class for all menu structure workspaces to handle ancestors and parent
export abstract class UmbMenuVariantTreeStructureWorkspaceContextBase extends UmbContextBase {
	manifest?: ManifestWorkspaceContextMenuStructureKind;

	#workspaceContext?: typeof UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT.TYPE;
	#args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs;

	#structure = new UmbArrayState<UmbVariantStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	#parent = new UmbObjectState<UmbVariantStructureItemModel | undefined>(undefined);
	/**
	 * @deprecated Will be removed in v.18: Use UMB_PARENT_ENTITY_CONTEXT instead.
	 */
	public readonly parent = this.#parent.asObservable();

	protected _sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;

	#parentContext = new UmbParentEntityContext(this);
	#ancestorContext = new UmbAncestorsEntityContext(this);
	#sectionSidebarMenuContext?: typeof UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT.TYPE;
	#isModalContext: boolean = false;
	#isNew: boolean | undefined = undefined;
	#variantWorkspaceContext?: typeof UMB_VARIANT_WORKSPACE_CONTEXT.TYPE;
	#workspaceActiveVariantId?: UmbVariantId;

	public readonly IS_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT = true;

	constructor(host: UmbControllerHost, args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs) {
		super(host, UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT);
		// 'UmbMenuStructureWorkspaceContext' is Obsolete, will be removed in v.18
		this.provideContext('UmbMenuStructureWorkspaceContext', this);
		this.#args = args;

		this.consumeContext(UMB_MODAL_CONTEXT, (modalContext) => {
			this.#isModalContext = modalContext !== undefined;
		});

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this._sectionContext = instance;
		});

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			if (!instance) return;
			this.#variantWorkspaceContext = instance;
			this.#observeWorkspaceActiveVariant();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT, (instance) => {
			this.#sectionSidebarMenuContext = instance;
		});

		this.consumeContext(UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(
				this.#workspaceContext?.unique,
				(value) => {
					if (!value) return;
					this.#requestStructure();
				},
				'observeUnique',
			);

			this.observe(
				this.#workspaceContext?.isNew,
				(value) => {
					// Workspace has changed from new to existing
					if (value === false && this.#isNew === true) {
						// TODO: We do not need to request here as we already know the structure and unique
						this.#requestStructure();
					}
					this.#isNew = value;
				},
				'observeIsNew',
			);
		});
	}

	getItemHref(structureItem: UmbVariantStructureItemModel): string | undefined {
		const sectionName = this._sectionContext?.getPathname();
		if (!sectionName) return undefined;

		const unique = structureItem.unique;
		if (!unique) return undefined;

		// find related variant id from structure item:
		const itemVariantFit = structureItem.variants.find(
			(variant) =>
				variant.culture === this.#workspaceActiveVariantId?.culture &&
				variant.segment === this.#workspaceActiveVariantId?.segment,
		);

		if (itemVariantFit) {
			const variantId = UmbVariantId.CreateFromPartial(itemVariantFit);
			return UMB_WORKSPACE_EDIT_VARIANT_PATH_PATTERN.generateAbsolute({
				sectionName,
				entityType: structureItem.entityType,
				unique,
				variantId: variantId.toString(),
			});
		}

		// If no related variantID, then lets the redirect go to the main-variant:
		return UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
			sectionName,
			entityType: structureItem.entityType,
			unique,
		});
	}

	async #requestStructure() {
		const isNew = this.#workspaceContext?.getIsNew();
		const uniqueObservable = isNew
			? this.#workspaceContext?._internal_createUnderParentEntityUnique
			: this.#workspaceContext?.unique;
		const entityTypeObservable = isNew
			? this.#workspaceContext?._internal_createUnderParentEntityType
			: this.#workspaceContext?.entityType;

		let structureItems: Array<UmbVariantStructureItemModel> = [];

		const unique = (await this.observe(uniqueObservable, () => {})?.asPromise()) as string;
		if (unique === undefined) throw new Error('Unique is not available');

		const entityType = (await this.observe(entityTypeObservable, () => {})?.asPromise()) as string;
		if (!entityType) throw new Error('Entity type is not available');

		// TODO: introduce variant tree item model
		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<any, UmbTreeRootModel>>(
			this,
			this.#args.treeRepositoryAlias,
		);

		const { data: root } = await treeRepository.requestTreeRoot();

		if (root) {
			structureItems = [
				{
					unique: root.unique,
					entityType: root.entityType,
					variants: [{ name: root.name, culture: null, segment: null }],
				},
			];
		}

		const { data } = await treeRepository.requestTreeItemAncestors({ treeItem: { unique, entityType } });

		if (data) {
			const treeItemAncestors = data.map((treeItem) => {
				return {
					unique: treeItem.unique,
					entityType: treeItem.entityType,
					variants: treeItem.variants.map((variant: any) => {
						return {
							name: variant.name,
							culture: variant.culture,
							segment: variant.segment,
						};
					}),
				};
			});

			structureItems.push(...treeItemAncestors);

			this.#structure.setValue(structureItems);
			this.#setParentData(structureItems);
			this.#setAncestorData(data);

			const menuItemAlias = this.manifest?.meta?.menuItemAlias;
			if (menuItemAlias && !this.#isModalContext) {
				this.#expandSectionSidebarMenu(structureItems, menuItemAlias);
			}
		}
	}

	#setParentData(structureItems: Array<UmbVariantStructureItemModel>) {
		/* If the item is not new, the current item is the last item in the array.
			We filter out the current item unique to handle any case where it could show up */
		const parent = structureItems.filter((item) => item.unique !== this.#workspaceContext?.getUnique()).pop();

		// TODO: remove this when the parent gets removed from the structure interface
		this.#parent.setValue(parent);

		const parentEntity = parent
			? {
					unique: parent.unique,
					entityType: parent.entityType,
				}
			: undefined;

		this.#parentContext.setParent(parentEntity);
	}

	/* Notice: ancestors are based on the server "data" ancestors and are not based on the full Menu (UI) structure.
		This will mean that any item placed in the data root will not have any ancestors. But will have a parent based on the UI structure.
	*/
	#setAncestorData(ancestors: Array<UmbTreeItemModel>) {
		const ancestorEntities = ancestors
			.map((treeItem) => {
				const entity: UmbEntityModel = {
					unique: treeItem.unique,
					entityType: treeItem.entityType,
				};

				return entity;
			})
			/* If the item is not new, the current item is the last item in the array.
			We filter out the current item unique to handle any case where it could show up */
			.filter((item) => item.unique !== this.#workspaceContext?.getUnique());

		this.#ancestorContext.setAncestors(ancestorEntities);
	}

	#expandSectionSidebarMenu(structureItems: Array<UmbVariantStructureItemModel>, menuItemAlias: string) {
		const linkedEntries = linkEntityExpansionEntries(structureItems);
		// Filter out the current entity as we don't want to expand it
		const expandableItems = linkedEntries.filter((item) => item.unique !== this.#workspaceContext?.getUnique());
		const expandableItemsWithMenuItem = expandableItems.map((item) => {
			return {
				...item,
				menuItemAlias,
			};
		});
		this.#sectionSidebarMenuContext?.expansion.expandItems(expandableItemsWithMenuItem);
	}

	#observeWorkspaceActiveVariant() {
		this.observe(
			this.#variantWorkspaceContext?.splitView.activeVariantsInfo,
			(value) => {
				if (!value) return;
				if (value?.length === 0) return;
				this.#workspaceActiveVariantId = UmbVariantId.Create(value[0]);
			},

			'breadcrumbWorkspaceActiveVariantObserver',
		);
	}

	override destroy(): void {
		super.destroy();
		this.#structure.destroy();
		this.#parent.destroy();
		this.#parentContext.destroy();
		this.#ancestorContext.destroy();
	}
}
