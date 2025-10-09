import type { UmbPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';

export interface UmbPickerPropertyEditorSearchableDataSource
	extends UmbPropertyEditorDataSource,
		UmbSearchRepository<any> {}
