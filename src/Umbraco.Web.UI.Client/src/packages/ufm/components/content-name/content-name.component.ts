import type { UfmToken } from '../../plugins/marked-ufm.plugin.js';
import { UmbUfmComponentBase } from '../ufm-component-base.js';

import './content-name.element.js';

export class UmbUfmContentNameComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;

		const attributes = super.getAttributes(token.text);
		return `<ufm-content-name ${attributes}></ufm-content-name>`;
	}
}

export { UmbUfmContentNameComponent as api };
