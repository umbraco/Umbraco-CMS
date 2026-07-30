import { UmbDocumentSaveWorkspaceAction } from '../../workspace/actions/save.action.js';

export class UmbDocumentSaveAndPreviewWorkspaceAction extends UmbDocumentSaveWorkspaceAction {
	override async execute() {
		// Opened before the first await, while we are still inside the click's synchronous call stack:
		// Safari only permits window.open() from there, and saving takes several awaits — including
		// server round-trips — before a preview URL exists (#22626). Targeting the document's own
		// preview tab reuses it instead of flashing a second one open and shut. The preview controller
		// adopts this tab, or closes it if no preview follows.
		const unique = this._workspaceContext?.getUnique();
		const previewWindow = window.open('', unique ? `umbpreview-${unique}` : '_blank');

		await this._retrieveWorkspaceContext;

		if (!this._workspaceContext) {
			previewWindow?.close();
			return;
		}

		await this._workspaceContext.saveAndPreview(undefined, previewWindow);
	}
}

export { UmbDocumentSaveAndPreviewWorkspaceAction as api };
