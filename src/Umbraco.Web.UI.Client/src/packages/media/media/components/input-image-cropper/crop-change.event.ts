export class UmbImageCropChangeEvent extends Event {
	public static readonly TYPE = 'imagecrop-change';

	public constructor(args?: EventInit) {
		// mimics the native change event
		super(UmbImageCropChangeEvent.TYPE, { bubbles: false, composed: false, cancelable: false, ...args });
	}
}
