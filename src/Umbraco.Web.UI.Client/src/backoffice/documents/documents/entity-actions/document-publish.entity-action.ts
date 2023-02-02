export class PublishDocumentEntityAction {
	#host: any;

	constructor(host: any) {
		this.#host = host;
	}

	execute() {
		alert('publish');
	}
}
