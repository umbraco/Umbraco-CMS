import { UMB_DOCUMENT_RECYCLE_BIN_WORKSPACE_ALIAS } from './constants.js';
import { UmbDocumentRecycleBinWorkspaceEditorElement } from './document-recycle-bin-workspace-editor.element.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbRoutableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbWorkspaceRouteManager } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentRecycleBinWorkspaceContext extends UmbControllerBase implements UmbRoutableWorkspaceContext {
	public readonly workspaceAlias = UMB_DOCUMENT_RECYCLE_BIN_WORKSPACE_ALIAS;
	public readonly routes = new UmbWorkspaceRouteManager(this);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.Document.RecycleBin');

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbDocumentRecycleBinWorkspaceEditorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					console.log('unique', unique);
				},
			},
		]);
	}

	getEntityType() {
		return 'document-recycle-bin';
	}

	destroy(): void {}
}

export { UmbDocumentRecycleBinWorkspaceContext as api };
