/** Explains something to the user, such as why an action is unavailable. */
export interface UmbReason {
	/** A localization key or text. */
	title: string;
	/** A localization key or text. */
	detail?: string;
}
