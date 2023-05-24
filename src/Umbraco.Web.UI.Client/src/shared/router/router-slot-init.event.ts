import { UUIEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbRouterSlotElement } from './router-slot.element.js';
export class UmbRouterSlotInitEvent extends UUIEvent<never, UmbRouterSlotElement> {
	static readonly INIT = 'init';
	constructor() {
		super(UmbRouterSlotInitEvent.INIT);
	}
}
