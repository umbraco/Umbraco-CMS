import { UmbDataTypeWorkspaceContext } from "../workspace/data-type-workspace.context.js";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbInvariantDatasetContext } from "@umbraco-cms/backoffice/workspace";

export class UmbDataTypeDatasetContext extends UmbInvariantDatasetContext<UmbDataTypeWorkspaceContext> {


	properties = this._workspace.properties

	// default data:

	constructor(host: UmbControllerHost, workspace: UmbDataTypeWorkspaceContext) {
		super(host, workspace);
	}

}
