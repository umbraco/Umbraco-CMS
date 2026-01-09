import { UmbDocumentSaveWorkspaceAction } from '../../workspace/actions/save.action.js';

export class UmbDocumentSaveAndPreviewWorkspaceAction extends UmbDocumentSaveWorkspaceAction {
	override async execute() {
		await this._retrieveWorkspaceContext;
		await this._workspaceContext?.saveAndPreview();
	}
}

export { UmbDocumentSaveAndPreviewWorkspaceAction as api };
