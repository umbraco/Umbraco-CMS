import { UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT } from './menu-structure-workspace-context.context-token.js';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from './section-sidebar-menu/index.js';
import type { ManifestWorkspaceContextMenuStructureKind, UmbStructureItemModel } from './types.js';
import type { UmbMenuStructureWorkspaceContext } from './menu-structure-workspace-context.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { debounce, linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';
import { UmbAncestorsEntityContext, UmbParentEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import {
	UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT,
	UMB_WORKSPACE_EDIT_PATH_PATTERN,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeRepository, UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

interface UmbMenuTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

// TODO: introduce base class for all menu structure workspaces to handle ancestors and parent
export abstract class UmbMenuTreeStructureWorkspaceContextBase
	extends UmbContextBase
	implements UmbMenuStructureWorkspaceContext
{
	manifest?: ManifestWorkspaceContextMenuStructureKind;

	#workspaceContext?: typeof UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT.TYPE;
	readonly #args: UmbMenuTreeStructureWorkspaceContextBaseArgs;

	readonly #structure = new UmbArrayState<UmbStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	protected _sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;

	readonly #parentContext = new UmbParentEntityContext(this);
	readonly #ancestorContext = new UmbAncestorsEntityContext(this);
	#sectionSidebarMenuContext?: typeof UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT.TYPE;
	#isModalContext: boolean = false;
	#isNew: boolean | undefined = undefined;
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#structureRequestId = 0;

	// Coalesces the unique/isNew/reload-event triggers when they fire in quick succession.
	readonly #requestStructure = debounce(() => this.#requestStructureImpl(), 100);

	constructor(host: UmbControllerHost, args: UmbMenuTreeStructureWorkspaceContextBaseArgs) {
		super(host, UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT);
		this.#args = args;

		this.consumeContext(UMB_MODAL_CONTEXT, (modalContext) => {
			this.#isModalContext = modalContext !== undefined;
		});

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this._sectionContext = instance;
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#removeEventListeners();
			this.#actionEventContext = instance;
			this.#addEventListeners();
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
					// Clear immediately so the previous entity's breadcrumb/structure isn't shown while the new one loads.
					this.#clearStructure();
					this.#requestStructure();
				},
				'observeUnique',
			);

			// isNew is observed on its own, separate from the structure fetch, so the expand decision never
			// depends on which of the two happens to settle first: whichever settles last (isNew resolving to
			// false, or the structure fetch resolving) is the one that actually triggers the expand.
			this.observe(
				this.#workspaceContext?.isNew,
				(isNew) => {
					// The item has just been created: the structure fetched while new was based on the parent (the
					// item didn't exist yet), so it must be re-fetched using the item's own identity - otherwise the
					// structure never ends with the item itself, which the breadcrumb relies on when trimming it.
					if (isNew === false && this.#isNew === true) {
						this.#requestStructure();
					} else if (isNew === false) {
						this.#tryExpandSectionSidebarMenu();
					}
					this.#isNew = isNew;
				},
				'observeIsNew',
			);
		});
	}

	getItemHref(structureItem: UmbStructureItemModel): string | undefined {
		if (structureItem.isFolder || !structureItem.unique) return undefined;

		const sectionName = this._sectionContext?.getPathname();
		if (!sectionName) return undefined;

		return UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
			sectionName,
			entityType: structureItem.entityType,
			unique: structureItem.unique,
		});
	}

	#addEventListeners() {
		this.#actionEventContext?.addEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadStructureForEntityRequest as EventListener,
		);
	}

	#removeEventListeners() {
		this.#actionEventContext?.removeEventListener(
			UmbRequestReloadStructureForEntityEvent.TYPE,
			this.#onReloadStructureForEntityRequest as EventListener,
		);
	}

	readonly #onReloadStructureForEntityRequest = (event: UmbRequestReloadStructureForEntityEvent) => {
		if (!this.#isCurrentEntityOrAncestor(event.getEntityType(), event.getUnique())) return;
		this.#requestStructure();
	};

	#isCurrentEntityOrAncestor(entityType: string, unique: string | null): boolean {
		if (entityType === this.#workspaceContext?.getEntityType() && unique === this.#workspaceContext?.getUnique()) {
			return true;
		}

		return this.#ancestorContext
			.getAncestors()
			.some((ancestor) => ancestor.entityType === entityType && ancestor.unique === unique);
	}

	async #requestStructureImpl() {
		const requestId = ++this.#structureRequestId;
		const isNew = this.#workspaceContext?.getIsNew();
		const uniqueObservable = isNew
			? this.#workspaceContext?._internal_createUnderParentEntityUnique
			: this.#workspaceContext?.unique;
		const entityTypeObservable = isNew
			? this.#workspaceContext?._internal_createUnderParentEntityType
			: this.#workspaceContext?.entityType;

		let structureItems: Array<UmbStructureItemModel> = [];

		const unique = (await this.observe(uniqueObservable, () => {})?.asPromise()) as string;
		if (unique === undefined) {
			if (this._host) console.warn('[UmbMenuTreeStructureWorkspaceContextBase] unique not available');
			return;
		}

		const entityType = (await this.observe(entityTypeObservable, () => {})?.asPromise()) as string;
		if (!entityType) {
			if (this._host) console.warn('[UmbMenuTreeStructureWorkspaceContextBase] entityType not available');
			return;
		}

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<UmbTreeItemModel, UmbTreeRootModel>>(
			this,
			this.#args.treeRepositoryAlias,
		);

		const { data: root } = await treeRepository.requestTreeRoot();

		if (root) {
			structureItems = [
				{
					unique: root.unique,
					entityType: root.entityType,
					name: root.name,
					isFolder: root.isFolder,
				},
			];
		}

		const isRoot = entityType === root?.entityType;

		// If the entity type is different from the root entity type, then we can request the ancestors.
		let ancestorData: Array<UmbTreeItemModel> | undefined;
		if (!isRoot) {
			const { data } = await treeRepository.requestTreeItemAncestors({ treeItem: { unique, entityType } });

			if (data) {
				const ancestorItems = data.map((treeItem) => {
					return {
						unique: treeItem.unique,
						entityType: treeItem.entityType,
						name: treeItem.name,
						isFolder: treeItem.isFolder,
					};
				});

				ancestorData = data;

				structureItems.push(...ancestorItems);
			}
		}

		// Guard: this context may have been destroyed while the async requests were in flight.
		if (!this._host) return;

		// Guard: a newer request has superseded this one; its result would already be stale.
		if (requestId !== this.#structureRequestId) return;

		if (ancestorData) {
			this.#setAncestorData(ancestorData);
		}

		this.#structure.setValue(structureItems);
		this.#setParentData(structureItems);

		this.#tryExpandSectionSidebarMenu();
	}

	/**
	 * Expands the parent in the section sidebar menu, but only once we know for certain the item isn't still being
	 * created, and only once the structure has actually been fetched. Reads both conditions fresh, so it's safe to
	 * call from either the structure-fetch completion or the isNew observer, whichever settles last.
	 */
	#tryExpandSectionSidebarMenu() {
		const menuItemAlias = this.manifest?.meta?.menuItemAlias;
		if (!menuItemAlias || this.#isModalContext) return;

		// Don't expand the parent for an item that hasn't been created yet.
		if (this.#workspaceContext?.getIsNew() !== false) return;

		const structureItems = this.#structure.getValue();
		if (!structureItems.length) return;

		this.#expandSectionSidebarMenu(structureItems, menuItemAlias);
	}

	#clearStructure() {
		this.#structure.setValue([]);
		this.#parentContext.setParent(undefined);
		this.#ancestorContext.setAncestors([]);
	}

	#setParentData(structureItems: Array<UmbStructureItemModel>) {
		/* If the item is not new, the current item is the last item in the array.
			We filter out the current item unique to handle any case where it could show up */
		const parent = structureItems.filter((item) => item.unique !== this.#workspaceContext?.getUnique()).pop();

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

	#expandSectionSidebarMenu(structureItems: Array<UmbStructureItemModel>, menuItemAlias: string) {
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

	override destroy(): void {
		this.#requestStructure.cancel();
		this.#removeEventListeners();
		super.destroy();
		this.#structure.destroy();
		this.#parentContext.destroy();
		this.#ancestorContext.destroy();
	}
}
