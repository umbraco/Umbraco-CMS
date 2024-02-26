import type { UmbTemplateEntityType, UmbTemplateRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbTemplateTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbTemplateEntityType;
}

export interface UmbTemplateTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbTemplateRootEntityType;
}
