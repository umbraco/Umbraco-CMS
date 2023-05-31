// Temp mocked database
export class UmbData<T> {
	protected data: Array<T> = [];

	constructor(data: Array<T>) {
		this.data = data;
	}

	get total() {
		return this.data.length;
	}
}
