export class DeleteDocumentEntityAction {
	#host: any;

	constructor(host: any) {
		this.#host = host;
	}

	execute() {
		alert('delete');
	}
}
