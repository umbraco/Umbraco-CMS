import { v4, validate } from 'uuid';

export class UmbId {
	public static new() {
		return v4();
	}

	public static validate(id: string) {
		return validate(id);
	}
}
