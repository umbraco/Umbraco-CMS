import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type * from './components/types.js';
export type * from './conditions/types.js';
export type * from './menu-item-element.interface.js';
export type * from './menu-item.extension.js';
export type * from './menu.extension.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbStructureItemModelBase extends UmbEntityModel {}

export interface UmbStructureItemModel extends UmbStructureItemModelBase {
	name: string;
	isFolder: boolean;
}

export interface UmbVariantStructureItemModel extends UmbStructureItemModelBase {
	variants: Array<{ name: string; culture: string | null; segment: string | null }>;
}
