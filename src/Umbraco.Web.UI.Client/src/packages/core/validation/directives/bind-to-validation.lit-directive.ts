import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { AsyncDirective, directive, nothing, type ElementPart } from '@umbraco-cms/backoffice/external/lit';
import type { UmbFormControlMixinInterface } from '@umbraco-cms/backoffice/validation';
import { UmbFormControlValidator } from '@umbraco-cms/backoffice/validation';

/**
 * The `bind to validation` directive connects the Form Control Element to closets Validation Context.
 */
class UmbBindToValidationDirective extends AsyncDirective {
	#host?: UmbControllerHost;
	#dataPath?: string;
	#el?: UmbFormControlMixinInterface<unknown>;
	#validator?: UmbFormControlValidator;

	// For Directives their arguments have to be defined on the Render method, despite them, not being used by the render method. In this case they are used by the update method.
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	override render(host: UmbControllerHost, dataPath: string) {
		return nothing;
	}

	override update(part: ElementPart, args: [UmbControllerHost, string]) {
		if (this.#el !== part.element) {
			this.#host = args[0];
			this.#dataPath = args[1];
			if (!this.#host || !this.#dataPath || !part.element) return nothing;
			this.#el = part.element as UmbFormControlMixinInterface<unknown>;
			this.#validator = new UmbFormControlValidator(this.#host, this.#el, this.#dataPath);
		}
		return nothing;
	}

	override disconnected() {
		if (this.#validator) {
			this.#validator?.destroy();
			this.#validator = undefined;
		}
		this.#el = undefined;
	}

	/*override reconnected() {
	}*/
}

/**
 * @description
 * A Lit directive, which binds the validation state of the element to the Validation Context.
 * @example:
 * ```js
 * html`<input ${umbBindToValidation('$.headline')}>`;
 * ```
 */
export const umbBindToValidation = directive(UmbBindToValidationDirective);

//export type { UmbFocusDirective };
