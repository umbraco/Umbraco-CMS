import { UmbBaseController, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class WorkspaceContextCounter extends UmbBaseController {

	constructor(host: UmbControllerHost) {
		super(host);

		console.log("HELLOOOOO WORLLLDDD")
	}
}

export const api = WorkspaceContextCounter;
