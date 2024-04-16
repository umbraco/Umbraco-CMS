export interface UmbValidationMessageTranslator {
	translate(message: string): undefined | string;
}
