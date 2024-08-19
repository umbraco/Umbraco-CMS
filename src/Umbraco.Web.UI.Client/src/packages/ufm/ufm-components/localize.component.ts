import type { UfmToken } from '../plugins/marked-ufm.plugin.js';
import { UmbUfmComponentBase } from './ufm-component-base.js';

export class UmbUfmLocalizeComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;
		return `<umb-localize key="${token.text}"></umb-localize>`;
	}
}

export { UmbUfmLocalizeComponent as api };
