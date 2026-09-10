import type { UmbVariantStructureItemModel } from './types.js';
import type { UmbMenuStructureWorkspaceContext } from './menu-structure-workspace-context.interface.js';

export interface UmbMenuVariantStructureWorkspaceContext extends UmbMenuStructureWorkspaceContext<UmbVariantStructureItemModel> {
	getItemHref(structureItem: UmbVariantStructureItemModel): string | undefined;
}
