import { v4, validate } from '@umbraco-cms/backoffice/external/uuid';

export class UmbId {
	public static new() {
		return v4();
	}

	public static validate(id: string) {
		return validate(id);
	}
}
