type Writeable<T> = { -readonly [P in keyof T]: T[P] };

export class UmbValidityState implements Writeable<ValidityState> {
	badInput: boolean = false;
	customError: boolean = false;
	patternMismatch: boolean = false;
	rangeOverflow: boolean = false;
	rangeUnderflow: boolean = false;
	stepMismatch: boolean = false;
	tooLong: boolean = false;
	tooShort: boolean = false;
	typeMismatch: boolean = false;
	valid: boolean = false;
	valueMissing: boolean = false;
}
