export interface UmbValidator {
	/**
	 * Validate the form, will return a promise that resolves to true if what the Validator represents is valid.
	 */
	validate(): Promise<boolean>;

	/**
	 * Reset the validator to its initial state.
	 */
	reset(): void;

	/**
	 * Returns true if the validator is valid.
	 * This might represent last known state and might first be updated when validate() is called.
	 */
	isValid: boolean;

	//getMessage(): string;
	getMessages(): string[]; // Should we enable bringing multiple messages?

	destroy(): void;
}
