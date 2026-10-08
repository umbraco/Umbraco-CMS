import type { UfmToken } from '../../plugins/types.js';
import { UmbUfmComponentBase } from '../ufm-component-base.js';

import './element-name.element.js';

export class UmbUfmElementNameComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;
		const attributes = super.getAttributes(token.text);
		return `<ufm-element-name ${attributes}></ufm-element-name>`;
	}
}

export { UmbUfmElementNameComponent as api };
