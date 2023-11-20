import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbFileSystemTreeItemModel, UmbFileSystemTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbPartialViewTreeItemModel = FileSystemTreeItemPresentationModel & UmbFileSystemTreeItemModel;
export type UmbPartialViewTreeRootModel = FileSystemTreeItemPresentationModel & UmbFileSystemTreeRootModel;
