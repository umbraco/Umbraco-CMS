import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing, TemplateResult } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextDebugRequest } from '@umbraco-cms/context-api';

@customElement('umb-debug')
export class UmbDebug extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			#container {
				display: block;
				font-family: monospace;

				z-index: 10000;

				width: 100%;
				padding: 10px 0;
			}

			.events {
				background-color: var(--uui-color-danger);
				color: var(--uui-color-selected-contrast);
				height: 0;
				transition: height 0.3s ease-out;
			}

			.events.open {
				height: auto;
			}

			.events > div {
				padding: 10px;
			}

			h4 {
				margin: 0;
			}
		`,
	];

	@property({ reflect: true, type: Boolean })
	enabled = false;

	@property()
	contextAliases = new Map();

	@state()
	private _debugPaneOpen = false;

	private _toggleDebugPane() {
		this._debugPaneOpen = !this._debugPaneOpen;
	}

	connectedCallback(): void {
		super.connectedCallback();

		// Dispatch it
		this.dispatchEvent(
			new UmbContextDebugRequest((instances: Map<any, any>) => {
				console.log('I have contexts now', instances);

				this.contextAliases = instances;
			})
		);
	}

	render() {
		if (this.enabled) {
			return html`
				<div id="container">
					<uui-button color="danger" look="primary" @click="${this._toggleDebugPane}">
						<uui-icon name="umb:bug"></uui-icon>
						Debug
					</uui-button>

					<div class="events ${this._debugPaneOpen ? 'open' : ''}">
						<div>
							<h4>Context Aliases to consume</h4>
							<ul>
								${this._renderContextAliases()}
							</ul>
						</div>
					</div>
				</div>
			`;
		}

		return nothing;
	}

	private _renderContextAliases() {
		const aliases = [];

		for (const [alias, instance] of this.contextAliases) {
			aliases.push(
				html` <li>
					Context: <strong>${alias}</strong>
					<span>(${typeof instance})</span>
					<ul>
						${this._renderInstance(instance)}
					</ul>
				</li>`
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
				instanceKeys.push(html`<li>Methods - ${methodNames.join(', ')}</li>`);
			}

			for (const key in instance) {
				if (key.startsWith('_')) {
					continue;
				}
				// Goes KABOOM - if try to loop over the class/object
				// instanceKeys.push(html`<li>${key} = ${instance[key]}</li>`);

				// console.log(`key: ${key} = ${value} TYPEOF: ${typeof value}`);

				const value = instance[key];
				if (typeof value === 'string') {
					instanceKeys.push(html`<li>${key} = ${value}</li>`);
				} else {
					instanceKeys.push(html`<li>${key} (${typeof value})</li>`);
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
		'umb-debug': UmbDebug;
	}
}
