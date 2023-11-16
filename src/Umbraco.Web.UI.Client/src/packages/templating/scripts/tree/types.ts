import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbFileSystemTreeItemModel, UmbFileSystemTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbScriptTreeItemModel = FileSystemTreeItemPresentationModel & UmbFileSystemTreeItemModel;
export type UmbScriptTreeRootModel = FileSystemTreeItemPresentationModel & UmbFileSystemTreeRootModel;
