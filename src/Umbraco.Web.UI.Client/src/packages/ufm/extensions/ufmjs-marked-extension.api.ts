import { ufmjs } from '../plugins/marked-ufmjs.plugin.js';
import type { UmbMarkedExtensionApi } from './marked-extension.extension.js';
import type { Marked } from '@umbraco-cms/backoffice/external/marked';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUfmJsMarkedExtensionApi implements UmbMarkedExtensionApi {
	constructor(_host: UmbControllerHost, marked: Marked) {
		marked.use(ufmjs());
	}

	destroy() {}
}

export default UmbUfmJsMarkedExtensionApi;
