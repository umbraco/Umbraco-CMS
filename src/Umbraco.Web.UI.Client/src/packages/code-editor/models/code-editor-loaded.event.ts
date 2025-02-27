export class UmbCodeEditorLoadedEvent extends Event {
	public static readonly TYPE = 'loaded';

	public constructor() {
		super(UmbCodeEditorLoadedEvent.TYPE, { bubbles: true, cancelable: false });
	}
}
