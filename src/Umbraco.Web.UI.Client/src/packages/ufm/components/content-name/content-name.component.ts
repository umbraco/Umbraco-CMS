import type { UfmToken } from '../../plugins/marked-ufm.plugin.js';
import { UmbUfmComponentBase } from '../ufm-component-base.js';

import './content-name.element.js';

export class UmbUfmContentNameComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;

		if (token.prefix === '~') {
			/*
			 * @deprecated since version 15.0-rc3
			 */
			console.warn(`Please update your UFM syntax from \`${token.raw}\` to \`{umbContentName:${token.text}}\`.`);
		}

		const attributes = super.getAttributes(token.text);
		return `<ufm-content-name ${attributes}></ufm-content-name>`;
	}
}

export { UmbUfmContentNameComponent as api };
