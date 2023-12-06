import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbFileSystemTreeItemModel, UmbFileSystemTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbScriptTreeItemModel extends FileSystemTreeItemPresentationModel, UmbFileSystemTreeItemModel {}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbScriptTreeRootModel
	extends Omit<Omit<FileSystemTreeItemPresentationModel, 'id'>, 'path'>,
		UmbFileSystemTreeRootModel {}
