import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

export type * from './extensions/types.js';
export type * from './kinds/types.js';
export type * from './conditions/types.js';
export type * from './data-manager/types.js';
export type * from './workspace-context.interface.js';
export type * from './namable/types.js';

/**
 * @deprecated Use `UmbEntityUnique`instead.
 */
export type UmbWorkspaceUniqueType = UmbEntityUnique;
