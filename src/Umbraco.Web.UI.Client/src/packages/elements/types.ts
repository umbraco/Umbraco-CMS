import type { UmbContentDetailModel } from '@umbraco-cms/backoffice/content';

export type * from './tree/types.js';
export type * from './entity.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementDetailModel extends UmbContentDetailModel {}
