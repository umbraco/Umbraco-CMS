export class UmbData<T> {
	protected data: Array<T> = [];

	constructor(data: Array<T>) {
		this.data = data;
	}

	getData() {
		return this.data;
	}

	get total() {
		return this.data.length;
	}
}
