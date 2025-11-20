import { directive, Directive, noChange, nothing, PartType } from '@umbraco-cms/backoffice/external/lit';
import type { ElementPart, PartInfo } from '@umbraco-cms/backoffice/external/lit';

/**
 * @description The directive applies attributes to ignore password managers on the given element.
 */
class UmbIgnorePasswordManagersDirective extends Directive {
	#attributes = [
		{ name: 'data-1p-ignore', value: '' }, // 1Password
		{ name: 'data-bwignore', value: '' }, // Bitwarden
		{ name: 'data-form-type', value: 'other' }, // Dashlane
		{ name: 'data-lpignore', value: 'true' }, // LastPass
	];

	constructor(partInfo: PartInfo) {
		super(partInfo);
		if (partInfo.type != PartType.ELEMENT) {
			throw new Error('The `umbIgnorePasswordManagers` directive can only be used in element parts');
		}
	}

	override render() {
		return nothing;
	}

	override update(part: ElementPart) {
		this.#attributes.forEach((attr) => {
			part.element.setAttribute(attr.name, attr.value);
		});
		return noChange;
	}
}

/**
 * @description A Lit directive which applies password manager ignore attributes to an element.
 * @example html`<div ${umbIgnorePasswordManagers()}></div>`
 */
export const umbIgnorePasswordManagers = directive(UmbIgnorePasswordManagersDirective);
