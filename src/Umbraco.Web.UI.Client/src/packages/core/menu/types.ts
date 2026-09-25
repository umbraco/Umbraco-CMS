import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type * from './components/types.js';
export type * from './conditions/types.js';
export type * from './menu-structure/types.js';
export type * from './menu.extension.js';
export type * from './section-sidebar-menu/types.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbStructureItemModelBase extends UmbEntityModel {}

export interface UmbStructureItemModel extends UmbStructureItemModelBase {
	name: string;
	isFolder: boolean;
}

export interface UmbVariantStructureItemModel extends UmbStructureItemModelBase {
	/** The item's flat, culture-agnostic name. Used to display non-variant items (e.g. folders) alongside variant ones. */
	name?: string;
	isFolder?: boolean;
	variants: Array<{ name: string; culture: string | null; segment: string | null }>;
}
