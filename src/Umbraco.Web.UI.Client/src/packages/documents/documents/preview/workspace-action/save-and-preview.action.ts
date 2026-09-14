import { UmbDocumentSaveWorkspaceAction } from '../../workspace/actions/save.action.js';

export class UmbDocumentSaveAndPreviewWorkspaceAction extends UmbDocumentSaveWorkspaceAction {
	override async execute() {
		await this._retrieveWorkspaceContext;

		if (!this._workspaceContext) {
			return;
		}

		await this._workspaceContext.saveAndPreview(undefined);
	}
}

export { UmbDocumentSaveAndPreviewWorkspaceAction as api };
