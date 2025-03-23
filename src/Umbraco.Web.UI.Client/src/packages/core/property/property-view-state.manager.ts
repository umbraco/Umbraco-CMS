import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbPropertyViewState extends UmbState {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyViewStateManager<
	ViewStateType extends UmbPropertyViewState = UmbPropertyViewState,
> extends UmbStateManager<ViewStateType> {
	/**
	 * Get the viewable state
	 * @returns {Observable<boolean>} True if the property is viewable
	 * @memberof UmbPropertyViewStateManager
	 */
	public readonly isViewable = this.isOn;

	/**
	 * Get the property viewable state
	 * @returns {boolean} True if the property is viewable
	 */
	public getIsViewable(): boolean {
		return this.getIsOn();
	}
}
