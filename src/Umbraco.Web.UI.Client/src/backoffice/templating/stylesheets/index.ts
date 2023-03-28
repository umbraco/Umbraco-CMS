import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: temp until we have a proper stylesheet model
export interface StylesheetDetails extends FileSystemTreeItemPresentationModel {
	content: string;
}

export const STYLESHEET_ENTITY_TYPE = 'stylesheet';
