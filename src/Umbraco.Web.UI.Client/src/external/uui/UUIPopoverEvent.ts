import { UUIEvent, UUIPopoverElement } from '.';

export class UUIPopoverEvent extends UUIEvent<{}, UUIPopoverElement> {
	public static readonly CLOSE = 'close';
}
