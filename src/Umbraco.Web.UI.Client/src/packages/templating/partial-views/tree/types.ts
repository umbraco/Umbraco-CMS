import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbFileSystemTreeItemModel, UmbFileSystemTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbPartialViewTreeItemModel extends FileSystemTreeItemPresentationModel, UmbFileSystemTreeItemModel {}
// TODO: TREE STORE TYPE PROBLEM:
export interface UmbPartialViewTreeRootModel
	extends Omit<FileSystemTreeItemPresentationModel, 'path'>,
		UmbFileSystemTreeRootModel {}
