import type { UmbState } from './state.manager.js';
import { UmbStateManager } from './state.manager.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbReadOnlyStateManager<StateType extends UmbState> extends UmbStateManager<StateType> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
