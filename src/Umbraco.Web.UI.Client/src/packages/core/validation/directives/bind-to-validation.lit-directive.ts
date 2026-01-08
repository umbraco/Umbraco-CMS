import type { UmbFormControlMixinInterface } from '../mixins/index.js';
import { UmbBindServerValidationToFormControl, UmbFormControlValidator } from '../controllers/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { AsyncDirective, directive, nothing, type ElementPart } from '@umbraco-cms/backoffice/external/lit';

/**
 * The `bind to validation` directive connects the Form Control Element to closets Validation Context.
 */
class UmbBindToValidationDirective extends AsyncDirective {
	#host?: UmbControllerHost;
	#dataPath?: string;
	#el?: UmbFormControlMixinInterface<unknown>;
	#validator?: UmbFormControlValidator;
	#msgBinder?: UmbBindServerValidationToFormControl;

	// For Directives their arguments have to be defined on the Render method, despite them, not being used by the render method. In this case they are used by the update method.
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	override render(host: UmbControllerHost, dataPath?: string, value?: unknown) {
		return nothing;
	}

	override update(part: ElementPart, args: Parameters<typeof this.render>) {
		if (!part.element) return nothing;
		if (this.#el !== part.element || this.#host !== args[0] || this.#dataPath !== args[1]) {
			this.#host = args[0];
			this.#dataPath = args[1];
			this.#el = part.element as UmbFormControlMixinInterface<unknown>;

			if (!this.#msgBinder && this.#dataPath) {
				this.#msgBinder = new UmbBindServerValidationToFormControl(this.#host, this.#el as any, this.#dataPath);
			}

			this.#validator = new UmbFormControlValidator(this.#host, this.#el, this.#dataPath);
		}

		// If we have a msgBinder, then lets update the value no matter the other conditions.
		if (this.#msgBinder) {
			this.#msgBinder.value = args[2];
		}

		return nothing;
	}

	override disconnected() {
		if (this.#validator) {
			this.#validator?.destroy();
			this.#validator = undefined;
		}
		if (this.#msgBinder) {
			this.#msgBinder.destroy();
			this.#msgBinder = undefined;
		}
		this.#el = undefined;
	}

	/*override reconnected() {
	}*/
}

/**
 * @description
 * A Lit directive, which binds the validation state of the element to the Validation Context.
 *
 * The minimal binding can be established by just providing a host element:
 * @example:
 * ```js
 * html`<input ${umbBindToValidation(this)}>`;
 * ```
 * But can be extended with a dataPath, which is the path to the data holding the value. (if no server validation in this context, then this can be fictive and is then just used internal for remembering the validation state despite the element begin removed from the DOM.)
 * @example:
 * ```js
 * html`<input ${umbBindToValidation(this, '$.headline')}>`;
 * ```
 *
 * Additional the value can be provided, which is then used to remove a server validation state, if the value is changed.
 * @example:
 * ```js
 * html`<input ${umbBindToValidation(this, '$.headline', this.headlineValue)}>`;
 * ```
 */
export const umbBindToValidation = directive(UmbBindToValidationDirective);

//export type { UmbFocusDirective };
