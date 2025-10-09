import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export type * from './property-data-source.extension.js';

export interface UmbPropertyEditorDataSource extends UmbApi {
	setConfig?(config: UmbConfigCollectionModel | undefined): void;
	getConfig?(): UmbConfigCollectionModel | undefined;
}
