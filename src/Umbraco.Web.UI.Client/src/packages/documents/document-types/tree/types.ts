import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentTypeTreeItemModel extends UmbEntityTreeItemModel {
	isElement: boolean;
	icon?: string | null;
}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbDocumentTypeTreeRootModel extends UmbEntityTreeRootModel {}
