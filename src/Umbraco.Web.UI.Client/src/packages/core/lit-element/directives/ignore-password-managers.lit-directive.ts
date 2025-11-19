import { directive, AsyncDirective, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { ElementPart } from '@umbraco-cms/backoffice/external/lit';

class UmbIgnorePasswordManagersDirective extends AsyncDirective {
	#attributes = [
		{ name: 'data-1p-ignore', value: '' },
		{ name: 'data-bwignore', value: '' },
		{ name: 'data-form-type', value: 'other' },
		{ name: 'data-lpignore', value: 'true' },
	];

	override render() {
		return nothing;
	}

	override update(part: ElementPart) {
		this.#attributes.forEach((attr) => {
			part.element.setAttribute(attr.name, attr.value);
		});
	}
}

export const umbIgnorePasswordManagers = directive(UmbIgnorePasswordManagersDirective);
