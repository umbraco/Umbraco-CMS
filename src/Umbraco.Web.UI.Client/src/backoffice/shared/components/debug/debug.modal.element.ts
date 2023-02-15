import { css, html, nothing, TemplateResult } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbModalHandler, UmbModalLayoutElement } from '@umbraco-cms/modal';

@customElement('umb-debug-modal-layout')
export class UmbDebugModalLayout extends UmbModalLayoutElement {
	static styles = [
		UUITextStyles,
		css`
			uui-dialog-layout {
				display: flex;
				flex-direction: column;
				height: 100%;

				padding: var(--uui-size-space-5);
				box-sizing: border-box;
			}

			uui-scroll-container {
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
				flex: 1;
			}

			uui-icon {
				vertical-align: text-top;
				color: var(--uui-color-danger);
			}

			.context {
				padding:15px 0;
				border-bottom:1px solid var(--uui-color-danger-emphasis);
			}

			h3 {
				margin-top: 0;
				margin-bottom: 0;
			}

			h3 > span {
				border-radius: var(--uui-size-4);
				background-color: var(--uui-color-danger);
				color: var(--uui-color-danger-contrast);
				padding: 8px;
				font-size: 12px;
			}
			

			ul {
				margin-top: 0;
			}
		`,
	];


	// the modal handler will be injected into the element when the modal is opened.
	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	private _handleClose() {
		/* Optional data of any type can be applied to the close method to pass it
		   to the modal parent through the onClose promise. */
		//this.modalHandler?.close('MY DATA');
		this.modalHandler?.close();
	}

	render() {
		return html`
			<uui-dialog-layout>
				<span slot="headline">
					<uui-icon name="umb:bug"></uui-icon> Debug: Contexts
				</span>
				<uui-scroll-container id="field-settings">
					${this._renderContextAliases()}
				</uui-scroll-container>
				<uui-button slot="actions" look="primary" label="Close sidebar" @click="${this._handleClose}">Close</uui-button>
			</uui-dialog-layout>
		`;
	}

	private _renderContextAliases() { 
		if(!this.data) {
			return nothing;
		}

		const aliases = [];
		for (const [alias, instance] of this.data.contexts) {
			aliases.push(
				html`
				<div class="context">
					<h3>${alias} <span>${typeof instance}</span></h3>
					${this._renderInstance(instance)}
				</div>`
			);
		}

		return aliases;
	}

	private _renderInstance(instance: any) {
		const instanceKeys: TemplateResult[] = [];

		if (typeof instance === 'function') {
			return instanceKeys.push(html`<li>Callable Function</li>`);
		} else if (typeof instance === 'object') {
			const methodNames = this.getClassMethodNames(instance);
			if (methodNames.length) {
				instanceKeys.push(
					html`
						<h4>Methods</h4>
						<ul>
							${methodNames.map((methodName) => html`<li>${methodName}</li>`)}
						</ul>
				`);
			}

			instanceKeys.push(html`<h4>Properties</h4>`);

			for (const key in instance) {
				if (key.startsWith('_')) {
					continue;
				}

				const value = instance[key];
				if (typeof value === 'string') {
					instanceKeys.push(html`<li>${key} = ${value}</li>`);
				} else {
					instanceKeys.push(html`<li>${key} Type (${typeof value})</li>`);
				}
			}
		} else {
			instanceKeys.push(html`<li>Context is a primitive with value: ${instance}</li>`);
		}

		return instanceKeys;
	}


	private getClassMethodNames(klass: any) {
		const isGetter = (x: any, name: string): boolean => !!(Object.getOwnPropertyDescriptor(x, name) || {}).get;
		const isFunction = (x: any, name: string): boolean => typeof x[name] === 'function';
		const deepFunctions = (x: any): any =>
			x !== Object.prototype &&
			Object.getOwnPropertyNames(x)
				.filter((name) => isGetter(x, name) || isFunction(x, name))
				.concat(deepFunctions(Object.getPrototypeOf(x)) || []);
		const distinctDeepFunctions = (klass: any) => Array.from(new Set(deepFunctions(klass)));

		const allMethods =
			typeof klass.prototype === 'undefined'
				? distinctDeepFunctions(klass)
				: Object.getOwnPropertyNames(klass.prototype);
		return allMethods.filter((name: any) => name !== 'constructor' && !name.startsWith('_'));
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-debug-modal-layout': UmbDebugModalLayout;
	}
}
