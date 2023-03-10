import { UUIEvent } from '@umbraco-ui/uui-base/lib/events';
import type { UmbRouterSlotElement } from './router-slot.element';
export class UmbRouterSlotInitEvent extends UUIEvent<never, UmbRouterSlotElement> {
	static readonly INIT = 'init';
	constructor() {
		super(UmbRouterSlotInitEvent.INIT);
	}
}
