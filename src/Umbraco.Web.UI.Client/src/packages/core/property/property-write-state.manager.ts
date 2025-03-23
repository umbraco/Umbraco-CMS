import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbPropertyWriteState extends UmbState {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyWriteStateManager<
	WriteStateType extends UmbPropertyWriteState = UmbPropertyWriteState,
> extends UmbStateManager<WriteStateType> {
	/**
	 * Get the writable state
	 * @returns {Observable<boolean>} True if the property is writable
	 * @memberof UmbPropertyWriteStateManager
	 */
	public readonly isWritable = this.isOn;

	/**
	 * Get the property writable state
	 * @returns {boolean} True if the property is writable
	 */
	public getIsWritable(): boolean {
		return this.getIsOn();
	}
}
