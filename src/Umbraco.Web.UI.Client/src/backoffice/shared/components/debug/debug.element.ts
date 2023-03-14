import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing, TemplateResult } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UMB_CONTEXT_DEBUGGER_MODAL_TOKEN } from './modals/debug';
import { UmbContextDebugRequest } from '@umbraco-cms/context-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

@customElement('umb-debug')
export class UmbDebug extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#container {
				display: block;
				font-family: monospace;

				z-index: 10000;

				position: relative;
				width: 100%;
				padding: 10px 0;
			}

			uui-badge {
				cursor: pointer;
			}

			uui-icon {
				font-size: 15px;
			}

			.events {
				background-color: var(--uui-color-danger);
				color: var(--uui-color-selected-contrast);
				max-height: 0;
				transition: max-height 0.25s ease-out;
				overflow: hidden;
			}

			.events.open {
				max-height: 500px;
				overflow: auto;
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

	@property({ reflect: true, type: Boolean })
	dialog = false;

	@property()
	contexts = new Map();

	@state()
	private _debugPaneOpen = false;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		// Dispatch it
		this.dispatchEvent(
			new UmbContextDebugRequest((contexts: Map<any, any>) => {
				// The Contexts are collected
				// When travelling up through the DOM from this element
				// to the root of <umb-app> which then uses the callback prop
				// of the this event tha has been raised to assign the contexts
				// back to this property of the WebComponent
				this.contexts = contexts;
			})
		);
	}

	render() {
		if (this.enabled) {
			return this.dialog ? this._renderDialog() : this._renderPanel();
		} else {
			return nothing;
		}
	}

	private _toggleDebugPane() {
		this._debugPaneOpen = !this._debugPaneOpen;
	}

	private _openDialog() {
		this._modalContext?.open(UMB_CONTEXT_DEBUGGER_MODAL_TOKEN, {
			content: html`${this._renderContextAliases()}`,
		});
	}

	private _renderDialog() {
		return html` <div id="container">
			<uui-badge color="danger" look="primary" attention @click="${this._openDialog}">
				<uui-icon name="umb:bug"></uui-icon> Debug
			</uui-badge>
		</div>`;
	}

	private _renderPanel() {
		return html` <div id="container">
			<uui-button color="danger" look="primary" @click="${this._toggleDebugPane}">
				<uui-icon name="umb:bug"></uui-icon>
				Debug
			</uui-button>

			<div class="events ${this._debugPaneOpen ? 'open' : ''}">
				<div>
					<ul>
						${this._renderContextAliases()}
					</ul>
				</div>
			</div>
		</div>`;
	}

	private _renderContextAliases() {
		const contextsTemplates: TemplateResult[] = [];

		for (const [alias, instance] of this.contexts) {
			contextsTemplates.push(
				html` <li>
					Context: <strong>${alias}</strong>
					<em>(${typeof instance})</em>
					<ul>
						${this._renderInstance(instance)}
					</ul>
				</li>`
			);
		}

		return contextsTemplates;
	}

	private _renderInstance(instance: any) {
		const instanceTemplates: TemplateResult[] = [];

		// TODO: WB - Maybe make this a switch statement?
		if (typeof instance === 'function') {
			return instanceTemplates.push(html`<li>Callable Function</li>`);
		} else if (typeof instance === 'object') {
			const methodNames = this.getClassMethodNames(instance);
			if (methodNames.length) {
				instanceTemplates.push(
					html`
						<li>
							<strong>Methods</strong>
							<ul>
								${methodNames.map((methodName) => html`<li>${methodName}</li>`)}
							</ul>
						</li>
					`
				);
			}

			const props: TemplateResult[] = [];

			for (const key in instance) {
				if (key.startsWith('_')) {
					continue;
				}

				const value = instance[key];
				if (typeof value === 'string') {
					props.push(html`<li>${key} = ${value}</li>`);
				} else {
					props.push(html`<li>${key} <em>(${typeof value})</em></li>`);
				}
			}

			instanceTemplates.push(html`
				<li>
					<strong>Properties</strong>
					<ul>
						${props}
					</ul>
				</li>
			`);
		} else {
			instanceTemplates.push(html`<li>Context is a primitive with value: ${instance}</li>`);
		}

		return instanceTemplates;
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
