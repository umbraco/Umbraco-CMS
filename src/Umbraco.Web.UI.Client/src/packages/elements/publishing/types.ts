import type { UmbEntityVariantPublishModel } from '@umbraco-cms/backoffice/variant';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementVariantPublishModel extends UmbEntityVariantPublishModel {}

export type * from './publish/types.js';
export type * from './unpublish/types.js';
export type * from './schedule-publish/types.js';
export type * from './workspace-context/types.js';
