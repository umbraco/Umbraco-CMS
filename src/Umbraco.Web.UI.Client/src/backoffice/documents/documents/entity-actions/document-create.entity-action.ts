export class CreateDocumentEntityAction {
	#host: any;

	constructor(host: any) {
		this.#host = host;
	}

	execute() {
		alert('create');
	}
}
