import { UMB_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbWorkspaceElement } from '../workspace.element.js';
import type { IRoutingInfo, PageComponent, UmbRouteEntry } from '@umbraco-cms/backoffice/router';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbWorkspaceSectionRouteEntry implements UmbApi, UmbRouteEntry {
	getPath(): string {
		return UMB_WORKSPACE_PATH_PATTERN.toString();
	}

	setup(element: PageComponent, info: IRoutingInfo) {
		(element as UmbWorkspaceElement).entityType = info.match.params.entityType;
	}

	destroy(): void {}
}

export { UmbWorkspaceSectionRouteEntry as api };
