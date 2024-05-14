import type { UmbTemplateEntityType, UmbTemplateRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbTemplateTreeItemModel extends UmbTreeItemModel {
	entityType: UmbTemplateEntityType;
}

export interface UmbTemplateTreeRootModel extends UmbTreeRootModel {
	entityType: UmbTemplateRootEntityType;
}
