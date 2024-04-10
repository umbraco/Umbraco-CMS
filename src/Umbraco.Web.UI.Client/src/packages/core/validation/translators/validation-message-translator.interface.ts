export interface UmbValidationMessageTranslator {
	match(message: string): boolean;
	translate(message: string): string;
}
