import type { UmbPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export interface UmbPickerPropertyEditorDataSource
	extends UmbPropertyEditorDataSource,
		UmbItemRepository<any>,
		UmbApi {}
