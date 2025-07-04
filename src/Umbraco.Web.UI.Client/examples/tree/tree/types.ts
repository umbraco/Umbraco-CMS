import type { ExampleEntityType, ExampleRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface ExampleTreeItemModel extends UmbTreeItemModel {
	entityType: ExampleEntityType;
}

export interface ExampleTreeRootModel extends UmbTreeRootModel {
	entityType: ExampleRootEntityType;
}
