import type { UmbRouterSlotElement } from './router-slot.element.js';
import { UUIEvent } from '@umbraco-cms/backoffice/external/uui';
export class UmbRouterSlotChangeEvent extends UUIEvent<never, UmbRouterSlotElement> {
	static readonly CHANGE = 'change';
	constructor() {
		super(UmbRouterSlotChangeEvent.CHANGE);
	}
}
