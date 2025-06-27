import type { UmbDataTypeEntityType, UmbDataTypeRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface ExampleTreeItemModel extends UmbTreeItemModel {
	entityType: 'example';
}

export interface ExampleTreeRootModel extends UmbTreeRootModel {
	entityType: 'example-root';
}
