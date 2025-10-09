import type { UmbPickerPropertyEditorDataSource } from '../data-source/types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

export interface UmbPickerPropertyEditorTreeDataSource
	extends UmbPickerPropertyEditorDataSource,
		UmbTreeRepository,
		UmbApi {}
