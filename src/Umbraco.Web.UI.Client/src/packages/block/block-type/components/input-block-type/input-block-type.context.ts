import { UmbBlockTypeBase } from '../../types.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockTypeInputContext<BlockType extends UmbBlockTypeBase = UmbBlockTypeBase> extends UmbBaseController {
	#types = new UmbArrayState<BlockType>([], (type) => type.contentElementTypeKey);
	types = this.#types.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'blockTypeInput');
	}

	create() {
		alert('create');
	}

	requestRemoveItem(contentTypeKey: string) {
		alert('request remove ' + contentTypeKey);
	}
}
