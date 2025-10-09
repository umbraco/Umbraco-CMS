import type { UmbPropertyEditorDataSourceConfigModel } from '../types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-data-source.extension.js';

export interface UmbPropertyEditorDataSource extends UmbApi {
	setConfig?(config: UmbPropertyEditorDataSourceConfigModel | undefined): void;
	getConfig?(): UmbPropertyEditorDataSourceConfigModel | undefined;
}
