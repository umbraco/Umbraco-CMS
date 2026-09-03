import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../workspace/member/member-workspace.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_WORKSPACE_EDIT_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT,
	type UmbMenuVariantStructureWorkspaceContext,
	type UmbVariantStructureItemModel,
} from '@umbraco-cms/backoffice/menu';
import { UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

/**
 * Members are invariant, so unlike the tree-based variant structure context, this hand-rolled
 * structure context does not need to resolve ancestors or track the active split-view variant.
 */
export class UmbMemberMenuStructureWorkspaceContext
	extends UmbContextBase
	implements UmbMenuVariantStructureWorkspaceContext
{
	// Marker read by UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT's apiCheck to discover this context.
	public readonly IS_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT = true;

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;
	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;

	#structure = new UmbArrayState<UmbVariantStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	#parentContext = new UmbParentEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT);
		// 'UmbMenuStructureWorkspaceContext' is Obsolete, will be removed in v.18
		this.provideContext('UmbMenuStructureWorkspaceContext', this);

		this.#parentContext.setParent({ unique: null, entityType: UMB_MEMBER_ROOT_ENTITY_TYPE });

		this.consumeContext(UMB_SECTION_CONTEXT, (instance) => {
			this.#sectionContext = instance;
		});

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			if (!instance) return;

			this.observe(
				observeMultiple([instance.unique, instance.entityType, instance.variants]),
				([unique, entityType, variants]) => this.#requestStructure(unique, entityType, variants),
				'umbMemberMenuStructureObserver',
			);
		});
	}

	getItemHref(structureItem: UmbVariantStructureItemModel): string | undefined {
		const sectionName = this.#sectionContext?.getPathname();
		if (!sectionName || !structureItem.unique) return undefined;

		return UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
			sectionName,
			entityType: structureItem.entityType,
			unique: structureItem.unique,
		});
	}

	#requestStructure(
		unique: string | null | undefined,
		entityType: string | undefined,
		variants: Array<UmbEntityVariantModel>,
	) {
		if (!entityType) return;

		// While new, the item itself does not exist yet, so its ancestors are just the (fixed) root.
		const items: Array<UmbVariantStructureItemModel> = [
			{
				unique: null,
				entityType: UMB_MEMBER_ROOT_ENTITY_TYPE,
				variants: [{ name: '#treeHeaders_member', culture: null, segment: null }],
			},
		];

		if (!this.#workspaceContext?.getIsNew()) {
			items.push({
				unique: unique ?? null,
				entityType,
				variants: variants.map((variant) => ({
					name: variant.name,
					culture: variant.culture,
					segment: variant.segment,
				})),
			});
		}

		this.#structure.setValue(items);
	}

	override destroy(): void {
		super.destroy();
		this.#structure.destroy();
	}
}

export { UmbMemberMenuStructureWorkspaceContext as api };
