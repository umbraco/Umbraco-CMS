import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbMediaItemRepository,
	UmbMediaSearchRepository,
	UmbMediaTreeRepository,
} from '@umbraco-cms/backoffice/media';

export class UmbMediaPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorTreeDataSource
{
	tree = new UmbMediaTreeRepository(this);
	item = new UmbMediaItemRepository(this);
	search = new UmbMediaSearchRepository(this);
}

export { UmbMediaPickerPropertyEditorDataSource as api };
