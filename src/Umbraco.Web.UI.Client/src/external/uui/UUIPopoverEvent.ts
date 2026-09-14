import { UUIEvent } from '@umbraco-ui/uui';
import type { UUIPopoverElement } from './uui-popover.element.js';

export class UUIPopoverEvent extends UUIEvent<{}, UUIPopoverElement> {
	public static readonly CLOSE = 'close';
}
