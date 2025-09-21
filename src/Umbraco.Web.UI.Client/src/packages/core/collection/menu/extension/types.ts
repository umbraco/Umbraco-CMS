import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './collection-menu.extension.js';

export interface UmbPropertyEditorDataSource extends UmbApi {
	execute(): Promise<void>;
}
