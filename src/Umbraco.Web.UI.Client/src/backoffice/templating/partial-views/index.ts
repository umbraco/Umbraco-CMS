import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: temp until we have a proper stylesheet model
export interface PartialViewDetails extends FileSystemTreeItemPresentationModel {
	content: string;
}

export const PARTIAL_VIEW_ENTITY_TYPE = 'partial-view';
