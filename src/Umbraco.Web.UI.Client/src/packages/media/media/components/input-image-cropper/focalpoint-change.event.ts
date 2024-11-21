import type { UmbFocalPointModel } from '../../types.js';

export class UmbFocalPointChangeEvent extends Event {
	public static readonly TYPE = 'focalpoint-change';

	public focalPoint: UmbFocalPointModel;

	public constructor(focalPoint: UmbFocalPointModel, args?: EventInit) {
		// mimics the native change event
		super(UmbFocalPointChangeEvent.TYPE, { bubbles: false, composed: false, cancelable: false, ...args });
		this.focalPoint = focalPoint;
	}
}
