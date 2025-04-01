import { UmbStateManager, type UmbStateEntry } from '@umbraco-cms/backoffice/utils';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbPropertyWriteState extends UmbStateEntry {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyWriteStateManager<
	WriteStateType extends UmbPropertyWriteState = UmbPropertyWriteState,
> extends UmbStateManager<WriteStateType> {
	constructor(host: UmbControllerHost) {
		super(host);
		// To avoid breaking changes in rendering this state is stopped by default. This means that properties are viewable by default.
		// We start this state in workspaces where we want to control the writability of properties.
		this.stop();
	}

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
