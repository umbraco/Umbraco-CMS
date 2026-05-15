import type { UmbRelationTypeCollectionFilterModel, UmbRelationTypeCollectionItemModel } from './types.js';
import { UMB_EDIT_RELATION_TYPE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

export class UmbRelationTypeCollectionContext extends UmbDefaultCollectionContext<
	UmbRelationTypeCollectionItemModel,
	UmbRelationTypeCollectionFilterModel
> {
	override async requestItemHref(item: UmbRelationTypeCollectionItemModel): Promise<string | undefined> {
		return UMB_EDIT_RELATION_TYPE_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });
	}
}

export { UmbRelationTypeCollectionContext as api };
