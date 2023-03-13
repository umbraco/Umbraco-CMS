import { UUIEvent } from '@umbraco-ui/uui-base/lib/events';
import type { UmbRouterSlotElement } from './router-slot.element';
export class UmbRouterSlotChangeEvent extends UUIEvent<never, UmbRouterSlotElement> {
	static readonly CHANGE = 'change';
	constructor() {
		super(UmbRouterSlotChangeEvent.CHANGE);
	}
}
