import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbStateManager, type UmbStateEntry } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyViewState extends UmbStateEntry {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyViewStateManager<
	ViewStateType extends UmbPropertyViewState = UmbPropertyViewState,
> extends UmbStateManager<ViewStateType> {
	constructor(host: UmbControllerHost) {
		super(host);
		// To avoid breaking changes in rendering this state is stopped by default. This means that properties are viewable by default.
		// We start this state in workspaces where we want to control the viewability of properties.
		this.fallbackToOn();
	}
}
