import type { UmbPickerPropertyEditorDataSource } from '../data-source/types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbPickerPropertyEditorCollectionDataSource
	extends UmbPickerPropertyEditorDataSource,
		UmbCollectionRepository,
		UmbApi {}
