export class UmbProgressEvent extends Event {
	public static readonly TYPE = 'progress';
	public progress: number;

	public constructor(progress: number) {
		super(UmbProgressEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.progress = progress;
	}
}
